using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Weapon")] public class WeaponData : ShopItemData //is this inheritence?
{
    [StatDisplay("Clip Size", null, "Weapon Information")]
    public int clipSize;
    [StatDisplay("Fire Rate", null, "Weapon Information")]
    public int fireRate;
    [StatDisplay("Reload Time", "s", "Weapon Information")]
    public float reloadTime;
    [StatDisplay("Damage", null, "Weapon Information")]
    public float damage;
    public int ammo;
}
