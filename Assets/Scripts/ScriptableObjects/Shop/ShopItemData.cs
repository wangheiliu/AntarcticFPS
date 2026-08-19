using UnityEngine;

public abstract class ShopItemData : ScriptableObject
{
    public string title;
    
    [StatDisplay("Type", null, "Basic Information")]
    public Catagories type;
    [StatDisplay("Cost", "$", "Basic Information")]
    public int cost;
    public string shortDescription;
    [StatDisplay("Description", null, "Description")]
    public string description;
    public Sprite icon;
    public string dataName;
}
