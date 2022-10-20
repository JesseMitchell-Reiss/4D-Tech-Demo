using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // control variables
    [SerializeField]
    float jumpHeight;
    [SerializeField]
    float playerSpeed;
    [SerializeField]
    float sprintMultiplier;
    [SerializeField]
    Vector2 sensitivity;

    // object variables
    CharacterController controller;
    Camera cam;

    // hidden variables
    [HideInInspector]
    public float w;
    float jumpVelocity;
    Vector2 XYmove;
    bool jump;
    float Wmove;
    Vector2 look;
    float Zvelocity;
    bool sprint;
    // Start is called before the first frame update
    void Start()
    {
        controller = this.gameObject.GetComponent<CharacterController>();
        jumpVelocity = 2 * Mathf.Sqrt(Physics.gravity.z * jumpHeight);
        cam = this.gameObject.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // player yaw look
        this.gameObject.transform.Rotate(0, look.x * sensitivity.x * Time.deltaTime, 0);
        // normalize the current camera pitch
        float camPitch = cam.transform.rotation.eulerAngles.x;
        float normalPitch = -(camPitch - 180f - Mathf.Sign(camPitch - 180f) * 180f);
        // check for pitch within range or moving in correct direction
        if(normalPitch > -85f && look.y < 0 || normalPitch < 85f && look.y > 0)
        {
            cam.transform.Rotate(-look.y * sensitivity.y * Time.deltaTime, 0, 0);
        }
    }

    private void FixedUpdate()
    {
        // calculate gravity
        if(!controller.isGrounded)
        {
            Zvelocity -= Physics.gravity.z * Time.fixedDeltaTime;
        }
        else
        {
            Zvelocity = 0f;
        }

        // jump if grounded and jumping
        if(jump && controller.isGrounded)
        {
            Zvelocity = jumpVelocity;
        }

        // add sprint
        if(sprint && XYmove.y > 0)
        {
            XYmove.y *= sprintMultiplier;
        }

        // create total movement vector
        Vector3 totalMove = new Vector3(XYmove.x * playerSpeed * Time.fixedDeltaTime, jumpVelocity * Time.fixedDeltaTime, XYmove.y * playerSpeed * Time.fixedDeltaTime);
        totalMove = this.transform.TransformDirection(totalMove);
        // move controller based on motion vectors
        controller.Move(totalMove);
    }

    public void moveXYFunction(InputAction.CallbackContext context)
    {
        XYmove = context.ReadValue<Vector2>();
    }

    public void lookFunction(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void moveWFunction(InputAction.CallbackContext context)
    {
        Wmove = context.ReadValue<float>();
    }

    public void jumpFunction(InputAction.CallbackContext context)
    {
        if(context.ReadValue<bool>())
        {
            jump = true;
        }
        else
        {
            jump = false;
        }
    }

    public void sprintFunction(InputAction.CallbackContext context)
    {
        if(context.ReadValue<bool>())
        {
            sprint = true;
        }
        else
        {
            sprint = false;
        }
    }
}
