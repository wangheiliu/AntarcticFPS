using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.ScriptableObjects.Settings
{
    [CreateAssetMenu(fileName = "DropDownInfo", menuName = "Settings/DropDownInfo")]
    public class DropDownInfo : BasicSettingsObject
    {
        [SerializeField, DontCreateProperty] private List<string> options;
        [CreateProperty] public List<string> Options
        {
            get => options;
            set
            {
                if (options == value) return;
                options = value;
                Option = options[0];
                UpdateProp();
            }
        }
        [SerializeField, DontCreateProperty] private string _option;
        [CreateProperty] public string Option
        {
            get => _option;
            set
            {
                if (value == null)
                {
                    _option = Options[0] ?? "Error";
                    return;

                }
                if (value != _option)
                {
                    if (Option.Contains(value)) return;

                    _option = value;
                }
            }
        }
    }
}
