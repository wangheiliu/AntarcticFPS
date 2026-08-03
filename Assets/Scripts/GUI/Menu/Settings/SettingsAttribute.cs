using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class SettingsAttribute : Attribute
{
    public string settingName;

    public SettingsAttribute(string settingName)
    {
        this.settingName = settingName;
    }
}
