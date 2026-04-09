using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 12f;

    public float minX = -20f;
    public float maxX = 20f;

    public float jumpForce = 8f;
    public float gravity = -20f;

    public float crouchHeight = 1f;

    public float jumpCooldown = 0.3f;
    public StagePathController stagePathController;

    CharacterController controller;

    float verticalVelocity;
    float originalHeight;

    float lastJumpTime = -10f;
    string lastCommand = "";

    Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalHeight = controller.height;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        string cmd = HandInputReceiver.LeftHand;

        float moveX = 0f;

        if (cmd == "MoveLeft")
            moveX = -moveSpeed;

        if (cmd == "MoveRight")
            moveX = moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        if (cmd == "Jump"
            && lastCommand != "Jump"
            && controller.isGrounded
            && Time.time - lastJumpTime >= jumpCooldown)
        {
            verticalVelocity = jumpForce;
            lastJumpTime = Time.time;

            animator.SetTrigger("jump");
        }

        verticalVelocity += gravity * Time.deltaTime;

        // ขยับเฉพาะ X,Y
        Vector3 move = new Vector3(moveX, verticalVelocity, 0f);

        controller.Move(move * Time.deltaTime);

        // จำกัดการหลบซ้ายขวา
        Vector3 pos = transform.localPosition;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = 0f; // ล็อก Z ให้ตรงกับ RailRoot
        transform.localPosition = pos;

        // if (cmd == "Crouch")
        //     controller.height = crouchHeight;
        // else
        //     controller.height = originalHeight;

        lastCommand = cmd;

        animator.SetBool("move", stagePathController.moving);


    }
}