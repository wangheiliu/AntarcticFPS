using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private Transform playerPos;
    public float speed = 5f;
    public float sprintSpeed = 10f;
    public float jumpForce = 2f;
    private float normalHeight;
    private float crouchHeight = 1f;
    private float crouchSpeed = 25f;
    [SerializeField] private float slideFriction = 3f;
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
    public float gravity = -25f;
    public float velocityY;
    
    
    private bool canSprint;
    private bool isCrouching;
    private bool canSlide;
    private bool isSlideCoolDownEnabled;
    private bool isSliding;
    private bool isSprinting;
    private float targetHeight;
    private bool canRegenerateStamina = true;

    private Vector3 slideDirection;
    private float startingSlideSpeed = 12f;
    private float slideSpeed;

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
        //cam = Camera.main.transform;
        normalHeight = charController.height;
    }

    void Update()
    {
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
        move = move.normalized * speed;

        //sprinting
        bool isMoving = moveInput != Vector2.zero;
        isCrouching = (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.cKey.isPressed) && !Keyboard.current.leftShiftKey.isPressed;
        canSlide = Keyboard.current.leftShiftKey.isPressed && Keyboard.current.cKey.isPressed && hudScript.stamina > 15;
        canSprint = Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.cKey.isPressed && isMoving && hudScript.stamina > 0 && !isCrouching && isMoving;
        targetHeight = (isCrouching || isSliding) ? crouchHeight : normalHeight;

        if (canSprint)
        {
            isSprinting = true;
            speed = sprintSpeed;
            staminaRecoveryTime = 1.5f;
            hudScript.stamina = Mathf.Clamp(
                hudScript.stamina - 20f * Time.deltaTime,
                0f,
                100f
            );
        } else
        {
            isSprinting = false;
            speed = 5f;
            if (staminaRecoveryTime > 0)
            {
                if (canRegenerateStamina)
                {
                    staminaRecoveryTime -= Time.deltaTime;
                }
            } else
            {
                hudScript.stamina = Mathf.Clamp(hudScript.stamina + 7f * Time.deltaTime, 0f, 100f);
            }
        }
        
        //crouching
        if (isCrouching && !canSlide)
        {
            Crouch();
        } else
        {
            if (CanStand())
            {
                UnCrouch();
            } else
            {
                Crouch();
            }
        }    

        //slide
        if (canSlide && !isSliding && !isSlideCoolDownEnabled && !isCrouching)
        {
            Debug.Log("Starting to slide!");
            StartSlide();
        }

        if (isSliding && !isSlideCoolDownEnabled)
        {
            UpdateSlide();
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
        if (Keyboard.current.spaceKey.wasPressedThisFrame && charController.isGrounded && !isCrouching)
        {
            velocityY = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        if (charController.collisionFlags == CollisionFlags.Above && velocityY > 0)
        {
            velocityY = 0; // stop upward movement if hitting ceiling
        }

        
        // Gravity
        velocityY += gravity * Time.deltaTime;

        if (isSliding)
        {   
            if (move != Vector3.zero)
            {
                slideDirection = Vector3.Slerp(slideDirection, move.normalized, 2.5f * Time.deltaTime); // lerps it over time if there's any movement 
            }
            
            move = slideDirection * slideSpeed;
        }
        // Apply vertical movement
        move.y = velocityY;
        // Final move
        charController.Move(move * Time.deltaTime);
    }
    bool CanStand()
    {
        return !Physics.Raycast(
            transform.position,
            Vector3.up,
            1f
        );
    }

    private void Crouch()
    {
        speed = 3f;
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
            speed = sprintSpeed;
        } else
        {
            speed = 5f;
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
        slideSpeed = Mathf.Max(speed, sprintSpeed) * 1.2f;
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
        Debug.Log("sliding");

    }

    private void UpdateSlide()
    {
        if (slideSpeed > 1f)
        {
            slideSpeed -= slideFriction * Time.deltaTime;
            
            Debug.Log(slideSpeed);
        } else
        {
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
        Debug.Log("End slide");
        
    }

    private void RespawnPlayer(Vector3 position)
    {
        if (playerPos.position.y <= -100)
        {
            playerPos.position = new Vector3(10,0,-10);
        }
    }
}