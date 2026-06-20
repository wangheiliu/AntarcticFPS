using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[System.Serializable] public class ShopItem
{
    public string title;
    public string type;
    public Sprite icon;
    public string description;
}
public class ShopData : MonoBehaviour
{
    //gui references
    [SerializeField] private VisualTreeAsset templateTree;
    [SerializeField] private UIDocument shopDocument;
    private VisualElement root;
    private Foldout[] foldouts;

    List<ShopItem> items = new();

    void OnEnable()
    {
        root = shopDocument.rootVisualElement;
        foldouts = root.Query<Foldout>().ToList().ToArray();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
}
