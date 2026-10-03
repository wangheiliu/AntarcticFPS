using System.Runtime.CompilerServices;
using Game.ScriptableObjects.Levels;
using BaseUIControls;
using UnityEngine;
using UnityEngine.UIElements;
using BasicUIControls;
using Game.ScriptableObjects.Shop;
using System.Collections.Generic;
using System.Reflection;
using System;

public class LevelsGUIScript : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private LevelDatabase levelDatabase;
    [Header("UI")]
    [SerializeField] private UIDocument document;

    private VisualElement levelSelectContainer;
    private VisualElement propContainer;
    private Label propTitle;
    private Button levelCloseBtn;
    private Button propCloseBtn;
    private Button playButton;

    private ScrollView levelSelectScrollView;
    private ScrollView propScrollView;

    private LevelsData currentLevelDisplay;
    public Dictionary<LevelInfoElement, LevelsData> levelLookup = new();
    public Dictionary<string, StatDisplay> statDisplays = new();
    private readonly PropertyInfo[] fields = typeof(LevelsData).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    void Awake()
    {
        
        foreach (var field in fields)
        {
            StatDisplay value = field.GetCustomAttribute<StatDisplay>();
            if (value != null)
            {
                statDisplays.TryAdd(field.Name, value);
            }
        }
    }
    void Start()
    {
        document.rootVisualElement.style.display = DisplayStyle.None;
        EnableElement(levelSelectContainer, false);
        levelSelectContainer.style.display = DisplayStyle.None;
        EnableElement(propContainer, false);
        propContainer.style.display = DisplayStyle.None;
    }

    void OnEnable()
    {
        var root = document.rootVisualElement;
        levelSelectContainer = root.Q<VisualElement>("selection-container");
        propContainer = root.Q<VisualElement>("properties");

        levelCloseBtn = levelSelectContainer.Q<Button>("close-button");
        propCloseBtn = propContainer.Q<Button>("close-button");
        playButton = propContainer.Q<Button>("play-button");

        levelSelectScrollView = levelSelectContainer.Q<ScrollView>();
        propScrollView = propContainer.Q<ScrollView>();
        propTitle = propContainer.Q<Label>("prop-title");

        propScrollView.dataSource = currentLevelDisplay;

        propContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        levelSelectContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        FillLevels();
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == propContainer)
        {
            propContainer.style.display = propContainer.ClassListContains("property-select--disabled") ? DisplayStyle.None : DisplayStyle.Flex;
        } else if (evt.target == levelSelectContainer)
        {
            levelSelectContainer.style.display = levelSelectContainer.ClassListContains("level-select--disabled") ? DisplayStyle.None : DisplayStyle.Flex;
            if (levelSelectContainer.ClassListContains("level-select--disabled"))
            {
                document.rootVisualElement.style.display = DisplayStyle.None;
            }
        }
    }

    void OnDisable()
    {
        levelLookup.Clear();
    }

    private void FillLevels()
    {
        if (levelDatabase == null)
        {
            return;
        }

        foreach ((LevelInfoElement key, LevelsData data) in levelLookup)
        {
            key.ViewButton?.UnregisterAllRemovableCallbacks();
        }

        levelLookup.Clear();
        levelSelectScrollView.Clear();
        foreach (LevelsData data in levelDatabase.levels)
        {
            LevelInfoElement levelCard = new()
            {
                LevelData = data,
                dataSource = data
            };
            levelSelectScrollView.Add(levelCard);
            levelLookup.TryAdd(levelCard, levelCard.GetLevelsData());
            levelCard.ViewButton.RegisterCallback<ClickEvent>(FillProperties);
        }
    }

    public void FillProperties(ClickEvent evt)
    {
        LevelInfoElement infoSelectedElement = null;
        if (evt.target is VisualElement element)
        {
            infoSelectedElement = element.GetFirstAncestorOfType<LevelInfoElement>();
        }

        if (infoSelectedElement == null)
        {
            return;
        }

        if (infoSelectedElement.GetLevelsData() == currentLevelDisplay)
        {
            return;
        }
        
        EnableElement(propContainer, true);
        
        propScrollView.Clear();
        currentLevelDisplay = infoSelectedElement.GetLevelsData();
        propContainer.dataSource = currentLevelDisplay;

        AddPropText();
    }

    private void AddPropText()
    {
        foreach (var field in fields)
        {
            var attribute = statDisplays[field.Name];
            if (attribute == null) return;
            StatValue statValue = new()
            {
                Name = attribute.DisplayName,
                Prefix = attribute.Prefix,
                Unit = attribute.Unit,
                Value = field.GetValue(currentLevelDisplay)
            };

            Label label = new()
            {
                text = string.Format("<b>{0}</b>: {1}", attribute.DisplayName, statValue.Display)
            };
            propScrollView.Add(label);
        }
    }

    private void OnCloseButtonClick(ClickEvent evt)
    {
        if (evt.target == propCloseBtn)
        {
            EnableElement(propContainer, false);
        } else if (evt.target == levelCloseBtn)
        {
            EnableElement(levelSelectContainer, false);
        }
    }

    private void EnableElement(VisualElement element, bool isOpen)
    {
        if (element == propContainer)
        {
            element.EnableInClassList("property-select--enabled", isOpen);
            element.EnableInClassList("property-select--disabled", !isOpen);
        } else if (element == levelSelectContainer)
        {
            element.EnableInClassList("level-select--enabled", isOpen);
            element.EnableInClassList("level-select--disabled", !isOpen);
        }
    }

    public void Open()
    {
        document.rootVisualElement.style.display = DisplayStyle.Flex;
        EnableElement(levelSelectContainer, true);
    }
}
