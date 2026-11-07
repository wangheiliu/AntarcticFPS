using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Game.ScriptableObjects.Levels;
using System;
using Unity.Properties;

namespace BasicUIControls
{
    [UxmlElement]
    public partial class LevelInfoElement : VisualElement
    {
        private VisualElement container;
        private VisualElement main;
        private Label lockedLevelElement;
        private Image levelImage;
        private Label title;
        private Button _viewButton;
        public Button ViewButton => _viewButton;
        private LevelsData _levelsData;
        [UxmlAttribute]
        public LevelsData LevelData
        {
            get => _levelsData;
            set
            {
                if (_levelsData != value)
                {
                    _levelsData = value;
                }
            }
        }

        public LevelInfoElement()
        {
            lockedLevelElement = new()
            {
                name = "level-info-locked",
                text = "🔒"
            };
            lockedLevelElement.AddToClassList("level-info-locked");
            
            main = new()
            {
                name = "level-info-main"
            };
            main.AddToClassList("level-info-main");
            Add(main);

            container = new()
            {
                name = "level-info-container"
            };
            container.AddToClassList("level-info-container");
            main.Add(lockedLevelElement);
            main.Add(container);

            levelImage = new()
            {
                name = "level-info-image"
            };
            levelImage.AddToClassList("level-info-image");
            levelImage.SetBinding(nameof(Image.sprite), new DataBinding
            {
                dataSourceType = typeof(LevelsData),
                dataSourcePath = new PropertyPath(nameof(LevelsData.LevelImage)),
                bindingMode = BindingMode.ToTarget
            });

            title = new()
            {
                text = "Level Title Here",
                name = "level-info-title"
            };
            title.AddToClassList("level-info-title");
            title.SetBinding(nameof(Label.text), new DataBinding
            {
                dataSourceType = typeof(LevelsData),
                dataSourcePath = new PropertyPath(nameof(LevelsData.DisplayName)),
                bindingMode = BindingMode.ToTarget
            });
            container.Add(title);
            container.Add(levelImage);

            _viewButton = new()
            {
                text = "View",
                name = "level-info-view-btn"
            };
            _viewButton.AddToClassList("level-info-button");
            _viewButton.AddToClassList("base-style-button");
            container.Add(_viewButton);
            SetLocked(true);
        }

        public LevelsData GetLevelsData()
        {
            return LevelData;
        }

        public void SetLocked(bool isLocked)
        {
            if (lockedLevelElement == null) return;
            if (isLocked)
            {
                container?.EnableInClassList("level-locked", true);
                ViewButton.SetEnabled(false);
                lockedLevelElement.style.display = DisplayStyle.Flex;
            } else
            {
                container?.EnableInClassList("level-locked", false);
                ViewButton.SetEnabled(true);
                lockedLevelElement.style.display = DisplayStyle.None;
            }
        }
    }
}
