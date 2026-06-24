using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;
using Unity.VisualScripting;
using System;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform playerPos;
    public float walkSpeed = 5f;
    private float currentSpeed;
    public float sprintSpeed = 10f;
    public float jumpForce = 5f;
    private float normalHeight;
    private float crouchHeight = 1f;
    private float crouchSpeed = 25f;
    [SerializeField] private float slideFriction = 10f;
    private float slideCoolDownTimer;
    private float slideCooldownDuration = 3f;
    public float staminaRecoveryTime;

    [Header("Humanoid")]
    private CharacterController charController;
    [Header("HUD")]
    [SerializeField] private UIDocument hudDocument;
    [SerializeField] private HUDScript hudScript;
    //private Transform transform;
    [Header("Camera")]
    [SerializeField] private Transform cam;


    [Header("Gravity")]
    public float gravity = -12f;
    public float velocityY;

    //all the booleans
    private bool canSprint;
    private bool isCrouching;
    private bool canSlide;
    private bool isSlideCoolDownEnabled;
    private bool isSliding;
    private bool isSprinting;
    private float targetHeight;
    private bool canRegenerateStamina = true;

    // Slide setting variables
    private Vector3 slideDirection;
    private float slideSpeed;
    private Vector3 downhillDirection;
    private Vector3 slopeNormal;
    private Vector3 lastMoveDirection;
    private Vector3 targetDirection;
    private bool onSlope;

    // Player State (I will work on that when I finish the sliding mechanic)
    public enum PlayerState
    {
        Walking,
        Sprinting,
        Crouching,
        Sliding
    }
    public PlayerState currentPlayerState;
    void Start()
    {
        charController = GetComponent<CharacterController>();
        normalHeight = charController.height;
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        CheckSlope();
        // Movement input
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current.wKey.isPressed)
            moveInput.x += 1;

        if (Keyboard.current.sKey.isPressed)
            moveInput.x -= 1;

        if (Keyboard.current.aKey.isPressed)
            moveInput.y -= 1;

        if (Keyboard.current.dKey.isPressed)
            moveInput.y += 1;


        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        camRight = Vector3.Cross(Vector3.up, camForward);

        Vector3 move = camForward * moveInput.x + camRight * moveInput.y;

        if (onSlope && !isSliding)
        {
            move = Vector3.ProjectOnPlane(move,slopeNormal);
        }

        lastMoveDirection = move.normalized;
        move = move.normalized * currentSpeed;

        // tracks the booleans
        bool isMoving = moveInput != Vector2.zero;
        isCrouching = (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.cKey.isPressed) && !Keyboard.current.leftShiftKey.isPressed;
        canSlide = Keyboard.current.leftShiftKey.isPressed && Keyboard.current.cKey.isPressed && hudScript.stamina > 15;
        canSprint = Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.cKey.isPressed && isMoving && hudScript.stamina > 0 && !isCrouching && isMoving;
        targetHeight = (isCrouching || isSliding) ? crouchHeight : normalHeight;

        // sprinting
        if (canSprint)
        {
            isSprinting = true;
            currentSpeed = sprintSpeed;
            staminaRecoveryTime = 1.5f;
            hudScript.stamina = Mathf.Clamp(
                hudScript.stamina - 20f * Time.deltaTime,
                0f,
                100f
            );
        }
        else
        {
            isSprinting = false;
            currentSpeed = 5f;
            if (staminaRecoveryTime > 0)
            {
                if (canRegenerateStamina)
                {
                    staminaRecoveryTime -= Time.deltaTime;
                }
            }
            else
            {
                hudScript.stamina = Mathf.Clamp(hudScript.stamina + 7f * Time.deltaTime, 0f, 100f);
            }
        }

        //crouching
        if (isCrouching && !canSlide)
        {
            Crouch();
        }
        else
        {
            if (CanStand())
            {
                UnCrouch();
            }
            else
            {
                Crouch();
            }
        }

        //slide
        if (canSlide && !isSliding && !isSlideCoolDownEnabled && !isCrouching)
        {
            StartSlide();
        }

        if (isSliding && !isSlideCoolDownEnabled)
        {
            UpdateSlide();
        }

        if (slideCoolDownTimer > 0 && !isSliding)
        {
            slideCoolDownTimer -= Time.deltaTime;
            hudScript.cooldownLabel.text = $"Sliding: {(float)Math.Round(slideCoolDownTimer, 1)}"; // explicitly rounds the timer to 1 decimal place, Mathf is kinda bad here, so we need System
        }
        else if (isSliding)
        {
            hudScript.cooldownLabel.text = "Sliding";
        }
        else if (slideCoolDownTimer <= 0 && hudScript.stamina > 15)
        {
            hudScript.cooldownLabel.text = "Sliding: ready";
        } 
        else
        {
            hudScript.cooldownLabel.text = "You need rest!";    
        }

        if (isSliding && Keyboard.current.cKey.wasReleasedThisFrame)
        {
            StartCoroutine(EndSlide());
        }

        // Ground check (THIS replaces OnCollision)
        if (charController.isGrounded && velocityY < 0)
        {
            velocityY = -2f; // keeps player "stuck" to ground slightly
        }


        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && charController.isGrounded && !isCrouching && !isSliding)
        {
            velocityY = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        if (charController.isGrounded && velocityY < 0)
        {
            velocityY = -2f;
        }
        else
        {
            velocityY += gravity * Time.deltaTime;
        }


        // Gravity
        velocityY += gravity * Time.deltaTime;

        if (isSliding)
        {

            if (targetDirection != Vector3.zero)
            {
                targetDirection = lastMoveDirection;
            } else
            {
                targetDirection = cam.forward;
                targetDirection.y = 0;
                targetDirection.Normalize();
            }
            slideDirection = Vector3.Slerp(
                slideDirection,
                targetDirection,
                5f * Time.deltaTime
            );
            if (onSlope)
            {
                Vector3 slopeMove = Vector3.ProjectOnPlane(
                    slideDirection,
                    slopeNormal
                );

                move = slopeMove.normalized * slideSpeed;
                move += Vector3.ProjectOnPlane(
                    Vector3.down,
                    slopeNormal
                ) * 2f;
            }
            else
            {
                move = slideDirection * slideSpeed;
                move.y = velocityY;
            }
        }
        else
        {
            move.y = velocityY; 
        }
        // Final move
        charController.Move(move * Time.deltaTime);
    }
    public bool CanStand()
    {
        return !Physics.Raycast(
            transform.position,
            Vector3.up,
            1f
        );
    }

    private void CheckSlope()
    {
        onSlope = false;
        if (Physics.Raycast(
        transform.position - Vector3.up * (charController.height / 2),
        Vector3.down,
        out RaycastHit hit,
        charController.height * 0.5f + 0.5f))
        {
            slopeNormal = hit.normal;
            onSlope = Vector3.Angle(slopeNormal, Vector3.up) > 5f;
        }
    }

    private void Crouch()
    {
        currentSpeed = 3f;
        charController.height = Mathf.Lerp(charController.height, targetHeight, crouchSpeed * Time.deltaTime);
        charController.center = new Vector3(
            0,
            0,
            0
        );
    }
    private void UnCrouch()
    {
        if (canSprint)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = 5f;
        }
        charController.height = Mathf.Lerp(charController.height, targetHeight, crouchSpeed * Time.deltaTime);
        charController.center = new Vector3(
            0,
            0,
            0
        );
    }

    private void StartSlide()
    {
        slideSpeed = Mathf.Max(currentSpeed, sprintSpeed) * 1.2f;
        canRegenerateStamina = false;
        hudScript.stamina -= 15;
        isSliding = true;
        charController.height = Mathf.Lerp(charController.height, targetHeight, crouchSpeed * Time.deltaTime);
        charController.center = new Vector3(
            0,
            0,
            0
        );

        slideDirection = cam.forward;
        slideDirection.y = 0;
        slideDirection.Normalize();

    }

    private void UpdateSlide()
    {
        onSlope = false;

        if (Physics.Raycast(
            transform.position - Vector3.up * (charController.height / 2),
            Vector3.down,
            out RaycastHit hit,
            charController.height * 0.5f + 0.5f))
        {
            slopeNormal = hit.normal;
            onSlope = Vector3.Angle(slopeNormal, Vector3.up) > 5f;

            if (onSlope)
            {
                float slopeAngle = Vector3.Angle(
                    slopeNormal,
                    Vector3.up
                );

                downhillDirection = Vector3.ProjectOnPlane(
                    Vector3.down,
                    slopeNormal
                ).normalized;

                float dot = Vector3.Dot(
                    slideDirection,
                    downhillDirection
                );

                if (dot < 0)
                {
                    slideSpeed -= Mathf.Abs(dot) * slopeAngle * 0.2f * Time.deltaTime;
                }
                else
                {
                    slideSpeed += dot * slopeAngle * 0.5f * Time.deltaTime;
                }
                slideSpeed = Mathf.Max(0.1f, slideSpeed);
            }
        }
        if (slideSpeed > 0.2f)
        {
            slideSpeed -= slideFriction * Time.deltaTime;
        }
        else
        {
            if (Keyboard.current.cKey.isPressed)
            {
                if (slideSpeed > 0)
                {
                    slideSpeed -= slideFriction * Time.deltaTime;
                }
                return;
            }
            canSlide = false;
            isSlideCoolDownEnabled = true;
            StartCoroutine(EndSlide());
            return;
        }
    }

    private IEnumerator EndSlide()
    {
        if (!isSliding)
        {
            yield break;
        }
        slideCoolDownTimer = slideCooldownDuration; //sets the timer so that it will start in Update()
        isSliding = false;
        isSlideCoolDownEnabled = true;
        charController.height = Mathf.Lerp(charController.height, targetHeight, crouchSpeed * Time.deltaTime);
        charController.center = new Vector3(
            0,
            0,
            0
        );

        yield return new WaitForSeconds(3);
        canRegenerateStamina = true;
        isSlideCoolDownEnabled = false;
    }

    private void RespawnPlayer(Vector3 position)
    {
        if (playerPos.position.y <= -100)
        {
            playerPos.position = new Vector3(10, 0, -10);
        }
    }
}