using System;
using UnityEngine;

public abstract class BasePickableItems : MonoBehaviour, IProximityPromptTriggerable
{
    [SerializeField] protected ProximityPrompt prompt;

    void OnEnable()
    {
        RegisterProximityEvents();
    }

    void OnDisable()
    {
        UnregisterProximityEvents();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public abstract void OnProximityPromptHold();

    public abstract void OnProximityPromptRelease();

    public abstract void OnProximityPromptTrigger();
    protected void RegisterProximityEvents()
    {
        prompt.OnPromptHold += OnProximityPromptHold;
        prompt.OnPromptRelease += OnProximityPromptRelease;
        prompt.OnPromptTrigger += OnProximityPromptTrigger;
    }
    protected void UnregisterProximityEvents()
    {
        prompt.OnPromptHold -= OnProximityPromptHold;
        prompt.OnPromptRelease -= OnProximityPromptRelease;
        prompt.OnPromptTrigger -= OnProximityPromptTrigger;
    }
}
