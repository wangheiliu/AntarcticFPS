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
        [CreateProperty] [field: SerializeField] public string ValueTitle {get; set;}
        [CreateProperty] [field: SerializeField] public int LowerRange {get; set;} = 0;
        [CreateProperty] [field: SerializeField] public int HighRange {get; set;} = 100;
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

