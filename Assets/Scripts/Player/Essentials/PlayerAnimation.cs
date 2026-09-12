using System;
using System.Collections;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_2 = new WaitForSeconds(0.2f);
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
    
    void Start()
    {
        currentState = playerMovementScript.CurrentPlayerState;
    }

    void OnEnable()
    {
        playerMovementScript.OnPlayerStateChanged += RunPlayerAnimation;
        playerMovementScript.OnPlayerJumped += OnPlayerJumped;
        playerMovementScript.OnPlayerLanded += OnPlayerLanded;
    }

    void OnDisable()
    {
        playerMovementScript.OnPlayerStateChanged -= RunPlayerAnimation;
        playerMovementScript.OnPlayerJumped -= OnPlayerJumped;
        playerMovementScript.OnPlayerLanded -= OnPlayerLanded;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(animator.GetBool(jumpTriggerHash));
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
            Debug.Log("Jump Animation Started");
        }
        else if (param == "End")
        {
            animator.ResetTrigger(jumpStartHash);
            animator.ResetTrigger(jumpTriggerHash);
            animator.SetBool(isTransitioningHash, false);
            Debug.Log("Jump Animation Ended");
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
}
