using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Game.ScriptableObjects.Shop;

public enum WeaponType
{
    Primary,
    Secondary,
    Tools
}
[CreateAssetMenu(menuName = "Shop/Weapon")] public class WeaponData : ShopItemData //is this inheritence?
{
    [Header("Weapon Information")]
    [StatDisplay(displayName: "Clip Size", group: "Weapon Information")]
    public int clipSize;
    [StatDisplay(displayName: "Weapon type", group: "Weapon Information")]
    public WeaponType weaponType;

    [Header("Weapon stats")]
    [StatDisplay(displayName: "Fire Rate", group: "Weapon Information", unit: "RPM")]
    public int fireRate;
    [StatDisplay(displayName: "Reload Time", unit: "s", group: "Weapon Information")]
    public float reloadTime;
    [StatDisplay(displayName: "Damage", group: "Weapon Information")]
    public float damage;
    public int ammo;
    [Header("Weapon Metadata")]
    public string modelName;
    
}
