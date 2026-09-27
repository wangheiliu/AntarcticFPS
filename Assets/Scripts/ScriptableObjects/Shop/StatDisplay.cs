using System;
using Unity.VisualScripting;
using UnityEngine;
public class StatDisplay : Attribute
{
    public string DisplayName;
    public string Unit;
    public string Prefix;
    public string Group;
    public object Value;

    public StatDisplay(string displayName, string prefix = null, string unit = null, string group = "Placeholder")
    {
        DisplayName = displayName;
        Group = group;
        Unit = unit;
        Prefix = prefix;
    }
}

public class StatValue
{
    public string Name;
    public string Unit;
    public string Prefix;
    public object Value;

    public string Display
    {
        get
        {
            return $"{Prefix}{Value}{Unit}";
            
        }
    }
}