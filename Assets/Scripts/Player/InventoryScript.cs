using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryScript : MonoBehaviour
{
    [SerializeField] private UIDocument inventoryUIDocument;
    [SerializeField] private VisualTreeAsset inventoryItemUIDocument;
    [Header("Weapon Info")]
    [SerializeField] private WeaponData primaryWeapon;
    [SerializeField] private WeaponData secondaryWeapon;
    [SerializeField] private WeaponData tools;

    private VisualElement root;
    private VisualElement container;
    void OnEnable()
    {
        root = inventoryUIDocument.rootVisualElement;       
        container = root.Q<VisualElement>("inventory-container");
        FillInventory(); 
    }

    void Update()
    {
        
    }

    public void FillInventory()
    {
        container.Clear();
        for (int i = 0; i < 3; i++)
        {
            VisualElement item = inventoryItemUIDocument.Instantiate();
            Debug.Log(item);
            TextElement itemName = item.Q<TextElement>("title");
            TextElement inputNumber = item.Q<TextElement>("input-label");
            inputNumber.text = (i+1).ToString();
            itemName.text = i switch //simplified switch statement to assign the correct weapon name based on the index
            {
                0 => primaryWeapon.name,
                1 => secondaryWeapon.name,
                2 => tools.name,
                _ => ""
            };
            container.Add(item);
        }
    }
}
