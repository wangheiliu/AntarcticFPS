using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.ScriptableObjects.Settings
{
    public enum SettingControlType
    {
        Slider,
        Toggle,
        OptionList
    }

    public enum SettingLocation
    {
        None,
        General,
        Graphics,
        Sound,
        Codes
    }
    public abstract class BasicSettingsObject : ScriptableObject
    {
        [Header("Basic Setting Info", order = 0)]
        [CreateProperty] [field: SerializeField]
        public string Title { get; set; }
        [CreateProperty] public string DataName {get; set;}
        public SettingControlType controlType;
        public SettingLocation settingLocation = SettingLocation.General;
        
    }
}

