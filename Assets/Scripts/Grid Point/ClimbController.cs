using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Climbing
{
    public class ClimbController : MonoBehaviour
    {
        #region Variables
        public bool climbing;
        bool initClimb;
        bool waitToStartClimb;

        PlayerInput input;
        Animator anim;
        ClimbIK ik;
        MotionController mc;
        Transform modelTransform;

        ClimbObjManager currentClimbObjManager;
        Point targetPoint;
        Point currentPoint;
        Point previousPoint;
        Neighbor neighbor;
        ConnectionType currentConnectionType;

        ClimbStates climbState;
        ClimbStates targetState;
        
        public enum ClimbStates
        {
            onPoint,
            betweenPoints,
            inTransition
        }

        #region MotionCurveControll
        CurvesHolder curvesHolder;
        BezierCurve leapCurveHorizontal;
        BezierCurve leapCurveVertical;
        BezierCurve dismountCurve;
        BezierCurve mountCurve;
        BezierCurve currentCurve;

        Vector3 _startPosition;
        Vector3 _endPosition;
        float _distance;
        float _time;
        bool initTransit;
        bool rootReached;
        #endregion

        // IK debug variables. Defined but not used
        // TODO: can rewrite to #define + #if blocks for toggling debug
        bool ikLandSideReached;
        bool ikFollowSideReached;

        bool lockInput;
        Vector3 inputDirection;
        Vector3 targetPosition;
        
        // For coping with animation that is not root at the character's hip
        public Vector3 rootOffset = new Vector3(0, -0.86f, 0);
        public float speed_linear = 3f;
        public float speed_leap = 2f;

        public AnimationCurve a_jumpingCurve;
        public AnimationCurve a_mountCurve;
        public bool isRootMovementEnbled;
        float _rootMovementStartMaxDelay = 0.25f;
        float _rootMovementTime;
        #endregion

        void SetCurveReferences()
        {
            GameObject curvesHolderPF = Resources.Load("CurvesHolder") as GameObject;
            GameObject curvesHoldeGO = Instantiate(curvesHolderPF) as GameObject;

            curvesHolder = curvesHoldeGO.GetComponent<CurvesHolder>();

            leapCurveHorizontal = curvesHolder.ReturnCurve(CurveType.horizontal);
            leapCurveVertical = curvesHolder.ReturnCurve(CurveType.vertical);
            dismountCurve = curvesHolder.ReturnCurve(CurveType.dismount);
            mountCurve = curvesHolder.ReturnCurve(CurveType.mount);

        }

        void Start()
        {
            input = FindObjectOfType<PlayerInput>();
            anim = GetComponentInChildren<Animator>();
            mc = GetComponent<MotionController>();
            ik = GetComponentInChildren<ClimbIK>();
            modelTransform = GameObject.Find("PlayerModel").transform;
            SetCurveReferences();
        }

        void FixedUpdate()
        {
            if (climbing)
            {
                if (waitToStartClimb)
                {
                    InitClimbing();
                    HandleMount();
                }
                else
                {
                    HandleClimbing();
                    InitFallOff();
                }
            }
            else
            {
                if (initClimb)
                {
                    transform.parent = null;
                    initClimb = false;
                }

                //if (input.grabBtnDown)
                if (mc.IsInGrabState())
                {
                    LookForClimbSpot();
                }
            }
        }

        void LookForClimbSpot()
        {
            // TODO: spaghetti code here, need refactoring 
            // (adjustable player center or a game object as player center origin)
            float maxDistance = 5;

            Vector3 playerCenter = transform.position + new Vector3(0, 0.9f, 0);
            Ray ray = new Ray(playerCenter, modelTransform.forward);
            Debug.DrawLine(playerCenter, playerCenter + (maxDistance * modelTransform.forward), Color.cyan);
            RaycastHit hitInfo;
            LayerMask mask = 1 << 30; // dummy, set it later to grid manger layer


            if (Physics.Raycast(ray, out hitInfo, maxDistance, mask))
            {
                //Debug.Log("Find Climbable!");
                ClimbObjManager gm = hitInfo.transform.GetComponentInParent<ClimbObjManager>();
                if (gm)
                {
                    Point closestPoint = gm.ReturnClosest(playerCenter);
                    bool faceTowardsPoint = Vector3.Angle(closestPoint.transform.forward, modelTransform.forward) <= 80;
                    bool notInsideWall = Vector3.Angle(closestPoint.transform.forward,
                        ((closestPoint.transform.position + 0.1f * closestPoint.transform.forward) - playerCenter).normalized) < 90;

                    //Debug.DrawRay(playerCenter, closestPoint.transform.forward, Color.red);
                    //Debug.DrawRay(playerCenter, (closestPoint.transform.position - (playerCenter)).normalized, Color.yellow);

                    //Debug.Log("faceTowardsPoint:" + faceTowardsPoint + " | " + "notInsideWall:" + notInsideWall);

                    if (!(notInsideWall && faceTowardsPoint))
                    {
                        // just for precaution
                        closestPoint = null;
                        return;
                    }

                    // since point is attached to hip position for ik, parant would be the actual point position
                    float distanceToPoint = Vector3.Distance(playerCenter, closestPoint.transform.parent.position);
                    //Debug.Log(distanceToPoint);
                    if (distanceToPoint < 1.5)
                    {
                        // Enter mount animation, revoke user control
                        mc.SetStateOnClimbGrid();
                        lockInput = true;
                        climbing = true;

                        currentClimbObjManager = gm;
                        targetPoint = closestPoint;
                        targetPosition = closestPoint.transform.position;
                        currentPoint = closestPoint;
                        targetState = ClimbStates.onPoint;

                        // animation params
                        // Can consider different mount initiation in the future
                        //if (mc.grounded)
                        //{
                        //    anim.CrossFade("Ground_Mount", 0.2f);
                        //}
                        //else
                        //{
                        //    anim.CrossFade("Air_Mount", 0.1f);
                        //}
                        // Can also consider preserving pre-mount velocity 
                        anim.CrossFade("Mount", 0.1f);
                        anim.SetBool("GP_OnGrid", true);

                        waitToStartClimb = true;
                    }
                }
            }
        }

        void HandleClimbing()
        {
            if (!lockInput)
            {
                float hori = input.goingRight;
                float verti = input.goingForward;

                inputDirection = ConvertToInputDirection(hori, verti);

                // Should be safe as ConvertToInputDirection handles floating point comparison already
                if (inputDirection != Vector3.zero)
                {
                    switch (climbState)
                    {
                        case ClimbStates.onPoint:
                            HandleOnPoint(inputDirection);
                            break;
                        case ClimbStates.betweenPoints:
                            HandleBetweenPoints(inputDirection);
                            break;
                    }
                }

                // Temporary snap player to point for moving climbable objects
                transform.parent = currentPoint.transform.parent;

                if (climbState == ClimbStates.onPoint)
                {
                    ik.UpdateAllTargetPositions(currentPoint);
                    ik.ForceUpdateAllHelpers();
                }
            }
            else
            {
                InitTransition(inputDirection);
            }
        }

        void HandleOnPoint(Vector3 moveDirection)
        {
            neighbor = currentClimbObjManager.GetNeighborForDirection(moveDirection, currentPoint);

            if (neighbor != null)
            {
                targetPoint = neighbor.target;
                previousPoint = currentPoint;
                climbState = ClimbStates.inTransition;
                UpdateConnectionTransitionByType(neighbor, moveDirection);
                lockInput = true;
            }
        }

        void HandleBetweenPoints(Vector3 moveDirection)
        {
            Neighbor n = targetPoint.ReturnNeighbor(previousPoint);

            if (n != null)
            {
                if (moveDirection == targetPoint.ReturnNeighbor(previousPoint).direction)
                {
                    targetPoint = previousPoint;
                }
                else if (moveDirection == previousPoint.ReturnNeighbor(targetPoint).direction)
                {
                    // Do nothing
                }
                else
                {
                    return;
                }
            }
            else
            {
                targetPoint = currentPoint;
            }

            targetPosition = targetPoint.transform.position + rootOffset;
            climbState = ClimbStates.inTransition;
            targetState = ClimbStates.onPoint;
            previousPoint = currentPoint;
            lockInput = true;
            anim.SetBool("GP_Move", false);
        }

        void UpdateConnectionTransitionByType(Neighbor n, Vector3 moveDirection)
        {
            Vector3 desiredPosition = Vector3.zero;
            currentConnectionType = n.cType;

            Vector3 direction = (targetPoint.transform.position - currentPoint.transform.position).normalized;
            TransitionType tType;
            switch (currentConnectionType)
            {
                case ConnectionType.inBetween:
                    // mid point
                    float distance = Vector3.Distance(currentPoint.transform.position, targetPoint.transform.position);
                    desiredPosition = currentPoint.transform.position + (direction * (distance / 2)) + rootOffset;
                    targetState = ClimbStates.betweenPoints;
                    tType = GetTransitionType(moveDirection, false);
                    PlayAnimation(tType);
                    break;
                case ConnectionType.leap:
                    desiredPosition = targetPoint.transform.position;
                    targetState = ClimbStates.onPoint;
                    tType = GetTransitionType(moveDirection, true);
                    PlayAnimation(tType, true);
                    break;
                case ConnectionType.dismount:
                    desiredPosition = targetPoint.transform.position;
                    anim.SetInteger("GP_ClimbType", 20);
                    anim.SetBool("GP_Move", true);
                    break;
            }

            targetPosition = desiredPosition;
        }

        Vector3 ConvertToInputDirection(float horizontalInput, float verticalInput)
        {
            float x, y;
            if (Mathf.Approximately(horizontalInput, 0))
            {
                x = 0;
            }
            else
            {
                x = horizontalInput > 0 ? 1 : -1;
            }

            if (Mathf.Approximately(verticalInput, 0))
            {
                y = 0;
            }
            else
            {
                y = verticalInput > 0 ? 1 : -1;
            }

            return new Vector3(x, y);
        }

        void InitTransition(Vector3 moveDirection)
        {
            switch (currentConnectionType)
            {
                case ConnectionType.inBetween:
                    UpdateLinearVariable(moveDirection);
                    LinearRootMovement();
                    LinearLerpIKInitiation();
                    WrapUp();
                    break;
                case ConnectionType.leap:
                    UpdateLeapVariables(moveDirection);
                    LeapRootMovement();
                    HandleDirectIK();
                    WrapUp(true);
                    break;
                case ConnectionType.dismount:
                    UpdateDismountVariables();
                    DismountRootMovement();
                    HandleDismountIK();
                    WrapUpDismount();
                    break;
            }
        }

        #region Linear
        void UpdateLinearVariable(Vector3 moveDirection)
        {
            if (!initTransit)
            {
                initTransit = true;
                isRootMovementEnbled = true;
                rootReached = false;
                ikFollowSideReached = false;
                ikLandSideReached = false;
                _time = 0;
                _startPosition = transform.position;
                _endPosition = targetPosition + rootOffset;
                Vector3 directionToPoint = (_endPosition - _startPosition).normalized;

                bool isMidPoint = (targetState == ClimbStates.betweenPoints);
                if (isMidPoint)
                {
                    // Adjust model root to better fit animation
                    Vector3 backSwing = modelTransform.forward * -0.05f;
                    _endPosition += backSwing;
                }

                _distance = Vector3.Distance(_endPosition, _startPosition);

                InitIK(moveDirection, !isMidPoint);
            }
        }

        void LinearRootMovement()
        {
            float speed = speed_linear * Time.deltaTime;
            float lerpProgress = speed / _distance;
            _time += lerpProgress;

            if (_time > 1)
            {
                _time = 1;
                rootReached = true;
            }

            Vector3 currentPosition = Vector3.Lerp(_startPosition, targetPosition, _time);
            transform.position = currentPosition;

            HandleRotation();
        }

        void LinearLerpIKInitiation()
        {
            float speed = speed_linear * Time.deltaTime;
            float lerpSpeed = speed / _distance;

            _ikInitiationTime += lerpSpeed * 2;
            
            if (_ikInitiationTime > 1)
            {
                _ikInitiationTime = 1;
                ikLandSideReached = true;
            }

            Vector3 ikInitiationPosition = Vector3.Lerp(_ikStartPositions[0], _ikEndPositions[0], _ikInitiationTime);
            ik.UpdateSingleTargetPosition(ikInitiation, ikInitiationPosition);

            _ikWrapUpTime += lerpSpeed * 2;
            if (_ikWrapUpTime > 1)
            {
                _ikWrapUpTime = 1;
                ikFollowSideReached = true;
            }

            Vector3 ikWrapUpPosition = Vector3.Lerp(_ikStartPositions[1], _ikEndPositions[1], _ikWrapUpTime);
            ik.UpdateSingleTargetPosition(ikWrapUp, ikWrapUpPosition);
        }
        #endregion

        #region Leap
        void UpdateLeapVariables(Vector3 moveDirection)
        {
            if (!initTransit)
            {
                initTransit = true;
                isRootMovementEnbled = false;
                rootReached = false;
                ikFollowSideReached = false;
                ikLandSideReached = false;
                _time = 0;
                _rootMovementTime = 0;
                _startPosition = transform.position;
                _endPosition = targetPosition + rootOffset;

                bool isJumpVertical = (Mathf.Abs(moveDirection.y) > 0.1f);
                currentCurve = isJumpVertical ? leapCurveVertical : leapCurveHorizontal;
                currentCurve.transform.rotation = currentPoint.transform.rotation;

                if (isJumpVertical)
                {
                    if (!(moveDirection.y > 0))
                    {
                        // flip movement curve
                        currentCurve.transform.eulerAngles = new Vector3(180, 180);
                    }
                }
                else
                {
                    if (!(moveDirection.x > 0))
                    {
                        // flip movement curve
                        currentCurve.transform.eulerAngles = new Vector3(0, -180);
                    }
                }

                BezierPoint[] trailPoints = currentCurve.GetAnchorPoints();
                trailPoints[0].transform.position = _startPosition;
                trailPoints[trailPoints.Length - 1].transform.position = _endPosition;

                InitIKDirect(inputDirection);
            }
        }

        void LeapRootMovement()
        {
            if (isRootMovementEnbled)
            {
                _time += Time.deltaTime * speed_leap;
            }
            else
            {
                if (_rootMovementTime < _rootMovementStartMaxDelay)
                {
                    _rootMovementTime += Time.deltaTime;
                }
                else
                {
                    isRootMovementEnbled = true;
                }
            }

            if (_time > 0.95f)
            {
                _time = 1;
                rootReached = true;
            }

            UpdateAllIKWeight(_time, a_jumpingCurve);

            Vector3 onCurvePosition = currentCurve.GetPointAt(_time);
            transform.position = onCurvePosition;

            HandleRotation();
        }

        void HandleDirectIK()
        {
            if (Mathf.Approximately(inputDirection.y, 0))
            {
                LeapLerpIKInitiation();
                LeapLerpIKWrapup();
            }
            else
            {
                LeapLerpHandsIK();
                LeapLerpFeetIK();
            }
        }

        void LeapLerpHandsIK()
        {
            if (isRootMovementEnbled)
                _ikInitiationTime += Time.deltaTime * 5;

            if (_ikInitiationTime > 1)
            {
                _ikInitiationTime = 1;
                ikLandSideReached = true;
            }

            Vector3 lhPosition = Vector3.Lerp(_ikStartPositions[0], _ikEndPositions[0], _ikInitiationTime);
            ik.UpdateSingleTargetPosition(AvatarIKGoal.LeftHand, lhPosition);

            Vector3 rhPosition = Vector3.Lerp(_ikStartPositions[2], _ikEndPositions[2], _ikInitiationTime);
            ik.UpdateSingleTargetPosition(AvatarIKGoal.RightHand, rhPosition);
        }

        void LeapLerpFeetIK()
        {
            if (isRootMovementEnbled)
                _ikWrapUpTime += Time.deltaTime * 5;

            if (_ikWrapUpTime > 1)
            {
                _ikWrapUpTime = 1;
                ikFollowSideReached = true;
            }

            Vector3 lfPosition = Vector3.Lerp(_ikStartPositions[1], _ikEndPositions[1], _ikWrapUpTime);
            ik.UpdateSingleTargetPosition(AvatarIKGoal.LeftFoot, lfPosition);

            Vector3 rfPosition = Vector3.Lerp(_ikStartPositions[3], _ikEndPositions[3], _ikWrapUpTime);
            ik.UpdateSingleTargetPosition(AvatarIKGoal.RightFoot, rfPosition);

        }

        void LeapLerpIKInitiation()
        {
            if (isRootMovementEnbled)
                _ikInitiationTime += Time.deltaTime * 3.2f;

            if (_ikInitiationTime > 1)
            {
                _ikInitiationTime = 1;
                ikLandSideReached = true;
            }

            Vector3 landPosition = Vector3.Lerp(_ikStartPositions[0], _ikEndPositions[0], _ikInitiationTime);
            ik.UpdateSingleTargetPosition(ikInitiation, landPosition);

            Vector3 followPosition = Vector3.Lerp(_ikStartPositions[1], _ikEndPositions[1], _ikInitiationTime);
            ik.UpdateSingleTargetPosition(ikWrapUp, followPosition);
        }

        void LeapLerpIKWrapup()
        {
            if (isRootMovementEnbled)
                _ikWrapUpTime += Time.deltaTime * 2.6f;

            if (_ikWrapUpTime > 1)
            {
                _ikWrapUpTime = 1;
                ikFollowSideReached = true;
            }

            Vector3 landPosition = Vector3.Lerp(_ikStartPositions[2], _ikEndPositions[2], _ikWrapUpTime);
            ik.UpdateSingleTargetPosition(ik.ReturnMirrorIKGoal(ikInitiation), landPosition);

            Vector3 followPosition = Vector3.Lerp(_ikStartPositions[3], _ikEndPositions[3], _ikWrapUpTime);
            ik.UpdateSingleTargetPosition(ik.ReturnMirrorIKGoal(ikWrapUp), followPosition);

        }
        #endregion

        #region Mount
        void InitClimbing()
        {
            if (!initClimb)
            {
                initClimb = true;

                // IK component initiation
                if (ik != null)
                {
                    ik.UpdateAllPointsToTarget(targetPoint);
                    ik.UpdateAllTargetPositions(targetPoint);
                    ik.ForceUpdateAllHelpers();
                }

                currentConnectionType = ConnectionType.leap;
                targetState = ClimbStates.onPoint;
            }
        }

        void HandleMount()
        {
            if (!initTransit)
            {
                initTransit = true;
                isRootMovementEnbled = false;
                ikFollowSideReached = false;
                ikLandSideReached = false;
                _time = 0;
                _startPosition = transform.position;
                _endPosition = targetPosition + rootOffset; // 

                currentCurve = mountCurve;
                currentCurve.transform.rotation = targetPoint.transform.rotation;
                BezierPoint[] trailPoints = currentCurve.GetAnchorPoints();
                trailPoints[0].transform.position = _startPosition;
                trailPoints[trailPoints.Length - 1].transform.position = _endPosition;
            }

            if (isRootMovementEnbled)
            {
                _time += Time.deltaTime * 2.5f;
            }

            // Snap to end to prevent undesirable boundary behavior
            if (_time >= 0.99f)
            {
                _time = 1;
                isRootMovementEnbled = false;
                waitToStartClimb = false;
                lockInput = false;
                initTransit = false;
                ikLandSideReached = false;
                climbState = targetState;
            }

            //Debug.Log(_time);
            Vector3 onCurvePosition = currentCurve.GetPointAt(_time);
            transform.position = onCurvePosition;

            // IK application weight
            UpdateAllIKWeight(_time, a_mountCurve);

            HandleRotation();
        }
        #endregion

        #region Dismount
        void UpdateDismountVariables()
        {
            if (!initTransit)
            {
                initTransit = true;
                isRootMovementEnbled = false;
                rootReached = false;
                ikLandSideReached = false;
                ikFollowSideReached = false;
                _time = 0;
                _rootMovementTime = 0;
                _startPosition = transform.position;
                _endPosition = targetPosition;

                currentCurve = dismountCurve;
                BezierPoint[] trailPoints = currentCurve.GetAnchorPoints();
                trailPoints[0].transform.position = _startPosition;
                trailPoints[trailPoints.Length - 1].transform.position = _endPosition;

                _ikInitiationTime = 0;
                _ikWrapUpTime = 0;
            }
        }

        void DismountRootMovement()
        {
            if (isRootMovementEnbled)
            {
                _time += Time.deltaTime / 2;
            }

            if (_time >= 0.99f)
            {
                _time = 1;
                rootReached = true;
            }

            Vector3 onCurvePosition = currentCurve.GetPointAt(_time);
            transform.position = onCurvePosition;
        }

        void HandleDismountIK()
        {
            if (isRootMovementEnbled)
            {
                _ikInitiationTime += Time.deltaTime * 3;
            }

            _ikWrapUpTime += Time.deltaTime * 2;
            UpdateDismountIKWeight(_ikInitiationTime, _ikWrapUpTime, 1, 0);
        }

        void UpdateDismountIKWeight(float handsProgress, float feetProgress, float startWeight, float endWeight)
        {
            // Gradually disable IK weights
            float newHandsProgress = handsProgress * 3;
            if (newHandsProgress > 1)
            {
                newHandsProgress = 1;
                ikLandSideReached = true;
            }

            float handsWeight = Mathf.Lerp(startWeight, endWeight, newHandsProgress);
            ik.UpdateSingleIKWeight(AvatarIKGoal.LeftHand, handsWeight);
            ik.UpdateSingleIKWeight(AvatarIKGoal.RightHand, handsWeight);

            // Since feet finish animation slower, use a lower multiplier
            float newFeetProgress = feetProgress;
            if (newFeetProgress > 1)
            {
                newFeetProgress = 1;
                ikFollowSideReached = true;
            }

            float feetWeight = Mathf.Lerp(startWeight, endWeight, newFeetProgress);
            ik.UpdateSingleIKWeight(AvatarIKGoal.LeftFoot, feetWeight);
            ik.UpdateSingleIKWeight(AvatarIKGoal.RightFoot, feetWeight);
        }

        void WrapUpDismount()
        {
            if (rootReached)
            {
                climbing = false;
                initTransit = false;
                //mc.SetStateOnClimbGrid2();
                mc.SetStateNormal();
                anim.SetBool("GP_OnGrid", false);
                anim.SetBool("GP_Move", false);
            }
        }
        #endregion

        #region FallOff
        void InitFallOff()
        {
            //if (climbState == ClimbStates.onPoint)
            {
                if (input.releaseBtnDown)
                {
                    climbing = false;
                    initTransit = false;
                    ik.SetAllIKWeights(0);
                    //mc.SetStateOnClimbGrid2();
                    mc.SetStateNormal();
                    anim.SetBool("GP_OnGrid", false);
                    anim.SetBool("GP_Move", false);
                    anim.SetBool("grounded", false);
                }
            }
        }
        #endregion

        #region MotionCommonUtil
        bool waitForWrapUp;

        void WrapUp(bool isDirect = false)
        {
            if (rootReached)
            {
                if (!anim.GetBool("GP_Leap"))
                {
                    if (!waitForWrapUp)
                    {
                        StartCoroutine(WrapUpTransition(0.05f));
                        waitForWrapUp = true;
                    }
                }
            }
        }

        IEnumerator WrapUpTransition(float t)
        {
            yield return new WaitForSeconds(t);
            climbState = targetState;

            if (climbState == ClimbStates.onPoint)
            {
                currentPoint = targetPoint;
            }

            initTransit = false;
            lockInput = false;
            inputDirection = Vector3.zero;
            waitForWrapUp = false; // double set here to make sure
        }

        void HandleRotation()
        {
            Vector3 targetDirection = targetPoint.transform.forward;
            
            // Cope with malformed point, just for precaution
            if (targetDirection == Vector3.zero)
            {
                targetDirection = modelTransform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, Time.deltaTime * 5);
        }
        #endregion

        #region Animation
        TransitionType GetTransitionType(Vector3 moveDirection, bool isJump)
        {
            if (!isJump)
            {
                if (Mathf.Abs(moveDirection.y) > 0)
                {
                    return TransitionType.moveVertical;
                }
                else
                {
                    return TransitionType.moveHorizontal;
                }
            }

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.y) * Mathf.Rad2Deg;
            if ((targetAngle < 22.5f) && (targetAngle > -22.5f))
            {
                return TransitionType.leapUp;
            }
            else if ((targetAngle < 180 + 22.5f) && (targetAngle > 180 - 22.5f))
            {
                return TransitionType.leapDown;
            }
            else if ((targetAngle < 90 + 22.5f) && (targetAngle > 90 - 22.5f))
            {
                return TransitionType.leapRight;
            }
            else if ((targetAngle < -90 + 22.5f) && (targetAngle > -90 - 22.5f))
            {
                return TransitionType.leapLeft;
            }

            if (Mathf.Abs(moveDirection.y) > Mathf.Abs(moveDirection.x))
            {
                if (moveDirection.y < 0)
                {
                    return TransitionType.leapDown;
                }
                else
                {
                    return TransitionType.leapUp;
                }
            }

            Debug.LogWarning("Unexpected fall through happened at GetTransitionType");
            return default(TransitionType);
        }

        void PlayAnimation(TransitionType tType, bool jump = false)
        {
            int animationLabel = 0;

            switch (tType)
            {
                case TransitionType.leapUp:
                    animationLabel = 0;
                    break;
                case TransitionType.leapDown:
                    animationLabel = 1;
                    break;
                case TransitionType.leapRight:
                    animationLabel = 2;
                    break;
                case TransitionType.leapLeft:
                    animationLabel = 3;
                    break;
                case TransitionType.moveHorizontal:
                    animationLabel = 5;
                    break;
                case TransitionType.moveVertical:
                    animationLabel = 6;
                    break;
            }

            anim.SetInteger("GP_ClimbType", animationLabel);

            if (jump)
            {
                anim.SetBool("GP_Leap", true);
            }
            else
            {
                anim.SetBool("GP_Move", true);
            }
        }

        enum TransitionType
        {
            moveHorizontal,
            moveVertical,
            leapUp,
            leapDown,
            leapLeft,
            leapRight
        }
        #endregion

        #region IK
        AvatarIKGoal ikInitiation; // IK component for the side which initiates the movement
        float _ikInitiationTime;
        Vector3[] _ikStartPositions = new Vector3[4];

        AvatarIKGoal ikWrapUp; // IK component fro the side which wraps up the movement
        float _ikWrapUpTime;
        Vector3[] _ikEndPositions = new Vector3[4];


        void InitIK(Vector3 moveDirection, bool isOnPoint)
        {
            Vector3 localDirection = moveDirection;

            if (Mathf.Abs(localDirection.y) > 0.5f)
            {
                float targetAnimation = 0;

                if (targetState == ClimbStates.onPoint)
                {
                    // Alternate limbs
                    ikInitiation = ik.ReturnMirrorIKGoal(ikInitiation);

                    // Revert move back to original point
                    if (targetPoint == previousPoint)
                    {
                        ikInitiation = ik.ReturnMirrorIKGoal(ikInitiation);
                    }
                }
                else
                {
                    if (!Mathf.Approximately(localDirection.x, 0))
                    {
                        ikInitiation = localDirection.x > 0 ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;
                    }

                    targetAnimation = (ikInitiation == AvatarIKGoal.RightHand) ? 1 : 0;
                    if (localDirection.y < 0)
                    {
                        targetAnimation = (ikInitiation == AvatarIKGoal.RightHand) ? 0 : 1;
                    }

                    anim.SetFloat("GP_VerticalFlipped", targetAnimation);
                }
            }
            else
            {
                ikInitiation = localDirection.x > 0 ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;

                // Revert move back to original point
                if (isOnPoint)
                {
                    ikInitiation = ik.ReturnMirrorIKGoal(ikInitiation);
                }
            }

            _ikInitiationTime = 0;
            UpdateIKTarget(0, ikInitiation, targetPoint);

            ikWrapUp = ik.ReturnFlipIKGoal(ikInitiation);
            _ikWrapUpTime = 0;
            UpdateIKTarget(1, ikWrapUp, targetPoint);
        }

        void InitIKDirect(Vector3 moveDirection)
        {
            if (Mathf.Approximately(moveDirection.y, 0))
            {
                InitIK(moveDirection, false);
                InitIKOpposite();
            }
            else
            {
                _ikInitiationTime = 0;
                _ikWrapUpTime = 0;

                UpdateIKTarget(0, AvatarIKGoal.LeftHand, targetPoint);
                UpdateIKTarget(1, AvatarIKGoal.LeftFoot, targetPoint);

                UpdateIKTarget(2, AvatarIKGoal.RightHand, targetPoint);
                UpdateIKTarget(3, AvatarIKGoal.RightFoot, targetPoint);
            }
        }

        void InitIKOpposite()
        {
            UpdateIKTarget(2, ik.ReturnMirrorIKGoal(ikInitiation), targetPoint);
            UpdateIKTarget(3, ik.ReturnMirrorIKGoal(ikWrapUp), targetPoint);
        }

        void UpdateIKTarget(int positionIndex, AvatarIKGoal ikComponent, Point p)
        {
            _ikStartPositions[positionIndex] = ik.ReturnCurrentPointPosition(ikComponent);
            _ikEndPositions[positionIndex] = p.ReturnIK(ikComponent).target.transform.position;
            ik.UpdateSinglePointToTarget(ikComponent, p);
        }

        void UpdateAllIKWeight(float curveTime, AnimationCurve aCurve)
        {
            float ikWeight = 1 - aCurve.Evaluate(curveTime);
            ik.SetAllIKWeights(ikWeight);
        }
        #endregion
    }

}
