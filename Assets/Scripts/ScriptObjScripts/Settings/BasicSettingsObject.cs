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

    public abstract class BasicSettingsObject : ScriptableObject, INotifyBindablePropertyChanged
    {
        [Header("Basic Setting Info", order = 0)]
        
        [SerializeField, DontCreateProperty] private string title;
        [CreateProperty]
        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                UpdateProp();
            }
        }
        [SerializeField, DontCreateProperty] private string dataName;
        [CreateProperty] public string DataName
        {
            get => dataName;
            set
            {
                if (dataName == value) return;
                dataName = value;
                UpdateProp();
            }
        }
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        public SettingControlType controlType;
        public SettingLocation settingLocation = SettingLocation.General;
        
        protected void UpdateProp([CallerMemberName] string propName = "")
        {
            Debug.Log($"Notifying property changed: {propName}");
            propertyChanged?.Invoke(this, new(propName));
        }
    }
}

