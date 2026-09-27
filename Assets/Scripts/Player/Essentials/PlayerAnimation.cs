using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayerMovement.PlayerState currentState;

    private readonly int playerStateHash = Animator.StringToHash("PlayerState");
    private readonly int isTransitioningHash = Animator.StringToHash("IsAnimating");
    private readonly int idleStateHash = Animator.StringToHash("Idle");
    private readonly int jumpTriggerHash = Animator.StringToHash("JumpTrigger");
    private readonly int jumpStartHash = Animator.StringToHash("JumpStart");
    private readonly int jumpEndHash = Animator.StringToHash("JumpEnd");
    private readonly int slideStartHash = Animator.StringToHash("SlideStart");
    private readonly int slideEndHash = Animator.StringToHash("SlideEnd");
    private readonly int runStateHash = Animator.StringToHash("PlayerSpeed");
    
    void Start()
    {
        currentState = playerMovementScript.CurrentPlayerState;
    }

    void OnEnable()
    {
        playerMovementScript.OnPlayerStateChanged += RunPlayerAnimation;
        playerMovementScript.OnPlayerJumped += OnPlayerJumped;
        playerMovementScript.OnPlayerLanded += OnPlayerLanded;
        playerMovementScript.OnPlayerSlide += OnPlayerSlide;
        playerMovementScript.OnPlayerSpeedChanged += OnPlayerSpeedChanged;
    }

    void OnDisable()
    {
        playerMovementScript.OnPlayerStateChanged -= RunPlayerAnimation;
        playerMovementScript.OnPlayerJumped -= OnPlayerJumped;
        playerMovementScript.OnPlayerLanded -= OnPlayerLanded;
        playerMovementScript.OnPlayerSlide -= OnPlayerSlide;
        playerMovementScript.OnPlayerSpeedChanged -= OnPlayerSpeedChanged;
    }

    private void RunPlayerAnimation(PlayerMovement.PlayerState state)
    {
        if (state == currentState) return;
        currentState = state;
        
        animator.SetInteger(playerStateHash, (int)state);
    }

    private void JumpAnimationEvt(string param)
    {
        if (param == "Start")
        {
            animator.SetTrigger(jumpStartHash);
            animator.SetBool(isTransitioningHash, true);
        }
        else if (param == "End")
        {
            animator.ResetTrigger(jumpStartHash);
            animator.ResetTrigger(jumpTriggerHash);
            animator.SetBool(isTransitioningHash, false);
        }
    }
    private void OnPlayerJumped()
    {
        animator.SetTrigger(jumpTriggerHash);
        //animator.SetInteger(playerStateHash, (int)PlayerMovement.PlayerState.Jumping);
    }

    private void OnPlayerLanded()
    {
        animator.SetTrigger(jumpEndHash);
    }

    private void OnPlayerSlide(bool isSliding)
    {
        if (isSliding)
        {
            animator.SetBool(isTransitioningHash, false);
            animator.ResetTrigger(slideEndHash);
            animator.SetTrigger(slideStartHash);
        }
        else
        {
            animator.ResetTrigger(slideStartHash);
            animator.SetTrigger(slideEndHash);
        }
    }

    private void OnPlayerSpeedChanged(float speed)
    {
        float multiplier = speed / playerMovementScript.walkSpeed;
        animator.SetFloat(runStateHash, multiplier);
    }
}
