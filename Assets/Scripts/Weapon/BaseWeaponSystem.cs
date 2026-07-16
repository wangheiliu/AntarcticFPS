using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable] public class Weapon
{
    public WeaponData data;
    public int ammo;
}
public class BaseWeaponSystem : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] public Weapon currentWeapon;
    [Header("Weapon Information")]
    
    public static bool isWeaponEquipped;
    public bool isOnCooldown;
    public bool isReloading;
    public Dictionary<string, Weapon> weaponInventory = new();
    public int Ammo
    {
        get => currentWeapon.ammo;
        set
        {
            currentWeapon.ammo = Mathf.Clamp(value, 0, currentWeapon.data.clipSize);
        }
    }
    void Start()
    {
        Ammo = currentWeapon.data.clipSize;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Mouse.current.leftButton.isPressed) // left click
        {
            if (gameManager.playerState != MenuState.Playing)
            {
                return;
            }
            if (Ammo <= 0 || isReloading)
            {
                return;
            }
            StartCoroutine(HandleFire(1f));
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) // reload
        {
            if (Ammo >= currentWeapon.data.clipSize)
            {
                return;
            }
            if (!isReloading)
            {
                StartCoroutine(HandleReload());
            }
        }
    }

    private IEnumerator HandleFire(float cooldown = 0.5f)
    {
        if (isOnCooldown)
        {
            yield break;
        }
        Ammo -= 1;
        isOnCooldown = true;
        
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    private IEnumerator HandleReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(currentWeapon.data.reloadTime);
        Ammo = currentWeapon.data.clipSize;
        isReloading = false;
    }

    
}
