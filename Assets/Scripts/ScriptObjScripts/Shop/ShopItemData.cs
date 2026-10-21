using System.ComponentModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ScriptableObjects.Shop
{
    public abstract class ShopItemData : ScriptableObject
    {
        [Header("Basic Information")]
        public string title;
        [CreateProperty] public string Title
        {
            get => title;
            set => title = value;
        }

        [CreateProperty]
        [StatDisplay(displayName: "Type", group: "Basic Information")]
        public ShopItemCatagory type;

        [CreateProperty]
        [StatDisplay(displayName: "Cost", prefix: "$", group: "Basic Information")]
        public int cost;
        [CreateProperty] public string shortDescription;

        [TextArea(1, 10)]
        [StatDisplay(displayName: "Description", group: "Description")]
        [CreateProperty] public string description;
        [CreateProperty] public Image icon;
        public string dataName;
    }
}

