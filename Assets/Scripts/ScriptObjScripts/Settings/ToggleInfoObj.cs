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
        [SerializeField] private string _unToggledText = "Off";
        [CreateProperty] public string UntoggledText {get => _unToggledText; set
            {
                if (_unToggledText == value) return;
                _unToggledText = value;
                UpdateProp();
            }
        }
        [SerializeField] private string _toggledText = "On";
        [CreateProperty] public string ToggledText {get => _toggledText; set
            {
                if (_toggledText == value) return;
                _toggledText = value;
                UpdateProp();
            }
        }
        
    }
}

