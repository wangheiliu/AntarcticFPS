using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopFilter : MonoBehaviour
{
    [SerializeField] private ShopData shopData;
    [SerializeField] private ShopMenuScript shopMenuScript;
    [SerializeField] private UIDocument shopDocument;
    private TextField searchField;
    private EnumField enumField;
    private TabView shopTabView;
    private List<VisualElement> filterFields = new();
    private Foldout[] allFoldouts;

    private readonly Dictionary<string, Func<ShopItemData, object>> selectors = new()
    {
      {"cost", item => item.cost},
      {"name", item => item.title},
      {"type", item => item.type}
    }; // good lookup for larger shops

    void Start()
    {
        
    }

    void OnEnable()
    {
        var root = shopDocument.rootVisualElement;
        searchField = root.Q<TextField>("search-field");
        enumField = root.Q<EnumField>("type-enum");
        shopTabView = root.Q<TabView>("tab-container");
        filterFields = root.Query<VisualElement>(className: "field-price-container").ToList();
        allFoldouts = root.Query<Foldout>(className: "shop-foldout").ToList().ToArray();

        foreach (VisualElement fields in filterFields)
        {
            if (fields.ClassListContains("int-field"))
            {
                var intField = fields.Q<IntegerField>();
                intField?.RegisterValueChangedCallback(IntFieldFormChange);
            }
        }

        enumField?.RegisterValueChangedCallback(HandleEnumChange);
    }

    void Update()
    {
        
    }

    private void RefreshList()
    {
        
    }

    public void DisplayFoldouts(string foldoutId)
    {
        // add another Enum that is named "any" and then exit out of the function if needed
        Debug.Log(foldoutId);
        if (!shopMenuScript.isFiltersOpen)
        {
            foreach (Foldout f in allFoldouts)
            {
                f.style.display = DisplayStyle.Flex;
            }
            return; 
        }
        foreach (Foldout f in allFoldouts)
        {
            f.style.display = DisplayStyle.None;
        }
        
        if (foldoutId == "Any")
        {
            foreach (Foldout f in allFoldouts)
            {
                f.style.display = DisplayStyle.Flex;
            }
            return;
        }

        Foldout foldout = Array.Find(allFoldouts, f => f.name.Equals(foldoutId, StringComparison.OrdinalIgnoreCase));
        
        if (foldout != null)
        {
            Tab tabToOpen = foldout.GetFirstAncestorOfType<Tab>();
            foldout.style.display = DisplayStyle.Flex;
            if (tabToOpen == null) 
                return;
            shopTabView.activeTab = tabToOpen;
        }
    }

    private void FilterShopSlots(string statName, object value)
    {
        if (!shopMenuScript.isFiltersOpen)
        {
            foreach ((Foldout _, (ShopItemData _, VisualElement element)) in shopData.lookUpTable)
            {
                element.style.display = DisplayStyle.Flex;
                // foldout.value = false;
            }
            return;
        }

        foreach ((Foldout _, (ShopItemData _, VisualElement element)) in shopData.lookUpTable)
        {
            element.style.display = DisplayStyle.None;
        }



        if (selectors.TryGetValue(statName, out var selector))
        {
            foreach ((Foldout foldout, (ShopItemData itemData, VisualElement element)) in shopData.lookUpTable)
            {
                if (Equals(selector(itemData), value))
                {
                    element.style.display = DisplayStyle.Flex;
                    foldout.value = true;
                }
            }
        }

    }

    public void ResetFilters()
    {
        foreach (Foldout f in allFoldouts)
        {
            f.style.display = DisplayStyle.Flex;
        }

        foreach ((Foldout _, (ShopItemData _, VisualElement element)) in shopData.lookUpTable)
        {
            element.style.display = DisplayStyle.Flex;
            // foldout.value = false;
        }
    }

    private void HandleEnumChange(ChangeEvent<Enum> evt)
    {
        string enumName = Enum.GetName(typeof(Catagories), evt.newValue);
        DisplayFoldouts(enumName);
    }

    private void IntFieldFormChange(ChangeEvent<int> evt)
    {
        
    }

    private void OnDisable()
    {
        enumField?.UnregisterValueChangedCallback(HandleEnumChange);
    }
}
