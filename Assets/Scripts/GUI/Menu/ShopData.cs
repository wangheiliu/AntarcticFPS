using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using Unity.VisualScripting;
using System.Linq;

public enum Catagories
{
    Automatics,
    SMGs,
    Shotguns,
    Pistols,
    Knives
    //Medkit
}
public class ShopData : MonoBehaviour
{
    //gui references
    [SerializeField] private VisualTreeAsset templateTree;
    [SerializeField] private UIDocument shopDocument;
    [SerializeField] private ShopDatabase database;
    private VisualElement root;
    private Dictionary<string, Foldout> foldouts = new();

    
    void OnEnable()
    {
        root = shopDocument.rootVisualElement;
        Debug.Log(foldouts);
        PopulateUI(database);
        foreach (ShopItemData item in database.itemList)
        {
            Debug.Log(item.name);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void PopulateUI(ShopDatabase database)
    {
        //adds the foldouts first
        string[] catagories = Enum.GetNames(typeof(Catagories)).ToArray(); //select is basically .map() from JavaScript, nice
        foreach (string str in catagories)
        {
            Foldout foldout = root.Q<Foldout>(str);
            if (foldout != null)
            {
                foldouts.TryAdd(str, foldout);
                Debug.Log(foldouts[str].name);
            }
        }

        foreach (ShopItemData item in database.itemList)
        {
            string itemType = item.type.ToString();

            if (itemType == null) return;

            var foldoutContainer = root.Q(itemType.ToLower());
            Debug.Log(foldoutContainer);
            TemplateContainer templateUI = templateTree.Instantiate();
            foldoutContainer.Add(templateUI);

        }
    }
}
