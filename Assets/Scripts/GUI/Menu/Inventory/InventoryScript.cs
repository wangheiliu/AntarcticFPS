using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Player.PlayerData;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryScript : MonoBehaviour
{
    private static string primaryWeaponTabName = "primary-weapons-tab";
    private static string secondaryWeaponTabName = "secondary-weapons-tab";
    private static string toolsWeaponTab = "tertiary-weapons-tab";
    [SerializeField] private GameManager mainMenuScript;
    [SerializeField] private UIDocument document;
    [SerializeField] private VisualTreeAsset propertyTree;
    [SerializeField] private ShopDatabase weaponDatabase;
    // Equip section
    private VisualElement equipSection;
    private VisualElement weaponsEquipElement;

    // Inventory section
    private VisualElement inventoryContainer;
    private VisualElement inventoryContentContainer;
    private VisualElement inventoryElement;
    private VisualElement weaponProperties;
    private VisualElement propertiesContainer;

    // Properties Section
    private Label propertyTitle;
    private Image propertyImage;
    private ScrollView propertiesScrollView;
    private Button equipButton;
    private Button closeBtn;
    private Button propertiesCloseBtn;
    private ShopItemData currentItem; // remember to get the tabview and the tab names

    // Weapons Tab
    private TabView weaponTabView;

    private Dictionary<string, List<StatValue>> groups = new();
    private Dictionary<string, WeaponsInventory> slotDictionary = new();

    private bool _isPropertyOpen = false;
    public bool IsPropertyOpen
    {
        get => _isPropertyOpen;
        set
        {
            if (_isPropertyOpen == value)
            {
                return;
            }

            _isPropertyOpen = value;

            if (propertiesContainer == null)
            {
                return;
            }

            propertiesContainer.style.display = DisplayStyle.Flex;

            if (_isPropertyOpen)
            {
                Debug.Log("Opening...");
                Debug.Log(propertiesContainer.style.display);
                propertiesContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
            }
            else
            {
                Debug.Log("Closing...");
                propertiesContainer.style.translate = new Translate(Length.Percent(110), 0, 0);
            }
        }
    }

    private bool _isInventoryOpen = false;
    public bool IsInventoryOpen
    {
        get => _isInventoryOpen;
        set
        {
            if (_isInventoryOpen == value)
            {
                return;
            }

            _isInventoryOpen = value;

            if (value)
            {
                document.rootVisualElement.style.display = DisplayStyle.Flex;
                inventoryContentContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
                equipSection.style.translate = new Translate(Length.Percent(0), 0, 0);
            } else
            {
                inventoryContentContainer.style.translate = new Translate(Length.Percent(110), 0, 0);
                equipSection.style.translate = new Translate(Length.Percent(-110), 0, 0);
                IsPropertyOpen = false;
            }
        }
    }
    void OnEnable()
    {
        var root = document.rootVisualElement;
        equipSection = root.Q<VisualElement>("equip-section");
        weaponsEquipElement = equipSection.Q<VisualElement>("weapon-equip-container");

        inventoryContainer = root.Q<VisualElement>("inventory-container");
        inventoryContentContainer = inventoryContainer.Q<VisualElement>("inventory-content-container");
        inventoryElement = inventoryContainer.Q<VisualElement>("inventory");
        closeBtn = inventoryContainer.Q<Button>("close-button");
        propertiesContainer = inventoryContainer.Q<VisualElement>("properties");

        weaponProperties = propertiesContainer.Q<VisualElement>("weapon-properties-container");
        propertyTitle = weaponProperties.Q<Label>();
        propertyImage = weaponProperties.Q<Image>();
        propertiesScrollView = weaponProperties.Q<ScrollView>();
        equipButton = propertiesContainer.Q<Button>("equip-button");
        propertiesCloseBtn = propertiesContainer.Q<Button>("prop-close-button");

        weaponTabView = inventoryContainer.Q<TabView>("weapon-inner-tab-view");

        equipButton.RegisterCallback<ClickEvent>(OnEquipButtonClick);
        propertiesCloseBtn.RegisterCallback<ClickEvent>(OnPropertyClose);
        closeBtn.RegisterCallback<ClickEvent>(evt =>
        {
            IsInventoryOpen = false;
        }, CallbackOptions.Removable);

        foreach (var children in weaponsEquipElement.Children())
        {
            Button unequipButton = children.Q<Button>("slot-close-button");
            unequipButton?.RegisterCallback<ClickEvent>(OnUnequipButtonClick);
        }

        propertiesContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        equipSection.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        inventoryContentContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);


        GeneralPlayerDataManager.OnWeaponEquipChanged += OnEquipWeapon;
        GeneralPlayerDataManager.OnWeaponInventoryChanged += PopulateInventorySlots;
    }

    void OnDisable()
    {
        equipButton.UnregisterAllRemovableCallbacks();
        propertiesContainer.UnregisterAllRemovableCallbacks();
        propertiesCloseBtn.UnregisterAllRemovableCallbacks();
        closeBtn.UnregisterAllRemovableCallbacks();
        inventoryContainer.UnregisterAllRemovableCallbacks();
        GeneralPlayerDataManager.OnWeaponEquipChanged -= OnEquipWeapon;
        GeneralPlayerDataManager.OnWeaponInventoryChanged -= PopulateInventorySlots;

        foreach (var children in weaponsEquipElement.Children())
        {
            Button unequipButton = children.Q<Button>("slot-close-button");
            unequipButton?.UnregisterAllRemovableCallbacks();
        }
    }

    void Start()
    {
        // document.rootVisualElement.style.display = DisplayStyle.None;
        // propertiesContainer.style.display = DisplayStyle.None;
        
        OnEquipWeapon(GeneralPlayerDataManager.PrimaryWeapon, WeaponType.Primary);
        slotDictionary["primary"] = GeneralPlayerDataManager.PrimaryWeapon;
        OnEquipWeapon(GeneralPlayerDataManager.SecondaryWeapon, WeaponType.Secondary);
        slotDictionary["secondary"] = GeneralPlayerDataManager.SecondaryWeapon;
        OnEquipWeapon(GeneralPlayerDataManager.ToolWeapon, WeaponType.Tools);
        slotDictionary["tools"] = GeneralPlayerDataManager.ToolWeapon;
        PopulateInventorySlots();
    }

    void Update()
    {

    }

    void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == propertiesContainer)
        {
            if (!IsPropertyOpen)
            {
                Debug.Log("Property is closed");
                propertiesContainer.style.display = DisplayStyle.None;
            } else {
                propertiesContainer.style.display = DisplayStyle.Flex;
            }
        } else if (evt.target == equipSection || evt.target == inventoryContentContainer)
        {
            if (!IsInventoryOpen)
            {
                equipSection.style.display = DisplayStyle.None;
                inventoryContentContainer.style.display = DisplayStyle.None;
                document.rootVisualElement.style.display = DisplayStyle.None;
                mainMenuScript.OpenMenuItems(MenuState.MainMenu);
            }
        }
    }

    private void PopulateInventorySlots()
    {
        foreach (VisualElement element in weaponTabView.Children())
        {
            Tab tab = element as Tab;
            if (tab == null)
            {
                continue;
            }

            foreach (VisualElement e in tab.Q<ScrollView>(className: "inventory-scroll-view").Children())
            {
                Button btn = e.Q<Button>(className: "weapon-inventory__btn");
                btn?.UnregisterAllRemovableCallbacks();
            }

            tab.Q<ScrollView>(className: "inventory-scroll-view").Clear();

        }
        var inventory = GeneralPlayerDataManager.WeaponsOwned;
        if (inventory == null)
        {
            return;
        }
        foreach (WeaponsInventory weapon in inventory)
        {
            var weaponData = GetWeaponData(weapon) as WeaponData;
            if (weapon == null)
            {
                continue;
            }
            string tabName = GetWeaponTabName(weaponData);
            Tab selectedTab = weaponTabView.Q<Tab>(tabName);

            if (selectedTab == null)
            {
                return;
            }

            VisualElement element = new();
            element.AddToClassList("weapon-inventory-card");
            Label title = new()
            {
                text = weaponData.title
            };
            title.AddToClassList("weapon-inventory__title");
            Image image = new();
            image.AddToClassList("weapon-inventory__image");
            Button viewButton = new()
            {
                text = "View"
            };
            viewButton.AddToClassList("weapon-inventory__btn");
            viewButton.AddToClassList("white-button-transition");

            element.Add(title);
            element.Add(image);
            element.Add(viewButton);

            viewButton.RegisterCallback<ClickEvent>(evt => {
                IsPropertyOpen = true;
                DisplayProperty(evt, weaponData);
            }, CallbackOptions.Removable);

            Debug.Log(selectedTab.name);
            ScrollView container = selectedTab.Q<ScrollView>(className: "inventory-scroll-view");
            container?.Add(element);
        }
    }

    private void DisplayProperty(ClickEvent evt, ShopItemData data)
    {
        if (data == null)
        {
            return;
        }
        groups.Clear();
        propertiesScrollView.Clear();

        currentItem = data;

        propertyTitle.text = data.title;

        if (data is WeaponData weaponData)
        {
            equipButton.text = weaponData.weaponType switch
            {
                WeaponType.Primary => "Equip Primary",
                WeaponType.Secondary => "Equip Secondary",
                WeaponType.Tools => "Equip Tool",
                _ => "Equip",
            };
        }
        else
        {
            equipButton.text = "Equip";
        }


        CheckEquipped(GeneralPlayerDataManager.WeaponsOwned.Find(m => m.weaponName == data.dataName));

        FieldInfo[] fields = data.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy); // use GetType() to include child classes

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<StatDisplay>();
            if (attribute == null)
            {
                continue;
            }

            string statName = attribute.DisplayName;
            string unit = attribute.Unit;
            object value = field.GetValue(data);
            string group = attribute.Group;

            if (string.IsNullOrWhiteSpace(group))
            {
                continue;
            }

            if (!groups.ContainsKey(group))
            {
                groups[group] = new();
            }

            groups[group].Add(new()
            {
                Name = statName,
                Unit = unit,
                Value = value
            });


        }

        foreach (var i_group in groups)
        {
            var infoClone = propertyTree.Instantiate();
            VisualElement cardRoot = infoClone.Q<VisualElement>("info-card");
            Label infoTitleElement = infoClone.Q<Label>("info-subtitle");
            Label infoTextTemplate = infoClone.Q<Label>("information");

            infoTitleElement.text = i_group.Key;

            foreach (var i_value in i_group.Value)
            {
                Label label = new()
                {
                    text = $"{i_value.Name}: {i_value.Display}"
                };
                label.AddToClassList("information");
                cardRoot.Add(label);
            }
            infoTextTemplate.RemoveFromHierarchy();
            propertiesScrollView.Add(infoClone);
        }
    }



    private void EquipWeapon(ShopItemData item, string slotName)
    {

        if (slotName == "primary-slot")
        {
            GeneralPlayerDataManager.PrimaryWeapon = GeneralPlayerDataManager.WeaponsOwned.Find(m => m.weaponName == item.dataName);
            OnEquipWeapon(GeneralPlayerDataManager.PrimaryWeapon, WeaponType.Primary);
            slotDictionary["primary"] = GeneralPlayerDataManager.PrimaryWeapon;
        }
        else if (slotName == "secondary-slot")
        {
            GeneralPlayerDataManager.SecondaryWeapon = GeneralPlayerDataManager.WeaponsOwned.Find(m => m.weaponName == item.dataName );
            OnEquipWeapon(GeneralPlayerDataManager.PrimaryWeapon, WeaponType.Secondary);
            slotDictionary["secondary"] = GeneralPlayerDataManager.SecondaryWeapon;
        }
        else if (slotName == "tertiary-slot")
        {
            GeneralPlayerDataManager.ToolWeapon = GeneralPlayerDataManager.WeaponsOwned.Find(m => m.weaponName == item.dataName);
            OnEquipWeapon(GeneralPlayerDataManager.PrimaryWeapon, WeaponType.Tools);
            slotDictionary["tools"] = GeneralPlayerDataManager.ToolWeapon;
        }

        CheckEquipped(GeneralPlayerDataManager.WeaponsOwned.Find(m => m.weaponName == item.dataName));
        CurrentPlayerData.Save();
    }

    private void UnequipWeapon(string slotName)
    {
        switch (slotName)
        {
            case "primary-slot":
                GeneralPlayerDataManager.PrimaryWeapon = null;
                break;
            case "secondary-slot":
                GeneralPlayerDataManager.SecondaryWeapon = null;
                break;
            case "tertiary-slot":
                GeneralPlayerDataManager.ToolWeapon = null;
                break;
            default:
                return;
        }
        CheckEquipped(null);
        CurrentPlayerData.Save();
    }

    private void OnWeaponUnequip(WeaponsInventory inventory, VisualElement slot)
    {
        Debug.Log("Running");
        string key = slotDictionary.FirstOrDefault(x => x.Value.weaponName == inventory.weaponName).Key;
        if (key == null)
        {
            return;
        }
        Label placeholder = slot.Q<Label>("placeholder-text");
        foreach (var element in slot.Children())
        {
            element.style.display = DisplayStyle.None;
        }
        if (placeholder != null)
        {
            placeholder.style.display = DisplayStyle.Flex;
        }
        slotDictionary[key] = null;
        inventory = null;

        UnequipWeapon(slot.name);
    }

    private void OnEquipWeapon(WeaponsInventory item, WeaponType type)
    {
        
        if (item == null)
        {
            return;
        }
        VisualElement element = equipSection.Q<VisualElement>(GetWeaponSlotName(type));
        var data = GetWeaponData(item);
        if (data == null)
        {
            foreach (var child in element.Children())
            {
                child.style.display = DisplayStyle.None;
            }
            Label placeholder = element.Q<Label>("placeholder-text");
            placeholder.style.display = DisplayStyle.Flex;
            return;
        }

        if (data is WeaponData data1)
        {
            foreach (var child in element.Children())
            {
                child.style.display = DisplayStyle.Flex;
            }
            Label label = element.Q<Label>("item-title");
            Label placeholder = element.Q<Label>("placeholder-text");
            placeholder.style.display = DisplayStyle.None;

            label.text = data.title;
        }

    }
    private string GetWeaponTabName(ShopItemData inventory)
    {
        WeaponData data = inventory as WeaponData;
        if (data == null)
        {
            return null;
        }

        string type = Enum.GetName(typeof(WeaponType), data.weaponType);

        return type switch
        {
            "Primary" => primaryWeaponTabName,
            "Secondary" => secondaryWeaponTabName,
            "Tools" => toolsWeaponTab,
            _ => null,
        };
    }

    private string GetWeaponSlotName(WeaponType type)
    {
        return type switch
        {
            WeaponType.Primary => "primary-slot",
            WeaponType.Secondary => "secondary-slot",
            WeaponType.Tools => "tertiary-slot",
            _ => null,
        };
    }

    private void CheckEquipped(WeaponsInventory data)
    {
        if (data == null)
        {
            equipButton.SetEnabled(true);
            return;
        }
        WeaponData weaponData = GetWeaponData(data) as WeaponData;
        string slot = GetWeaponSlotName(weaponData.weaponType);
        if (slot == null)
        {
            equipButton.SetEnabled(true);
            return;
        }

        var match = slotDictionary.FirstOrDefault(x => x.Value != null && x.Value.weaponName == weaponData.dataName);
        string key = match.Key;

        VisualElement element = equipSection.Q<VisualElement>(slot);
        Label title = element.Q<Label>("item-title");

        if (key != null)
        {
            if (slotDictionary[key].weaponName == weaponData.dataName)
            {
                equipButton.SetEnabled(false);
            }
            else
            {
                equipButton.SetEnabled(true);
            }
        }
        else
        {
            equipButton.SetEnabled(true);
        }

    }

    private void OnUnequipButtonClick(ClickEvent evt)
    {
        if (evt.target is not VisualElement element)
            return;
        element = element.parent;
        Label title = element.Q<Label>("item-title");

        if (title == null)
        {
            Debug.LogWarning("no title element");
            return;
        }

        var itemRequested = weaponDatabase.itemList.Find(m => title.text == m.title);
        if (itemRequested == null)
        {
            Debug.LogWarning("Item doesn't exist");
            return;
        }
        var item = GeneralPlayerDataManager.WeaponsOwned.Find(m => m.weaponName == itemRequested.dataName);
        if (item == null)
        {
            return;
        }
        OnWeaponUnequip(item, element);
    }

    private void OnEquipButtonClick(ClickEvent evt)
    {
        WeaponData data = currentItem as WeaponData;
        if (data != null)
        {
            EquipWeapon(currentItem, GetWeaponSlotName(data.weaponType));
        }
    }

    private void OnPropertyClose(ClickEvent _)
    {
        IsPropertyOpen = false;
    }

    private ShopItemData GetWeaponData(WeaponsInventory inventory)
    {
        return weaponDatabase.itemList.Find(m => m.dataName == inventory.weaponName);
    }

    public void OpenInventory() {
        equipSection.style.display = DisplayStyle.Flex;
        inventoryContentContainer.style.display = DisplayStyle.Flex;
        IsInventoryOpen = true;
    }
}
