using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;

namespace Game.ScriptableObjects.Settings
{
    [CreateAssetMenu(fileName = "SliderInfo", menuName = "Settings/SliderInfo")]
    public class SliderInfoObj : BasicSettingsObject
    {
        [Header("Slider Information")]
        [SerializeField, DontCreateProperty] private string valueTitle;
        [CreateProperty] public string ValueTitle {get => valueTitle; set
            {
                if (valueTitle == value) return;
                valueTitle = value;
                UpdateProp();
            }
        }

        [SerializeField, DontCreateProperty] private int lowerRange = 0;
        [CreateProperty] public int LowerRange
        {
            get => lowerRange;
            set
            {
                if (lowerRange == value) return;
                lowerRange = value;
                UpdateProp();
            }   
        }
        [SerializeField, DontCreateProperty] private int highRange = 100;
        [CreateProperty] public int HighRange
        {
            get => highRange;
            set
            {
                if (highRange == value) return;
                highRange = value;
                UpdateProp();
            }
        }
        [SerializeField, DontCreateProperty] private int _value;
        [CreateProperty] public int Value
        {
            get => _value;
            set
            {
                if (_value == value) return;
                _value = Mathf.Clamp(value, LowerRange, HighRange);
            }
        }
    }
}

