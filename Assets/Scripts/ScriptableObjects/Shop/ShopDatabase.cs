using UnityEngine;
using System.Collections.Generic;

namespace Game.ScriptableObjects.Shop
{
    [CreateAssetMenu(menuName = "Shop/Database")]
    public class ShopDatabase : ScriptableObject
    {
        public List<ShopItemData> itemList;
    }
}

