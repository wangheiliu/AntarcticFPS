using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

public class HotbarScript : MonoBehaviour
{
    [SerializeField] private BaseWeaponSystem weaponSystem;
    [SerializeField] private GameObject weaponContainer;
    [Header("GUI")]
    [SerializeField] private UIDocument hotbarUIDocument;
    [SerializeField] private VisualTreeAsset hotbarItemUIDocument;
    [Header("Weapon Info")]
    [SerializeField] private Weapon primaryWeapon;
    [SerializeField] private Weapon secondaryWeapon;
    [SerializeField] private Weapon tools;
    private List<Weapon> weapons = new();
    private Dictionary<Weapon, VisualElement> hotbarSlotsList = new();
    public List<GameObject> weaponModels = new();
    private readonly Key[] numKeys = {Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0};


    private VisualElement root;
    private VisualElement container;

    void Awake()
    {
        weapons = new List<Weapon> {primaryWeapon, secondaryWeapon, tools};
    }
    void OnEnable()
    {
        root = hotbarUIDocument.rootVisualElement;       
        container = root.Q<VisualElement>("inventory-container");
        FillHotbar();
        
    }

    void Start()
    {
        foreach (Transform child in weaponContainer.transform)
        {
            GameObject childObj = child.gameObject;
            Debug.Log(childObj);
            Debug.Log(weaponContainer);
            try
            {
                weaponModels.Add(childObj);
            } catch
            {
                continue;
            }
        }

        foreach (Weapon weapon in weapons)
        {
            weapon.ammo = weapon.data.clipSize;
        }

        EquipTransition(weapons[0]);
    }

    void Update()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (Keyboard.current[numKeys[i]].wasPressedThisFrame)
            {
                Debug.Log($"Equipped: {weapons[i].data.name}");
                weaponSystem.currentWeapon = weapons[i];
                EquipTransition(weaponSystem.currentWeapon);
                break;
            }
        }
    }

    public void FillHotbar()
    {
        container.Clear();
        hotbarSlotsList.Clear();
        for (int i = 0; i < weapons.Count; i++)
        {
            VisualElement item = hotbarItemUIDocument.Instantiate();
            TextElement itemName = item.Q<TextElement>("title");
            TextElement inputNumber = item.Q<TextElement>("input-label");
            inputNumber.text = (i+1).ToString();
            itemName.text = weapons[i].data.name;
            
            hotbarSlotsList.Add(weapons[i], item);
            container.Add(item);
        }
    }

    private void EquipTransition(Weapon weapon)
    {
        foreach (var (_, slot) in hotbarSlotsList)
        {
            VisualElement background = slot.Q<VisualElement>("background");
            background.style.backgroundColor = new StyleColor(new Color32(27, 27, 27, 174));
            background.style.borderBottomWidth = 0;
        }

        VisualElement chosenElement = hotbarSlotsList[weapon];
        if (!hotbarSlotsList.TryGetValue(weapon, out VisualElement element)) 
            return;
        
        VisualElement backgroundElement = chosenElement.Q<VisualElement>("background");
        backgroundElement.style.backgroundColor = new StyleColor(new Color32(27,27,27,255));
        backgroundElement.style.borderBottomWidth = 5;

        if (weaponSystem.isReloading)
        {
            if (weaponSystem.coroutine != null)
            {
                weaponSystem.StopCoroutine(weaponSystem.coroutine); // when stopping coroutines, it must be used in the same monobehavior instance
            }
            weaponSystem.isReloading = false;
        }

        
        weaponSystem.currentWeapon = weapon;

        foreach (GameObject weaponModel in weaponModels)
        {
            weaponModel.SetActive(false);
        }

        if (weapon.data.modelName == null)
        {
            return;
        }

        GameObject weaponModelToEnable = weaponModels.Find(f => f.name == weapon.data.modelName);
        if (weaponModelToEnable != null)
        {
            weaponSystem.currentWeaponModel = weaponModelToEnable;
            weaponModelToEnable.SetActive(true);
        } else
        {
            weaponSystem.currentWeaponModel = null;
        }
    }

    public void AddItem(Weapon weapon)
    {
        weapons.Add(weapon);
        FillHotbar();
    }

    public void RemoveItem(Weapon weapon)
    {
        weapons.Remove(weapon);
        FillHotbar();
    }
}
