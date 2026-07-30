using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class Weapon
{
    public WeaponData data;
    public int ammo;
}
public class BaseWeaponSystem : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private GameManager gameManager;
    [SerializeField] public Weapon currentWeapon;
    public GameObject currentWeaponModel;
    [SerializeField] private Camera gameCam;
    [Header("Weapon Information")]
    [SerializeField] private HotbarScript hotbarScript;
    public static bool isWeaponEquipped;
    public bool isOnCooldown;
    public bool isReloading;
    public Dictionary<string, Weapon> weaponInventory = new();

    public Coroutine coroutine;
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
        if (gameManager.playerState != MenuState.Playing)
        {
            return;
        }
        if (Mouse.current.leftButton.isPressed) // left click
        {
            if (Ammo <= 0 || isReloading)
            {
                return;
            }
            StartCoroutine(HandleFire(currentWeapon.data.fireRate / 60f));
        }

        FaceWeaponOrientation();

        if (Keyboard.current.rKey.wasPressedThisFrame) // reload
        {
            if (Ammo >= currentWeapon.data.clipSize)
            {
                return;
            }
            if (!isReloading)
            {
                coroutine = StartCoroutine(HandleReload());
            }
        }
    }

    public virtual IEnumerator HandleFire(float cooldown = 0.5f)
    {
        if (isOnCooldown || gameManager.playerState != MenuState.Playing)
        {
            yield break;
        }
        if (currentWeaponModel == null)
        {
            yield break;
        }
        Transform muzzle = currentWeaponModel.transform.Find("Barrel");
        Ray camRay = gameCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(camRay, out RaycastHit hit, 300f))
        {
            Vector3 muzzleDir = (hit.point - muzzle.position).normalized;
            Ray ray = new(muzzle.position, muzzleDir);
            if (Physics.Raycast(ray, out RaycastHit targetHit, 300f))
            {
                if (IsHittingOwnChar(targetHit))
                {
                    yield break;
                }

                HUDScript healthManager = hit.transform.GetComponentInChildren<HUDScript>();
                if (healthManager != null)
                {
                    healthManager.DamageHealth(currentWeapon.data.damage);
                }
            }
        }



        Debug.DrawRay(muzzle.position, Mouse.current.position.ReadValue(), Color.red);
        Ammo -= 1;
        isOnCooldown = true;

        yield return new WaitForSeconds(cooldown);
        isOnCooldown = false;
    }

    public IEnumerator HandleReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(currentWeapon.data.reloadTime);
        Ammo = currentWeapon.data.clipSize;
        isReloading = false;
    }

    public void FaceWeaponOrientation()
    {
        GameObject weaponModel = hotbarScript.weaponModels.Find(f => f.name == currentWeapon.data.modelName);
        if (weaponModel == null)
        {
            return;
        }

        if (gameCam.enabled == false) // if no camera in a tag is enabled, returns nullreferenceexception
        {
            weaponModel.transform.rotation = Quaternion.Euler(0, 180, 0);
            return;
        }

        Ray ray = gameCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Vector3 targetPos = hit.point - weaponModel.transform.position;
            weaponModel.transform.rotation = Quaternion.LookRotation(targetPos) * Quaternion.Euler(0, 180, 0);
        }
    }

    public bool IsHittingOwnChar(RaycastHit hit)
    {
        return hit.transform.root == playerCharacter.transform;
    }
}
