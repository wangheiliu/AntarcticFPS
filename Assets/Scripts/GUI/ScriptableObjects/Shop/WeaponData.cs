using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Weapon")] public class WeaponData : ShopItemData //is this inheritence?
{
    public int clipSize;
    public int fireRate;
    public float reloadTime;
    public float damage;
}
