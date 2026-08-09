using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.ComponentModel.Design.Serialization;
using Unity.VisualScripting;

public enum Catagories
{
    Any,
    Automatics,
    SMG,
    Shotguns,
    Pistols,
    Knives
    //Medkit
}
public class ShopData : MonoBehaviour
{
    [Header("ShopMenuScript")]
    [SerializeField] private ShopMenuScript shopMenuScript;
    //gui references
    [Header("GUI")]
    [SerializeField] private VisualTreeAsset shopCardTemplate;
    [SerializeField] private VisualTreeAsset infoCardTemplate;
    [SerializeField] private UIDocument shopDocument;
    [SerializeField] private ShopDatabase database;
    
    private VisualElement root;
    private ScrollView infoScrollContainer;
    private Button purchaseButton;
    private Button infoPurchaseButton;
    private Label purchasePromptTitle;
    private Label moneyDisplay;

    private Dictionary<string, Foldout> foldouts = new();
    public List<(Foldout, ShopItemData, VisualElement)> lookUpTable = new();
    private Action viewButtonLambda;

    private ShopItemData currentItem;
    Action CheckPurchasedLambda;
    
    void OnEnable()
    {
        root = shopDocument.rootVisualElement;
        infoScrollContainer = root.Q<ScrollView>("info-scroller");
        purchaseButton = root.Q<Button>("prompt-purchase-button");
        infoPurchaseButton = root.Q<Button>("purchase-button");
        purchasePromptTitle = root.Q<Label>("prompt-message-label");
        moneyDisplay = root.Q<Label>("money-counter");

        CheckPurchasedLambda = () => CheckItemPurchased(currentItem);

        purchaseButton.RegisterCallback<ClickEvent>(OnPurchaseButtonClicked);

        if (CheckPurchasedLambda != null)
        {
            ProgressionDataManager.OnProgressionDataChanged += CheckPurchasedLambda;
        }
        ProgressionDataManager.OnProgressionDataChanged += MoneyDisplay;
        
        
        PopulateUI(database);
        
    }

    void Start()
    {
        MoneyDisplay();
    }

    public void PopulateUI(ShopDatabase database)
    {
        //adds the foldouts first
        string[] catagories = Enum.GetNames(typeof(Catagories)).ToArray(); //select is basically .map() from JavaScript, nice
        foreach (string str in catagories)
        {
            if (str == "Any")
            {
                continue;
            }
            Foldout foldout = root.Q<Foldout>(str);
            if (foldout != null)
            {
                foldouts.TryAdd(str, foldout);
            }
        }

        foreach (ShopItemData item in database.itemList)
        {
            string itemType = item.type.ToString();

            if (itemType == null) return;

            
            // shop cards in foldouts
            var foldoutContainer = root.Q<Foldout>(itemType.ToLower());
            TemplateContainer templateUI = shopCardTemplate.Instantiate();
            Label title = templateUI.Q<Label>("Title");
            Label descriptionElement = templateUI.Q<Label>("Description");
            Image icon = templateUI.Q<Image>();
            Button viewButton = templateUI.Q<Button>();

            //information stuff
            

            title.text = item.title;
            descriptionElement.text = item.shortDescription;
            //icon.image = item.icon.texture;

            viewButtonLambda = () => OnViewClick(item);
            viewButton.clicked += viewButtonLambda;
            foldoutContainer.Add(templateUI);
            lookUpTable.Add((foldoutContainer, item, templateUI));
        }
    }

