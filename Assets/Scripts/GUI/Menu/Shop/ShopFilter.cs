using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Game.ScriptableObjects.Shop;
using Game.GUI.AttributeElements;

public class ShopFilter : MonoBehaviour
{
    [SerializeField] private ShopData shopData;
    [SerializeField] private ShopMenuScript shopMenuScript;
    [SerializeField] private PanelRenderer shopDocument;
    private VisualElement root;
    private TextField searchField;
    private EnumField enumField;
    private TabView shopTabView;
    private List<FilterAttributes> filterFields = new();
    private Foldout[] allFoldouts;

    private readonly Dictionary<string, Func<ShopItemData, object>> selectors = new()
    {
      {"cost", item => item.cost},
      {"name", item => item.title},
      {"type", item => item.type}
    }; // good lookup for larger shops

    void OnEnable()
    {
        shopDocument.RegisterUIReloadCallback(OnUIRefresh);
    }

    void OnUIRefresh(PanelRenderer renderer,VisualElement _root, int currentVersion)
    {
        var root = _root;
        searchField = root.Q<TextField>("search-field");
        enumField = root.Q<EnumField>("type-enum");
        shopTabView = root.Q<TabView>("tab-container");
        filterFields = root.Query<FilterAttributes>().ToList();
        allFoldouts = root.Query<Foldout>(className: "shop-foldout").ToList().ToArray();

        foreach (FilterAttributes fields in filterFields)
        {
            if (fields.ClassListContains("int-field"))
            {
                var intField = fields.Q<IntegerField>();
                intField?.RegisterValueChangedCallback(evt =>
                {
                    IntFieldFormChange(evt, fields.key);
                });
            } else if (fields.ClassListContains("str-field"))
            {
                var field = fields.Q<TextField>();
                field?.RegisterValueChangedCallback(evt =>
                {
                    TextFieldChange(evt, fields.key);
                });
            }
        }

        enumField?.RegisterValueChangedCallback(HandleEnumChange);
    }

    public void DisplayFoldouts(string foldoutId)
    {
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
        int integerValue = 0;
        if (int.TryParse(value.ToString(), out int res))
        {
            integerValue = res;
        }
        string strValue = value == null ? "" : value.ToString();
        
        if (!shopMenuScript.isFiltersOpen)
        {
            foreach (ShopData.ShopLookupTable shopLookupTable in shopData.lookUpTable)
            {
                shopLookupTable.element.style.display = DisplayStyle.Flex;
            }
            return;
        }

        foreach (ShopData.ShopLookupTable shopLookupTable in shopData.lookUpTable)
            {
                shopLookupTable.element.style.display = DisplayStyle.None;
            }



        if (selectors.TryGetValue(statName, out var selector))
        {
            foreach (ShopData.ShopLookupTable shopLookupTable in shopData.lookUpTable)
            {
                object selected = selector(shopLookupTable.item);
                if (selected is string strData && strData.Contains(strValue))
                {
                    shopLookupTable.element.style.display = DisplayStyle.Flex;
                    shopLookupTable.category.value = true;
                }
                
                if (selected is int intData && integerValue <= intData)
                {
                    shopLookupTable.element.style.display = DisplayStyle.Flex;
                    shopLookupTable.category.value = true;
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

        foreach (ShopData.ShopLookupTable shopLookupTable in shopData.lookUpTable)
        {
            shopLookupTable.element.style.display = DisplayStyle.Flex;
            // foldout.value = false;
        }
    }

    private void HandleEnumChange(ChangeEvent<Enum> evt)
    {
        string enumName = Enum.GetName(typeof(ShopItemCatagory), evt.newValue);
        DisplayFoldouts(enumName);
    }

    private void IntFieldFormChange(ChangeEvent<int> evt, string key)
    {
        FilterShopSlots(key,evt.newValue);
    }

    private void TextFieldChange(ChangeEvent<string> evt, string key)
    {
        FilterShopSlots(key, evt.newValue);
    }

    private void OnDisable()
    {
        enumField?.UnregisterValueChangedCallback(HandleEnumChange);
        shopDocument.UnregisterUIReloadCallback(OnUIRefresh);
    }
}
