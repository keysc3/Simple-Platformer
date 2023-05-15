using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    private Vector3 movement = Vector3.zero;
    private bool shouldJump;

    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float maxVelocity =  5.0f;
    [SerializeField] private float jumpSpeed = 400.0f;
    [SerializeField] private Text velocityText;
    [SerializeField] private Text rotationText;

    //Get Rigidbody and animator components
    void Awake() {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    //Update is called once per frame
    void Update() {
        velocityText.text = "Velocity: " + rb.velocity.z;
        rotationText.text = "Rotation: " + transform.eulerAngles.y;

        if (shouldJump == false)
            shouldJump = Input.GetKeyDown(KeyCode.Space);

        //If W key is down shift character a lane to the left.
        if (Input.GetKeyDown(KeyCode.W))
            if((transform.position.x - 1.0f) > -2.0f)
                transform.position = transform.position + Vector3.left;
        //If S key is down shift character a lane to the right
        if (Input.GetKeyDown(KeyCode.S))
            if((transform.position.x + 1.0f) < 2.0f)
                transform.position = transform.position + Vector3.right;
    }

    //FixedUpdate is called at a constant rate, handles physics engine.
    void FixedUpdate()
    {

        if (shouldJump){
            rb.AddForce(jumpSpeed * Vector3.up);
            shouldJump = false;
        }

        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)){
            Debug.Log("No key press");
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            rb.angularVelocity = Vector3.zero;
            animator.SetBool("isRunning", false);
        }

        //If max speed reached return
        if (Mathf.Abs(rb.velocity.z) <= maxVelocity){
            //If D key is pressed move the player forward and update animation
            if(Input.GetKey(KeyCode.D)){
                rb.AddForce(Vector3.forward * speed, ForceMode.Impulse);
                animator.SetBool("isRunning", true);
                //If character is not turned forward, rotate them.
                if(transform.eulerAngles.y != 0)
                    transform.RotateAround(transform.position, Vector3.up, 180);
            }
            //If A key is pressed move the player forward and update animation
            if (Input.GetKey(KeyCode.A)){
                rb.AddForce(Vector3.back * speed, ForceMode.Impulse);
                animator.SetBool("isRunning", true);
                //If character is not turned backwards, rotate them.
                if(transform.eulerAngles.y != 180)
                    transform.RotateAround(transform.position, Vector3.up, -180);
            }
        }
    }
}