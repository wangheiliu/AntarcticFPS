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
using System.Text.Json;

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
    [SerializeField] private WeaponData defaultWeapon;

    private List<Weapon> weapons = new();
    private Dictionary<Weapon, VisualElement> hotbarSlotsList = new();
    private Dictionary<WeaponType, (Weapon weapon, VisualElement element)> weaponTypeToSlot = new();
    public List<GameObject> weaponModels = new();
    private readonly Key[] numKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0 };
    private Dictionary<Weapon, PickableWeapon> pickableWeapons = new();
    private Weapon currentWeaponEquipped;
    private Weapon currentEquippedPickableWeapon;

    private VisualElement root;
    private VisualElement container;
    private int currVersion = -1;
    private bool canDropWeapon = false;


    void Awake()
    {
        weapons = new List<Weapon> { primaryWeapon, secondaryWeapon, tools };
    }
    void OnEnable()
    {
        hotbarUIDocument.RegisterUIReloadCallback(OnUIReload);
        InventoryScript.OnInventoryItemChanged += OnInventoryChanged;
    }

    void OnDisable()
    {
        hotbarUIDocument.UnregisterUIReloadCallback(OnUIReload);
        InventoryScript.OnInventoryItemChanged -= OnInventoryChanged;
    }

    void OnUIReload(PanelRenderer renderer, VisualElement _root, int version)
    {
        if (currVersion == version) return;
        currVersion = version;

        root = _root;
        container = root.Q<VisualElement>("inventory-container");
        root.style.display = DisplayStyle.Flex;
        PickableWeapon.OnWeaponPickup += OnWeaponPickUp;

        
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

    async Awaitable Update()
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

        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            await DropWeapon(currentWeaponEquipped);
        }
    }

    public void FillHotbar()
    {
        if (root == null) return;
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
            WeaponType type = (WeaponType)(i + 1);
            if (weaponTypeToSlot.TryGetValue(type, out var tuple))
            {
                tuple.element = item;
            }

            hotbarSlotsList.TryAdd(weapons[i], item);
            
            Debug.Log(type);
            weaponTypeToSlot.TryAdd(type, (weapons[i], item)); // try to fix the empty dictionary issue
            container.Add(item);
        }
    }

    private void EquipTransition(Weapon weapon)
    {
        foreach (var (_, slot) in hotbarSlotsList)
        {
            if (slot == null) continue;
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

        if (chosenElement == null) return;

        VisualElement backgroundElement = chosenElement.Q<VisualElement>("background");
        backgroundElement.style.backgroundColor = new StyleColor(new Color32(27, 27, 27, 255));
        backgroundElement.style.borderBottomWidth = 5;

        weaponSystem.StopReloading();
        weaponSystem.CurrentWeapon = weapon;
        currentWeaponEquipped = weapon;
        EquipWeaponModel(weapon.data.modelName);
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
            weapons = new List<Weapon> { primaryWeapon, secondaryWeapon, tools };
            
            if (tuple.element != null)
            {
                LoadSlotNames(tuple.element, tuple.weapon.data);
                hotbarSlotsList[weapon] = tuple.element;
            }
            EquipTransition(tuple.weapon);
        }
        else
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
        Debug.Log(modelName);
        GameObject weaponModel = weaponModels.Find(m => m.name == modelName);
        if (weaponModel != null)
        {
            weaponSystem.currentWeaponModel = weaponModel;
            weaponModel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Can't find model name");
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
            WeaponData weaponData = FindWeaponItem(data, type);

            if (weaponData == null)
            {
                canDropWeapon = false;
                return;
            }
            canDropWeapon = true;
            Weapon weapon = new(weaponData, weaponData.clipSize);
            Debug.Log($"Loaded weapon: {weaponData.Title}");

            weaponTypeToSlot.TryAdd(type, (weapon, null));
            hotbarSlotsList.TryAdd(weapon, null);
            if (weaponTypeToSlot.TryGetValue(type, out var tuple))
            {
                tuple.weapon = weapon;
            }

            EquipWeapon(weapon);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: failed to load weapons. Error Log: {e}");
        }
    }

    private WeaponData FindWeaponItem(PlayerData data, WeaponType type)
    {
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
        return weaponData;
    }

    private async Awaitable DropWeapon(Weapon weapon)
    {
        if (pickableWeapons.TryGetValue(weapon, out var pickableWeaponComponent))
        {
            await LoadWeaponFromInventory(weapon.data.weaponType);
            if (!canDropWeapon) return;
            pickableWeaponComponent.InvokeEnableProximityEvent(true);
            pickableWeapons.Remove(weapon);
            canDropWeapon = false;
        }
    }

    private async void OnInventoryChanged(WeaponData data)
    {
        Debug.Log($"Recieved {data.Title}");
        await LoadWeaponFromInventory(data.weaponType);
    }

    private void OnWeaponPickUp(PickableWeapon main, Weapon weapon)
    {
        if (pickableWeapons.TryAdd(weapon, main))
        {
            main.transform.SetParent(weaponContainer.transform, worldPositionStays: false);
            main.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0,180,0));
            Debug.Log(main.transform.position);
            weaponModels.Add(main.gameObject);
            currentEquippedPickableWeapon = weapon;
            EquipWeapon(weapon);
        }
    }
    
}
