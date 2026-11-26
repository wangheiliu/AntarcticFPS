using System;
using Game.Weapons;
using Unity.VisualScripting;
using UnityEngine;

public class PickableWeapon : BasePickableItems
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Rigidbody rb;
    [DoNotSerialize] public WeaponData WeaponData { get; }
    public static event Action<PickableWeapon, Weapon> OnWeaponPickup;
    public event Action<bool> EnablePrompt;

    void OnEnable()
    {
        RegisterProximityEvents();
        EnablePrompt += EnableProximityPrompt;
    }

    void OnDisable()
    {
        UnregisterProximityEvents();
    }

    void Update()
    {
        
    }

    public override void OnProximityPromptHold()
    {
        
    }

    public override void OnProximityPromptRelease()
    {
        
    }

    public override void OnProximityPromptTrigger()
    {
        Weapon weapon = new(weaponData, weaponData.clipSize);
        prompt.EnablePrompt(false);
        OnWeaponPickup?.Invoke(this, weapon);
    }

    private void EnableProximityPrompt(bool state)
    {
        prompt.EnablePrompt(state);

        if (state)
        {
            transform.SetParent(null, worldPositionStays: true);
        }
    }

    public void InvokeEnableProximityEvent(bool state)
    {
        EnablePrompt?.Invoke(state);
    }
}
