using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Climbing
{
    public class ClimbIK : MonoBehaviour
    {
        Animator anim;
        Transform modelTransform;

        Point lhPoint;
        Point rhPoint;
        Point lfPoint;
        Point rfPoint;

        public float lhWeight = 1;
        public float rhWeight = 1;
        public float lfWeight = 1;
        public float rfWeight = 1;

        Transform lhHelper;
        Transform rhHelper;
        Transform lfHelper;
        Transform rfHelper;

        Vector3 lhTargetPosition;
        Vector3 rhTargetPosition;
        Vector3 lfTargetPosition;
        Vector3 rfTargetPosition;

        public float helperSpeed = 15;

        [HideInInspector]
        public bool useCurve;

        Transform hips;

        public bool forceFeetHeight;

        // Start is called before the first frame update
        void Start()
        {
            modelTransform = GameObject.Find("PlayerModel").transform;
            anim = GetComponent<Animator>();
            hips = anim.GetBoneTransform(HumanBodyBones.Hips);

            lhHelper = new GameObject().transform;
            lhHelper.name = "LH Helper IK";
            rhHelper = new GameObject().transform;
            rhHelper.name = "RH Helper IK";
            lfHelper = new GameObject().transform;
            lfHelper.name = "LF Helper IK";
            rfHelper = new GameObject().transform;
            rfHelper.name = "RF Helper IK";
        }

        public void UpdateAllPointsToTarget(Point targetPoint)
        {
            lhPoint = targetPoint;
            rhPoint = targetPoint;
            lfPoint = targetPoint;
            rfPoint = targetPoint;
        }

        public void UpdateSinglePointToTarget(AvatarIKGoal ik, Point targetPoint)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftHand:
                    lhPoint = targetPoint;
                    break;
                case AvatarIKGoal.RightHand:
                    rhPoint = targetPoint;
                    break;
                case AvatarIKGoal.LeftFoot:
                    lfPoint = targetPoint;
                    break;
                case AvatarIKGoal.RightFoot:
                    rfPoint = targetPoint;
                    break;
                default:
                    Debug.LogError("Setting an invalid IK component to a point!");
                    break;
            }
        }

        public void UpdateAllTargetPositions(Point p)
        {
            IkPositions lhTemp = p.ReturnIK(AvatarIKGoal.LeftHand);
            if (lhTemp.target)
            {
                lhTargetPosition = lhTemp.target.position;
            }

            IkPositions rhTemp = p.ReturnIK(AvatarIKGoal.RightHand);
            if (rhTemp.target)
            {
                rhTargetPosition = rhTemp.target.position;
            }

            IkPositions lfTemp = p.ReturnIK(AvatarIKGoal.LeftFoot);
            if (lfTemp.target)
            {
                lfTargetPosition = lfTemp.target.position;
            }

            IkPositions rfTemp = p.ReturnIK(AvatarIKGoal.RightFoot);
            if (rfTemp.target)
            {
                rfTargetPosition = rfTemp.target.position;
            }
        }

        public void UpdateSingleTargetPosition(AvatarIKGoal ik, Vector3 targetPosition)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftHand:
                    lhTargetPosition = targetPosition;
                    break;
                case AvatarIKGoal.RightHand:
                    rhTargetPosition = targetPosition;
                    break;
                case AvatarIKGoal.LeftFoot:
                    lfTargetPosition = targetPosition;
                    break;
                case AvatarIKGoal.RightFoot:
                    rfTargetPosition = targetPosition;
                    break;
                default:
                    Debug.LogError("Setting an invalid IK component to a target position!");
                    break;
            }
        }

        public Vector3 ReturnCurrentPointPosition(AvatarIKGoal ik)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftHand:
                    return lhPoint.ReturnIK(ik).target.transform.position;
                case AvatarIKGoal.RightHand:
                    return rhPoint.ReturnIK(ik).target.transform.position;
                case AvatarIKGoal.LeftFoot:
                    return lfPoint.ReturnIK(ik).target.transform.position;
                case AvatarIKGoal.RightFoot:
                    return rfPoint.ReturnIK(ik).target.transform.position;
                default:
                    Debug.LogError("Getting position of an invalid IK Component!");
                    return default(Vector3);
            }
        }

        public Point ReturnPointForIk(AvatarIKGoal ik)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftHand:
                    return lhPoint;
                case AvatarIKGoal.RightHand:
                    return rhPoint;
                case AvatarIKGoal.LeftFoot:
                    return lfPoint;
                case AvatarIKGoal.RightFoot:
                    return rfPoint;
                default:
                    Debug.LogError("Getting Point object of an invalid IK Component!");
                    return null;
            }
        }

        public AvatarIKGoal ReturnMirrorIKGoal(AvatarIKGoal ik)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftHand:
                    return AvatarIKGoal.RightHand;
                case AvatarIKGoal.RightHand:
                    return AvatarIKGoal.LeftHand;
                case AvatarIKGoal.LeftFoot:
                    return AvatarIKGoal.RightFoot;
                case AvatarIKGoal.RightFoot:
                    return AvatarIKGoal.LeftFoot;
                default:
                    Debug.LogError("Getting the opposite IKGoal of an invalid IK Component!");
                    return default(AvatarIKGoal);
            }
        }

        public AvatarIKGoal ReturnFlipIKGoal(AvatarIKGoal ik)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftHand:
                    return AvatarIKGoal.LeftFoot;
                case AvatarIKGoal.RightHand:
                    return AvatarIKGoal.RightFoot;
                case AvatarIKGoal.LeftFoot:
                    return AvatarIKGoal.LeftHand;
                case AvatarIKGoal.RightFoot:
                    return AvatarIKGoal.RightHand;
                default:
                    Debug.LogError("Getting the opposite IKGoal of an invalid IK Component!");
                    return default(AvatarIKGoal);
            }
        }

        public void SetAllIKWeights(float newWeight)
        {
            lhWeight = newWeight;
            rhWeight = newWeight;
            lfWeight = newWeight;
            rfWeight = newWeight;
        }

        public void ForceUpdateAllHelpers()
        {
            if (lhPoint != null)
            {
                lhHelper.position = lhTargetPosition;
            }

            if (rhPoint != null)
            {
                rhHelper.position = rhTargetPosition;
            }

            if (lfPoint != null)
            {
                lfHelper.position = lfTargetPosition;
            }

            if (rfPoint != null)
            {
                rfHelper.position = rfTargetPosition;
            }
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (lhPoint)
            {
                IkPositions lhTemp = lhPoint.ReturnIK(AvatarIKGoal.LeftHand);

                if (lhTemp.target)
                {
                    lhHelper.transform.position = Vector3.Lerp(lhHelper.transform.position, lhTargetPosition, Time.deltaTime * helperSpeed);
                }

                UpdateIK(AvatarIKGoal.LeftHand, lhTemp, lhHelper, lhWeight, AvatarIKHint.LeftElbow);
            }

            if (rhPoint)
            {
                IkPositions rhTemp = rhPoint.ReturnIK(AvatarIKGoal.RightHand);

                if (rhTemp.target)
                {
                    rhHelper.transform.position = Vector3.Lerp(rhHelper.transform.position, rhTargetPosition, Time.deltaTime * helperSpeed);
                }

                UpdateIK(AvatarIKGoal.RightHand, rhTemp, rhHelper, rhWeight, AvatarIKHint.RightElbow);
            }

            if (hips == null)
            {
                hips = anim.GetBoneTransform(HumanBodyBones.Hips);
            }

            if (lfPoint)
            {
                IkPositions lfTemp = lfPoint.ReturnIK(AvatarIKGoal.LeftFoot);

                if (lfTemp.target)
                {
                    Vector3 targetPosition = lfTargetPosition;

                    if (forceFeetHeight)
                    {
                        if (targetPosition.y > hips.transform.position.y)
                        {
                            targetPosition.y = targetPosition.y - 0.2f;
                        }
                    }

                    lfHelper.transform.position = Vector3.Lerp(lfHelper.transform.position, lfTargetPosition, Time.deltaTime * helperSpeed);
                }

                UpdateIK(AvatarIKGoal.LeftFoot, lfTemp, lfHelper, lfWeight, AvatarIKHint.LeftKnee);
            }

            if (rfPoint)
            {
                IkPositions rfTemp = rfPoint.ReturnIK(AvatarIKGoal.RightFoot);

                if (rfTemp.target)
                {
                    Vector3 targetPosition = rfTargetPosition;

                    if (forceFeetHeight)
                    {
                        if (targetPosition.y > hips.transform.position.y)
                        {
                            targetPosition.y = targetPosition.y - 0.2f;
                        }
                    }

                    rfHelper.transform.position = Vector3.Lerp(rfHelper.transform.position, rfTargetPosition, Time.deltaTime * helperSpeed);
                }

                UpdateIK(AvatarIKGoal.RightFoot, rfTemp, rfHelper, rfWeight, AvatarIKHint.RightKnee);
            }
        }

        void UpdateIK(AvatarIKGoal ik, IkPositions ikInfo, Transform ikHelper, float ikWeight, AvatarIKHint ikHint)
        {
            if (ikInfo != null)
            {
                anim.SetIKPositionWeight(ik, ikWeight);
                anim.SetIKRotationWeight(ik, ikWeight);
                anim.SetIKPosition(ik, ikHelper.position);
                anim.SetIKRotation(ik, ikHelper.rotation);

                if ((ik == AvatarIKGoal.LeftHand) || (ik == AvatarIKGoal.RightHand))
                {
                    Transform shoulder = (ik == AvatarIKGoal.LeftHand) ?
                        anim.GetBoneTransform(HumanBodyBones.LeftShoulder) :
                        anim.GetBoneTransform(HumanBodyBones.RightShoulder);

                    // relative offset
                    Vector3 offset = modelTransform.forward + Vector3.up / 2;

                    // [TUNE] Rotate hand to align with should so that the gesture is more natural, can be tuned
                    Vector3 targetRotationDirection = shoulder.transform.position - (ikHelper.transform.position + offset);

                    Quaternion targetRotation = Quaternion.LookRotation(-targetRotationDirection);
                    ikHelper.rotation = targetRotation;
                }
                else
                {
                    ikHelper.rotation = ikInfo.target.transform.rotation;
                }

                if (ikInfo.hint != null)
                {
                    anim.SetIKHintPositionWeight(ikHint, ikWeight);
                    anim.SetIKHintPosition(ikHint, ikInfo.hint.position);
                }
            }
        }

        public void UpdateSingleIKWeight(AvatarIKGoal ik, float newWeight)
        {
            switch (ik)
            {
                case AvatarIKGoal.LeftHand:
                    lhWeight = newWeight;
                    break;
                case AvatarIKGoal.RightHand:
                    rhWeight = newWeight;
                    break;
                case AvatarIKGoal.LeftFoot:
                    lfWeight = newWeight;
                    break;
                case AvatarIKGoal.RightFoot:
                    rfWeight = newWeight;
                    break;
                default:
                    Debug.LogError("Updating IK weight of an invalid IK Component!");
                    break;
            }
        }
    }

}
