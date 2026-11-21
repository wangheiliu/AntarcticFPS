using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;
using Game.Weapons;
using Game.Weapons.WeaponSystem;
using System;
using Unity.VisualScripting;
using Player.PlayerData;
using Game.ScriptableObjects.Shop;
using System.Threading.Tasks;

public class HotbarScript : MonoBehaviour
{
    [Header("Main Menu")]
    [SerializeField] private GameManager gameManager;

    [Header("Weapon System")]
    [SerializeField] private BaseWeaponSystem weaponSystem;
    [SerializeField] private ShopDatabase weaponDatabase;
    [SerializeField] private GameObject weaponContainer;

    [Header("GUI")]
    [SerializeField] private PanelRenderer hotbarUIDocument;
    [SerializeField] private VisualTreeAsset hotbarItemUIDocument;

    [Header("Weapon Info")]
    [SerializeField] private Weapon primaryWeapon;
    [SerializeField] private Weapon secondaryWeapon;
    [SerializeField] private Weapon tools;

    private List<Weapon> weapons = new();
    private Dictionary<Weapon, VisualElement> hotbarSlotsList = new();
    private Dictionary<WeaponType, (Weapon weapon, VisualElement element)> weaponTypeToSlot = new();
    public List<GameObject> weaponModels = new();
    private readonly Key[] numKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0 };


    private VisualElement root;
    private VisualElement container;
    private int currVersion = -1;


    void Awake()
    {
        weapons = new List<Weapon> { primaryWeapon, secondaryWeapon, tools };
    }
    void OnEnable()
    {
        hotbarUIDocument.RegisterUIReloadCallback(OnUIReload);
    }

    void OnDisable()
    {
        hotbarUIDocument.UnregisterUIReloadCallback(OnUIReload);
        InventoryScript.OnInventoryItemChanged -= OnInventoryChanged;
    }

    async void OnUIReload(PanelRenderer renderer, VisualElement _root, int version)
    {
        if (currVersion == version) return;
        currVersion = version;

        root = _root;
        container = root.Q<VisualElement>("inventory-container");
        root.style.display = DisplayStyle.Flex;
        PickableWeapon.OnWeaponPickup += OnWeaponPickUp;

        InventoryScript.OnInventoryItemChanged += OnInventoryChanged;
        FillHotbar();
        EquipTransition(weapons[0]);
    }

    async Awaitable Start()
    {

        foreach (Transform child in weaponContainer.transform)
        {
            GameObject childObj = child.gameObject;
            try
            {
                weaponModels.Add(childObj);
            }
            catch
            {
                continue;
            }
        }

        try
        {
            foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
            {
                if (type == WeaponType.None) continue;
                await LoadWeaponFromInventory(type);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load weapon from inventory. Error message: {e}");
            return;
        }

        foreach (Weapon weapon in weapons)
        {
            weapon.ammo = weapon.data.clipSize;
        }


    }

    void Update()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (Keyboard.current[numKeys[i]].wasPressedThisFrame)
            {
                if (gameManager.playerState != MenuState.Playing)
                {
                    return;
                }
                weaponSystem.CurrentWeapon = weapons[i];
                EquipTransition(weaponSystem.CurrentWeapon);
                break;
            }
        }
    }

    public void FillHotbar()
    {
        container.Clear();
        hotbarSlotsList.Clear();
        weaponTypeToSlot.Clear();
        for (int i = 0; i < weapons.Count; i++)
        {
            VisualElement item = hotbarItemUIDocument.Instantiate();
            Label inputNumber = item.Q<Label>("input-label");
            inputNumber.text = (i + 1).ToString();
            if (weapons[i] == null)
            {
                Label itemName = item.Q<Label>("title");
                itemName.text = "Placeholder";
                continue;
            }
            LoadSlotNames(item, weapons[i].data);

            hotbarSlotsList.TryAdd(weapons[i], item);
            WeaponType type = (WeaponType)(i + 1);
            Debug.Log(type);
            weaponTypeToSlot.TryAdd(type, (weapons[i], item));
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

        VisualElement chosenElement = null;

        if (hotbarSlotsList.TryGetValue(weapon, out VisualElement element1))
        {
            chosenElement = element1;
        }
        else
        {
            Debug.LogWarning($"Can't get element in hotbar. Weapon name trying to get is {weapon.data.Title}");
            return;
        }

        VisualElement backgroundElement = chosenElement.Q<VisualElement>("background");
        backgroundElement.style.backgroundColor = new StyleColor(new Color32(27, 27, 27, 255));
        backgroundElement.style.borderBottomWidth = 5;

        weaponSystem.StopReloading();
        weaponSystem.CurrentWeapon = weapon;
        EquipWeaponModel(weapon.data.modelName);

        GameObject weaponModelToEnable = weaponModels.Find(f => f.name == weapon.data.modelName);
        if (weaponModelToEnable != null)
        {
            weaponSystem.currentWeaponModel = weaponModelToEnable;
            weaponModelToEnable.SetActive(true);
        }
        else
        {
            weaponSystem.currentWeaponModel = null;
        }
    }

    private void EquipWeapon(Weapon weapon)
    {
        if (weapon == null) return;
        WeaponData data = weapon.data;
        if (data == null) return;
        
        if (weaponTypeToSlot.TryGetValue(data.weaponType, out var tuple))
        {
            switch (data.weaponType)
            {
                case WeaponType.Primary:
                    primaryWeapon = weapon;
                    break;
                case WeaponType.Secondary:
                    secondaryWeapon = weapon;
                    break;
                case WeaponType.Tools:
                    tools = weapon;
                    break;
                default:
                    break;
            }

            tuple.weapon = weapon;
            Debug.Log("Equipping weapon: " + tuple.weapon.data.Title);
            weapons = new List<Weapon> { primaryWeapon, secondaryWeapon, tools };
            FillHotbar();
            EquipTransition(weapon);
        } else
        {
            Debug.LogWarning($"Can't extract value for {weapon.data.Title}");
        }
    }

    private void EquipWeaponModel(string modelName)
    {
        foreach (GameObject model in weaponModels)
        {
            model.SetActive(false);
        }
        if (modelName == null)
        {
            return;
        }
        GameObject weaponModel = weaponModels.Find(m => m.name == modelName);
        if (weaponModel != null)
        {
            weaponSystem.currentWeaponModel = weaponModel;
            weaponModel.SetActive(true);
        }
        else
        {
            weaponSystem.currentWeaponModel = null;
        }
    }

    private void LoadSlotNames(VisualElement element, WeaponData data)
    {
        if (element == null)
        {
            Debug.LogWarning("No element");
            return;
        }
        Label itemName = element.Q<Label>("title");
        if (itemName == null) return;
        itemName.text = data.name;
    }

    private async Awaitable LoadWeaponFromInventory(WeaponType type)
    {
        try
        {
            var data = await CurrentPlayerData.LoadDataAsync();
            Debug.Log($"Trying to find {type}...");
            ShopItemData shopItemData = weaponDatabase.itemList.Find((m) =>
            {
                return type switch
                {
                    WeaponType.Primary => m.dataName == data.primaryWeaponEquipped.weaponName,
                    WeaponType.Secondary => m.dataName == data.secondaryWeaponEquipped.weaponName,
                    WeaponType.Tools => m.dataName == data.toolsEquipped.weaponName,
                    _ => false,
                };
            });
            WeaponData weaponData = (WeaponData)shopItemData;
            
            if (weaponData == null) {
                Debug.LogWarning("Weapon Data not found");
                return;
            }
            Weapon weapon = new(weaponData, weaponData.clipSize);
            Debug.Log($"Loaded weapon: {weaponData.Title}");
            EquipWeapon(weapon);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: failed to load weapons. Error Log: ${e}");
        }
    }

    private async void OnInventoryChanged(WeaponData data)
    {
        Debug.Log($"Recieved {data.Title}");
        await LoadWeaponFromInventory(data.weaponType);
    }

    public void OnWeaponPickUp(WeaponData data)
    {

    }
}
