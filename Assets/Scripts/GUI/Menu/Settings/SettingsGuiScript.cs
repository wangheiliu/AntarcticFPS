using Player.PlayerData;

using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Reflection;
using BasicUIControls;
using Game.ScriptableObjects.Settings;


public class SettingsScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;
    [Header("UI Document")]
    [SerializeField] private PanelRenderer document;
    [Header("Settings")]
    [SerializeField] private SettingsDatabase settingsDatabase;
    #region UI Element Variables
    private VisualElement root;
    private VisualElement settingsContainer;
    private Button closeButton;
    private Button saveButton;
    private Button resetButton;
    private Button restoreDefaultsButton;
    private ScrollView[] settingsScrollView;
    #endregion
    private List<ISettingAttributes> settingElements = new List<ISettingAttributes>();
    private Dictionary<string, object> settingElementsDictionary = new Dictionary<string, object>(); // load in settings first, then load in the data to the settings elements
    #region Settings to Type Mapping
    private Dictionary<Type, Type> settingsToType = new()
    {
        {typeof(CustomIntSlider), typeof(int)},
        {typeof(SlideToggle), typeof(bool)},
        {typeof(CustomDropDownOption), typeof(string)}
    };
    #endregion
    private bool isOpen = false;
    private PlayerSettings settingData;
    private PlayerData playerData;
    private PlayerSettings lastSavedSettings;

    void OnEnable()
    {
        document.RegisterUIReloadCallback(OnUIReload);
    }

    void OnUIReload(PanelRenderer renderer, VisualElement _root, int currentVersion)
    {
        root = _root;
        settingsContainer = root.Q<VisualElement>("main-container");
        closeButton = settingsContainer.Q<Button>("close-button");
        saveButton = settingsContainer.Q<Button>("save-button");
        resetButton = settingsContainer.Q<Button>("last-saved-button");
        restoreDefaultsButton = settingsContainer.Q<Button>("restore-defaults-button");
        settingsScrollView = settingsContainer.Query<ScrollView>(className: "settings-container").ToList().ToArray();

        LoadSettingsElement();

        settingsContainer?.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        closeButton.clicked += SettingsTransition;
        saveButton.clicked += SaveSettings;
        restoreDefaultsButton.clicked += ResetToDefault;
        resetButton.clicked += ResetSettings;

        gameManager.OnMenuStateChanged += OnSettingsOpen;
    }

    async void Start()
    {
        playerData = await CurrentPlayerData.LoadDataAsync();
        if (playerData == null)
        {
            Debug.LogWarning("No save data found. Creating default settings data.");
            SaveDefaultSettingsData();
            playerData = CurrentPlayerData.Data;
            settingData = playerData.settings;
        }
        else
        {
            settingData = playerData.settings;
        }

        lastSavedSettings = CloneSettings(settingData);
        LoadSettings();
    }

    // data display
    private async void SaveDefaultSettingsData()
    {
        settingData = new PlayerSettings()
        {
            volume = 75,
            fov = 90,
            vSync = true,
            fullscreen = true,
            shadowsEnabled = true
        };

        playerData.settings = settingData;
        lastSavedSettings = CloneSettings(settingData);
        await SaveData.Save(playerData);
    }

    private PlayerSettings CloneSettings(PlayerSettings source)
    {
        if (source == null)
        {
            return null;
        }

        PlayerSettings clone = new PlayerSettings();
        foreach (FieldInfo field in typeof(PlayerSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            field.SetValue(clone, field.GetValue(source));
        }

        return clone;
    }


    // ui display
    void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == settingsContainer)
        {
            if (isOpen == false)
            {
                ResetSettings();
                settingsContainer.style.display = DisplayStyle.None;
                gameManager.OnEventNotifierChanged(gameManager.IsPaused ? MenuState.Paused : MenuState.MainMenu);
            }
            else
            {
                LoadSettings();
            }
        }
    }

    public void SettingsTransition()
    {
        if (isOpen)
        {
            Debug.Log("Closing settings");
            isOpen = false;
            settingsContainer.style.translate = new Translate(0, Length.Percent(120), 0);
        }
        else
        {
            Debug.Log("Opening settings");
            settingsContainer.style.display = DisplayStyle.Flex;
            isOpen = true;
            settingsContainer.style.translate = new Translate(0, Length.Percent(0), 0);
        }
    }

    #region Loading GUI
    private void LoadSettingsElement()
    {
        foreach (ScrollView scrollView in settingsScrollView)
        {
            foreach (var element in scrollView.contentContainer.Children())
            {
                if (element is ISettingAttributes && element.ClassListContains("setting-item"))
                {
                    var elementSettingAttributes = element as ISettingAttributes;
                    settingElements.Add(elementSettingAttributes);
                    
                    RegisterValueChanged(element, elementSettingAttributes.DataName);
                }
            }
        }
    }

    private void LoadSettings()
    {
        FieldInfo[] fields = typeof(PlayerSettings).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var settingElement = settingElements.Find(e => e.DataName.ToLower() == field.Name.ToLower());
            string fieldName = field.Name;
            object fieldValue = field.GetValue(settingData);
            RangeAttribute rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
            settingElementsDictionary[fieldName] = fieldValue;
            if (settingElement == null)
            {
                continue;
            }

            (settingElement as VisualElement).dataSource = settingsDatabase.settingsObjects.Find(data => data.DataName == settingElement.DataName);

            if (settingElement is CustomIntSlider slider)
            {
                slider.Value = Convert.ToInt32(fieldValue);
            }
            else if (settingElement is SlideToggle toggle)
            {
                toggle.value = Convert.ToBoolean(fieldValue);
            }
            else
            {
                Debug.LogWarning($"Unsupported setting element type: {settingElement.GetType().Name}");
            }
        }
    }

    private void LoadSettingsElement<T>(T element, BasicSettingsObject settingsObject) where T : VisualElement
    {
        element.dataSource = settingsObject;
    }

    #endregion

    #region Data
    private void ValueChanged(string dataName, object newValue)
    {
        FieldInfo field = typeof(PlayerSettings).GetField(dataName, BindingFlags.Public | BindingFlags.Instance);
        if (field == null)
        {
            return;
        }

        object convertedValue = Convert.ChangeType(newValue, field.FieldType);
        field.SetValue(settingData, convertedValue);
        settingElementsDictionary[dataName] = convertedValue;
    }

    private async void SaveSettings()
    {
        lastSavedSettings = CloneSettings(settingData);
        playerData.settings = settingData;
        await CurrentPlayerData.Save();
    }

    private void ResetToDefault()
    {
        SaveDefaultSettingsData();
        playerData = CurrentPlayerData.Data;
        if (playerData != null)
        {
            settingData = playerData.settings;
            LoadSettings();
        }

    }

    private void ResetSettings()
    {
        if (lastSavedSettings != null)
        {
            settingData = CloneSettings(lastSavedSettings);
            LoadSettings();
            return;
        }

        playerData = CurrentPlayerData.Data;
        if (playerData != null)
        {
            settingData = playerData.settings;
            LoadSettings();
        }
    }

    #endregion

    #region Events
    #region Value Changed Events
    private void OnValueChangedEvt<T>(ChangeEvent<T> evt, string dataName)
    {
        ValueChanged(dataName, evt.newValue);
    }

    private void RegisterValueChanged<T>(T field, string dataName)
    {
        if (settingsToType.TryGetValue(field.GetType(), out Type value))
        {
            var method = GetType().GetMethod(nameof(RegisterCallbackGeneric), BindingFlags.Instance | BindingFlags.NonPublic);
            method?.MakeGenericMethod(value).Invoke(this, new object[] { field, dataName });
        }
        else
        {
            Debug.LogWarning("Invalid type");
        }
    }

    private void RegisterCallbackGeneric<TValue>(VisualElement element, string dataName)
    {
        element.RegisterCallback<ChangeEvent<TValue>>(evt => OnValueChangedEvt(evt, dataName), CallbackOptions.Removable);
    }
    #endregion

    private void OnSettingsOpen(MenuState state)
    {
        if (state == MenuState.Settings)
        {
            document.enabled = true;
            root.style.display = DisplayStyle.Flex;
            settingsContainer.style.display = DisplayStyle.Flex;
            root.schedule.Execute(() =>
            {
                SettingsTransition();
            }).StartingIn(100);
        }
    }

    #endregion

    void OnDisable()
    {
        closeButton.clicked -= SettingsTransition;
        saveButton.clicked -= SaveSettings;
        restoreDefaultsButton.clicked -= ResetToDefault;
        gameManager.OnMenuStateChanged -= OnSettingsOpen;
        settingsContainer?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);

        foreach (ScrollView scrollView in settingsScrollView)
        {
            foreach (var element in scrollView.contentContainer.Children())
            {
                if (element is ISettingAttributes && element.ClassListContains("setting-item"))
                {
                    element.UnregisterAllRemovableCallbacks();
                }
            }
        }

        document.UnregisterUIReloadCallback(OnUIReload);
    }
}
