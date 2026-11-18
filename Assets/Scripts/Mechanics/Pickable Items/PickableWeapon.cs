using System;
using Game.Weapons;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(ProximityPrompt))]
public class PickableWeapon : BasePickableItems
{
    [SerializeField] private WeaponData weaponData;
    [DoNotSerialize] public WeaponData WeaponData { get; }
    public static event Action<WeaponData> OnWeaponPickup;

    void OnEnable()
    {
        RegisterProximityEvents();
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
        OnWeaponPickup?.Invoke(weaponData);
    }
}
