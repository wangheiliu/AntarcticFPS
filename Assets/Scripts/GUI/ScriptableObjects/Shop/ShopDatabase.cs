using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Shop/Database")] public class ShopDatabase : ScriptableObject
{
    public List<ShopItemData> itemList;
}
