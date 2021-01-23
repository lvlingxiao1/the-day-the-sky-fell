using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour {
    public float maxSpeed = 3;
    public float acceleration = 0.5f;

    Rigidbody rb;
    GameObject player;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update() {
    }

    void FixedUpdate() {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = rb.velocity.y; // enemy does not jump
        direction = direction.normalized * acceleration;
        rb.AddForce(direction, ForceMode.VelocityChange);
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        if (rb.position.y < -30) {
            FindObjectOfType<Main>().AddScore();
            Destroy(gameObject);
        }
    }

    //public void CollisionDetected(childHeadCollision childScript) {
    //    Debug.Log("child head collied!");
    //}
}
