using System;
using Game.Weapons;
using Unity.VisualScripting;
using UnityEngine;

public class PickableWeapon : BasePickableItems
{
    [SerializeField] private WeaponData weaponData;
    [DoNotSerialize] public WeaponData WeaponData { get; }
    public static event Action<PickableWeapon, Weapon> OnWeaponPickup;

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
        Weapon weapon = new(weaponData, weaponData.clipSize);
        prompt.EnablePrompt(false);
        OnWeaponPickup?.Invoke(this, weapon);
    }
}
