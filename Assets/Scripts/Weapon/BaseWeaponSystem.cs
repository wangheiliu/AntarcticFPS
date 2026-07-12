using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseWeaponSystem : MonoBehaviour
{
    
    [SerializeField] protected WeaponData currentWeapon;
    [Header("Weapon Information")]
    public int ammo;
    public bool weaponEquipped;
    private bool isOnCooldown;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!weaponEquipped)
        {
            return;
        }
        if (Mouse.current.leftButton.isPressed) // left click
        {
            StartCoroutine(HandleFire(1f));
        }
    }

    private IEnumerator HandleFire(float cooldown = 0.5f)
    {
        if (isOnCooldown)
        {
            yield break;
        }
        ammo -= 1;
        isOnCooldown = true;
        
        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }
}
