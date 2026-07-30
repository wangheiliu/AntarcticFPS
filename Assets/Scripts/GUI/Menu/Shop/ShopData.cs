using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

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
    private Dictionary<string, Foldout> foldouts = new();
    public List<(Foldout, ShopItemData, VisualElement)> lookUpTable = new();
    private Action viewButtonLambda;
    

    
    void OnEnable()
    {
        root = shopDocument.rootVisualElement;
        infoScrollContainer = root.Q<ScrollView>("info-scroller");
        
        PopulateUI(database);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        
    }
    public void OnViewClick(ShopItemData item)
    {
        shopMenuScript.OpenInfo();
        //shopMenuScript.CloseInfo();
        ViewItemDetails(item);
    }
}
