using System.ComponentModel;
using System.IO;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.ScriptableObjects.Settings
{
    [CreateAssetMenu(fileName = "ToggleInfo", menuName = "Settings/Toggle")]
    public class ToggleInfoObj : BasicSettingsObject
    {
        [SerializeField, DontCreateProperty] private bool _value = false;
        [SerializeField] private string _unToggledText = "Off";
        [CreateProperty] public string UntoggledText {get => _unToggledText; set => _unToggledText = value;}
        [SerializeField] private string _toggledText = "On";
        [CreateProperty] public string ToggledText {get => _toggledText; set => _toggledText = value;}
        [DontCreateProperty] private string _valueDisplay;
        [CreateProperty] public string ValueDisplay
        {
            get => _valueDisplay;
            set
            {
                if (value == _valueDisplay) return;
                _valueDisplay = _value ? UntoggledText : ToggledText;
            }
        }
        [CreateProperty] public bool Value
        {
            get => _value;
            set => _value = value;
        }
    }
}

