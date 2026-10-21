using System.Collections.Generic;
using UnityEngine;

namespace Game.ScriptableObjects.Settings
{
    [CreateAssetMenu(menuName = "Settings/Database", fileName = "SettingsDatabase")]
    public class SettingsDatabase : ScriptableObject
    {
        public List<BasicSettingsObject> settingsObjects = new();
    }
}

