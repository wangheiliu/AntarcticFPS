using System.Runtime.CompilerServices;
using Game.ScriptableObjects.Levels;
using UnityEngine;
using UnityEngine.UIElements;
using BasicUIControls;
using Game.ScriptableObjects.Shop;
using System.Collections.Generic;
using System.Reflection;
using System;
using Unity.Mathematics;
using Unity.VisualScripting;

public class LevelsGUIScript : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject playerObj;
    [SerializeField] private GameObject levelList;

    [Header("Database")]
    [SerializeField] private LevelDatabase levelDatabase;

    [Header("UI")]
    [SerializeField] private PanelRenderer document;
    [SerializeField] private GameManager mainMenuScript;

    private bool canOpenMenu = false;
    private bool isClickingOnPlayBtn = false;

    private VisualElement root;
    private VisualElement levelSelectContainer;
    private VisualElement propContainer;
    private Label propTitle;
    private Button levelCloseBtn;
    private Button propCloseBtn;
    private Button playButton;

    private ScrollView levelSelectScrollView;
    private ScrollView propScrollView;

    private LevelsData currentLevelDisplay;
    [NonSerialized] public Dictionary<LevelInfoElement, LevelsData> levelLookup = new();
    [NonSerialized] public Dictionary<string, StatDisplay> statDisplays = new();
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
        
    }

    void OnEnable()
    {
        document.RegisterUIReloadCallback(OnGUIReload);
    }

    private void OnGUIReload(PanelRenderer renderer, VisualElement _root, int currentVersion)
    {
        root = _root;
        levelSelectContainer = root.Q<VisualElement>("selection-container");
        propContainer = root.Q<VisualElement>("properties");

        levelCloseBtn = levelSelectContainer.Q<Button>("close-button");
        propCloseBtn = propContainer.Q<Button>("close-button");
        playButton = propContainer.Q<Button>("play-button");

        levelSelectScrollView = levelSelectContainer.Q<ScrollView>();
        propScrollView = propContainer.Q<ScrollView>();
        propTitle = propContainer.Q<Label>("prop-title");

        propScrollView.dataSource = currentLevelDisplay;

        #region Register Callbacks
        propContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        levelSelectContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        levelCloseBtn.RegisterCallback<ClickEvent>(evt =>
        {
            OnCloseButtonClick(evt);
            canOpenMenu = true;
        }, CallbackOptions.Removable);
        propCloseBtn.RegisterCallback<ClickEvent>(OnCloseButtonClick);
        playButton.RegisterCallback<ClickEvent>(evt => SpawnPlayer(currentLevelDisplay.mapName), CallbackOptions.Removable);
        #endregion
        mainMenuScript.OnMenuStateChanged += Open;
        FillLevels();
        root.style.display = DisplayStyle.None;
        EnableElement(levelSelectContainer, false);
        levelSelectContainer.style.display = DisplayStyle.None;
        EnableElement(propContainer, false);
        propContainer.style.display = DisplayStyle.None;
    }

    void OnDisable()
    {
        propContainer?.UnregisterAllRemovableCallbacks();
        levelSelectContainer?.UnregisterAllRemovableCallbacks();
        levelCloseBtn?.UnregisterAllRemovableCallbacks();
        propCloseBtn?.UnregisterAllRemovableCallbacks();
        playButton?.UnregisterAllRemovableCallbacks();

        mainMenuScript.OnMenuStateChanged -= Open;
        levelLookup.Clear();
        document.UnregisterUIReloadCallback(OnGUIReload);
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == propContainer)
        {
            propContainer.style.display = propContainer.ClassListContains("property-select--disabled") ? DisplayStyle.None : DisplayStyle.Flex;
        }
        else if (evt.target == levelSelectContainer)
        {
            levelSelectContainer.style.display = levelSelectContainer.ClassListContains("level-select--disabled") ? DisplayStyle.None : DisplayStyle.Flex;
            if (levelSelectContainer.ClassListContains("level-select--disabled"))
            {
                root.style.display = DisplayStyle.None;
            }
        }

        if (canOpenMenu)
        {
            canOpenMenu = false;
            mainMenuScript.OpenMenuItems(MenuState.MainMenu);
        }

        if (isClickingOnPlayBtn)
        {
            isClickingOnPlayBtn = false;
            mainMenuScript.OpenMenuItems(MenuState.Playing);
        }
    }

    #region UI Functions

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

        currentLevelDisplay = infoSelectedElement.GetLevelsData();
        propContainer.style.display = DisplayStyle.Flex;

        EnableElement(propContainer, true);

        propScrollView.Clear();
        propContainer.dataSource = currentLevelDisplay;
        currentLevelDisplay = infoSelectedElement.GetLevelsData();


        AddPropText();
    }

    private void AddPropText()
    {
        foreach (var field in fields)
        {
            var attribute = statDisplays.TryGetValue(field.Name, out StatDisplay statDisplay) ? statDisplay : null;
            if (attribute == null) continue;
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
        }
        else if (evt.target == levelCloseBtn)
        {
            EnableElement(levelSelectContainer, false);
            EnableElement(propContainer, false);
        }
        currentLevelDisplay = null;
    }

    private void EnableElement(VisualElement element, bool isOpen)
    {
        if (element == propContainer)
        {
            element.EnableInClassList("property-select--enabled", isOpen);
            element.EnableInClassList("property-select--disabled", !isOpen);
        }
        else if (element == levelSelectContainer)
        {
            element.EnableInClassList("level-select--enabled", isOpen);
            element.EnableInClassList("level-select--disabled", !isOpen);
        }
    }

    public void Open(MenuState state)
    {
        if (state != MenuState.LevelSelection)
        {
            return;
        }
        root.style.display = DisplayStyle.Flex;
        levelSelectContainer.style.display = DisplayStyle.Flex;

        root.schedule.Execute(() =>
        {
            if (levelSelectContainer == null) return;
            EnableElement(levelSelectContainer, true);
        }).StartingIn(100);
    }

    #endregion

    #region Spawn Player

    public void SpawnPlayer(string mapName)
    {
        #if UNITY_EDITOR
                isClickingOnPlayBtn = true;
                EnableElement(levelSelectContainer, false);
                EnableElement(propContainer, false);
                currentLevelDisplay = null;
                return;
        #endif
        #pragma warning disable CS0162 // Unreachable code detected
                Transform levelTransform = levelList.transform.Find(mapName);
        #pragma warning restore CS0162 // Unreachable code detected
        if (levelTransform == null) return;
        GameObject levelModel = levelTransform.gameObject;
        Transform spawnTransform = levelModel.transform.Find("SpawnPoint");
        if (spawnTransform == null) return;
        GameObject spawn = spawnTransform.gameObject;
        if (spawn == null)
        {
            Debug.LogError($"Spawn point not found for map: {mapName}");
            return;
        }

        currentLevelDisplay = null;
        playerObj.transform.position = spawn.transform.position;
        mainMenuScript.playerState = MenuState.Playing;
    }

    #endregion
}
