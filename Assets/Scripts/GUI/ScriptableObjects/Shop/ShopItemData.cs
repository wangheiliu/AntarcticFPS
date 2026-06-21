using UnityEngine;

public abstract class ShopItemData : ScriptableObject
{
    public string title;
    public Catagories type;
    public int cost;
    public string shortDescription;
    public string description;
    public Sprite icon;
}
