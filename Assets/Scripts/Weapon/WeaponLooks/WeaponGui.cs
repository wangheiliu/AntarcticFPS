using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class WeaponGui : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private BaseWeaponSystem weaponSystem;

    private VisualElement container;
    private TextElement nameLabel;
    private TextElement ammoLabel;
    private VisualElement root;

    private WeaponData currentWeapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        root = uiDocument.rootVisualElement;
        container = root.Q<VisualElement>("container");
        nameLabel = root.Q<TextElement>("weapon-name");
        ammoLabel = root.Q<TextElement>("ammo-count");
        
        weaponSystem.OnWeaponChanged += WeaponGuiUpdate;
        weaponSystem.OnWeaponReload += UpdateReloading;
    }

    // Update is called once per frame
    void Update()
    {
        if (weaponSystem.CurrentWeapon == null)
        {
            root.style.display = DisplayStyle.None;
            nameLabel.text = "No Weapon";
            ammoLabel.text = "0/0";
            return;
        } else
        {
            root.style.display = DisplayStyle.Flex;
        }
        ammoLabel.text = $"{weaponSystem.Ammo}/{weaponSystem.CurrentWeapon.data.clipSize}";
    }

    private void WeaponGuiUpdate(WeaponData weapon)
    {
        if (weapon == null) return;
        currentWeapon = weapon;
        nameLabel.text = currentWeapon.title;
    }

    private void UpdateReloading(bool isReloading)
    {
        if (currentWeapon == null) return;

        if (isReloading)
        {
            nameLabel.text = "Reloading...";
        } else
        {
            nameLabel.text = currentWeapon.title;
        }
    }
}
