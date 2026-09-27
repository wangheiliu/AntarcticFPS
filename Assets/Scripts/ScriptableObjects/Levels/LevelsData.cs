using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ScriptableObjects.Levels
{
    [CreateAssetMenu(menuName = "Levels/LevelsData", fileName = "LevelsData")]
    public class LevelsData : ScriptableObject
    {
        [Header("Display Information")]
        [SerializeField, DontCreateProperty] private string _displayName;
        [CreateProperty] [StatDisplay(displayName: "Level name")] public string DisplayName
        {
            get => _displayName;
            set => _displayName = value;   
        }
        [TextArea(1, 10)]
        [SerializeField, DontCreateProperty] private string _description;
        [CreateProperty] [StatDisplay(displayName: "Description")] public string Description
        {
            get => _description;
            set => _description = value;
        }
        [SerializeField, DontCreateProperty] private Sprite _levelImage;
        [CreateProperty] public Sprite LevelImage
        {
            get => _levelImage;
            set => _levelImage = value;
        }
        [Header("Metadata")]
        [DontCreateProperty] public string dataName;
        [DontCreateProperty] public string mapName;
        [DontCreateProperty] public int id;
        [DontCreateProperty] public bool isVisible = true;

    }
}
