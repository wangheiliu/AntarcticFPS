using UnityEngine;
using System.Collections.Generic;

namespace Game.ScriptableObjects.Levels
{
    [CreateAssetMenu(menuName = "Levels/Database", fileName = "LevelDatabase")]
    public class LevelDatabase : ScriptableObject
    {
        public List<LevelsData> levels;
    }
}

