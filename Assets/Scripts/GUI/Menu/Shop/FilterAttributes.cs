using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// this script creates the custom "keys" element for a lookup function in ShopFilter
[UxmlElement]
public partial class FilterAttributes : VisualElement
{
    [UxmlAttribute] public string key {get; set;}

    public void Display()
    {
        
    } // don't add to this unless if you want to intiialize something
}
