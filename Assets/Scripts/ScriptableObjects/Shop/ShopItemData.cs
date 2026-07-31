using UnityEngine;

public abstract class ShopItemData : ScriptableObject
{
    public string title;
    
    [StatDisplay("Type", null, "Weapon Information")]
    public Catagories type;
    [StatDisplay("Cost", "$", "Weapon Information")]
    public int cost;
    public string shortDescription;
    [StatDisplay("Description", null, "Description")]
    public string description;
    public Sprite icon;
    public string dataName;
}