    public void ViewItemDetails(ShopItemData item)
    {
        infoScrollContainer.Clear();

        Type type = item.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        FieldInfo[] fields = type.GetFields(flags);

        Dictionary<string, List<StatValue>> groups = new();
        
        Label infoTitle = root.Q<Label>("info-title");
        infoTitle.text = item.title;
        purchasePromptTitle.text = $"Would you like to buy: {item.title ?? "item"}? (${item.cost})";
        currentItem = item;
        

        
        // gets the values from the item parameter
        foreach (FieldInfo field in fields)
        {
            
            StatDisplay statDisplay = field.GetCustomAttribute<StatDisplay>();
            if (statDisplay == null)
            {
                continue;
            }
            // check if value exists or not and add them to a List
            string statName = statDisplay.DisplayName;
            string unit = statDisplay.Unit;
            object value = field.GetValue(item);
            string group = statDisplay.Group;

            if (string.IsNullOrEmpty(group))
            {
                continue;
            }

            if (!groups.ContainsKey(group))
            {
                groups[group] = new();
            }

            groups[group].Add(new StatValue
            {
                Name = statName,
                Value = value,
                Unit = unit
            });
        }
        // use a foreach loop to clone these cards for the group
        foreach (var group in groups)
        {
            var infoCardClone = infoCardTemplate.Instantiate();
            VisualElement cardRoot = infoCardClone.Q<VisualElement>("info-card");
            Label infoTitleElement = infoCardClone.Q<Label>("info-subtitle");
            Label infoTextTemplate = infoCardClone.Q<Label>("information");

            infoTitleElement.text = group.Key; //gets the key
            foreach (var stat in group.Value)
            {
                Label infoTextClone = new()
                {
                    text = $"{stat.Name}: {stat.Display}"
                };
                foreach (string classes in infoTextTemplate.GetClasses())
                {
                    infoTextClone.AddToClassList(classes);
                }
                cardRoot.Add(infoTextClone);
                
            }
            infoTextTemplate.RemoveFromHierarchy();
            infoScrollContainer.Add(infoCardClone);
        }
        if (ProgressionDataManager.Money < item.cost)
        {
            infoPurchaseButton.text = "You can't afford this!";
            infoPurchaseButton.SetEnabled(false);
        } else
        {
            infoPurchaseButton.text = "Purchase";
            infoPurchaseButton.SetEnabled(true);
        }

        CheckItemPurchased(item);
    }

    private void HandlePurchase(ShopItemData item)
    {
        if (CurrentPlayerData.Data != null)
        {
            if (CheckItemPurchased(item))
            {
                return;
            }
            var data = CurrentPlayerData.Data;
            if (ProgressionDataManager.Money < item.cost)
            {
                Debug.LogWarning("Player is too poor");
                return;
            }
            if (data.weaponsOwned.Find(m => m.weaponName == item.dataName) != null)
            {
                return;
            }
            ProgressionDataManager.Money -= item.cost;

            // work with other data models
            data.weaponsOwned.Add(new()
            {
               weaponName = item.dataName,
               weaponLevel = 1,
               attachments = new List<string>() 
            });

            CurrentPlayerData.Data = data;

            CurrentPlayerData.Save();
            shopMenuScript.HandlePrompt(false);
            
            CheckItemPurchased(item);
        }
    }

    private bool CheckItemPurchased(ShopItemData item)
    {
        var data = CurrentPlayerData.Data;
        if (item == null || data == null)
        {
            return false;
        }
        if (item is WeaponData weaponData)
        {
            if (data.weaponsOwned.Find(m => m.weaponName == item.dataName) != null)
            {
                infoPurchaseButton.SetEnabled(false);
                infoPurchaseButton.text = "Equip in inventory";
                return true;
            } else
            {
                infoPurchaseButton.SetEnabled(true);
                infoPurchaseButton.text = "Purchase";
                return false;
            }
        }
        return false;
    }

    private void OnPurchaseButtonClicked(ClickEvent evt)
    {
        // use this data to check if the name is on the player's data, if so, then return it
        
        if (currentItem != null)
        {
            HandlePurchase(currentItem);
        }
    }

    public void OnViewClick(ShopItemData item)
    {
        shopMenuScript.OpenInfo();
        //shopMenuScript.CloseInfo();
        ViewItemDetails(item);
    }

    public void MoneyDisplay()
    {
        moneyDisplay.text = $"$ {ProgressionDataManager.Money}";
    }

    void OnDisable()
    {
        purchaseButton?.UnregisterCallback<ClickEvent>(OnPurchaseButtonClicked);
        if (CheckPurchasedLambda != null)
        {
            ProgressionDataManager.OnProgressionDataChanged -= CheckPurchasedLambda;
        }
        ProgressionDataManager.OnProgressionDataChanged -= MoneyDisplay;
    }
}
