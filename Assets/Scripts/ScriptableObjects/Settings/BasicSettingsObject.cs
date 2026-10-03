using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.ScriptableObjects.Settings
{
    public abstract class BasicSettingsObject : ScriptableObject
    {
        [CreateProperty] [field: SerializeField]
        public string Title { get; set; }
    }
}

