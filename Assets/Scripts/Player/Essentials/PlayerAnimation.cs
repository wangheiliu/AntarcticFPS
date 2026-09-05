using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript;
    [SerializeField] private Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private PlayerMovement.PlayerState currentState;

    private int playerStateHash;
    private int isTransitioningHash;
    void Awake()
    {
        playerStateHash = Animator.StringToHash("PlayerState");
        isTransitioningHash = Animator.StringToHash("IsAnimating");
    }
    void Start()
    {
        currentState = playerMovementScript.currentPlayerState;
    }

    void OnEnable()
    {
        playerMovementScript.OnPlayerStateChanged += RunPlayerAnimation;
    }

    void OnDisable()
    {
        playerMovementScript.OnPlayerStateChanged -= RunPlayerAnimation;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RunPlayerAnimation(PlayerMovement.PlayerState state)
    {
        animator.SetInteger(playerStateHash, (int)state);
    }
}
