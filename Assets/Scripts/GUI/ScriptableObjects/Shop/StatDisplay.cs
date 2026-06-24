using System;
using Unity.VisualScripting;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field)]
public class StatDisplay : Attribute
{
    public string DisplayName;
    public string Unit;
    public string Group;
    public object Value;

    public StatDisplay(string displayName, string unit = null, string group = "Placeholder")
    {
        DisplayName = displayName;
        Group = group;
        Unit = unit;
    }
}

public class StatValue
{
    public string Name;
    public string Unit;
    public object Value;

    public string Display
    {
        get
        {
            if (Unit == "$")
            {
                return $"{Unit}{Value}";
            } else
            {
                return $"{Value}{Unit}";
            }
            
        }
    }
}