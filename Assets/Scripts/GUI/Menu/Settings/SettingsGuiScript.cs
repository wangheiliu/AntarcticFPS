using Player.PlayerData;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using BaseUIControls;
using System.Collections.Generic;
using System.Reflection;
using BasicUIControls;

public class SettingsScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;
    [Header("UI Document")]
    [SerializeField] private UIDocument uIDocument;
    private VisualElement settingsContainer;
    private Button closeButton;
    private Button saveButton;
    private Button resetButton;
    private Button restoreDefaultsButton;
    private ScrollView[] settingsScrollView;
    private List<ISettingAttributes> settingElements = new List<ISettingAttributes>(); 
    private Dictionary<string, object> settingElementsDictionary = new Dictionary<string, object>(); // load in settings first, then load in the data to the settings elements
    private bool waitingTransition;
    private bool isOpen = false;
    private PlayerSettings settingData;
    private PlayerData playerData;
    void OnEnable()
    {
        settingsContainer = uIDocument.rootVisualElement.Q<VisualElement>("main-container");
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
    }

    void Start()
    {
        playerData = SaveData.Load();
        if (playerData == null)
        {
            Debug.LogWarning("No save data found. Creating default settings data.");
            SaveDefaultSettingsData();
            playerData = SaveData.Load();
            settingData = playerData.settings;
        }
        else
        {
            settingData = playerData.settings;
        }
        LoadSettings();
    }

    // data display
    private void SaveDefaultSettingsData()
    {
        playerData ??= new PlayerData();
        settingData = new PlayerSettings()
        {
            volume = 75,
            fov = 90,
            vSync = true,
            fullscreen = true,
            shadowsEnabled = true
        };

        playerData.settings = settingData;
        SaveData.Save(playerData);
    }
    

    // ui display
    void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == settingsContainer)
        {
            waitingTransition = false;
            if (isOpen == false)
            {
                ResetSettings();
                settingsContainer.style.display = DisplayStyle.None;
                gameManager.OpenMenuItems(MenuState.MainMenu);
            }
        }
    }

    public void SettingsTransition()
    {
        if (waitingTransition)
        {
            return;
        }
        waitingTransition = true;
        if (isOpen)
        {

            isOpen = false;
            settingsContainer.style.translate = new Translate(0, Length.Percent(120), 0);
        }
        else
        {
            settingsContainer.style.display = DisplayStyle.Flex;
            isOpen = true;
            settingsContainer.style.translate = new Translate(0, Length.Percent(0), 0);
        }
    }

    private void LoadSettingsElement()
    {
        foreach (ScrollView scrollView in settingsScrollView)
        {
            foreach (var element in scrollView.contentContainer.Children())
            {
                if (element is ISettingAttributes && element.ClassListContains("setting-item"))
                {
                    settingElements.Add(element as ISettingAttributes);
                    if (element is SlideToggle toggle)
                    {
                        element.RegisterCallback<ChangeEvent<bool>>(evt => OnToggleEvent(evt, (element as ISettingAttributes).DataName), CallbackOptions.Removable);
                    }
                    else if (element is CustomIntSlider slider)
                    {
                        element.RegisterCallback<ChangeEvent<int>>(evt => OnSliderEvent(evt, (element as ISettingAttributes).DataName), CallbackOptions.Removable);
                    }
                    
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

            if (settingElement is CustomIntSlider slider)
            {
                slider.Value = Convert.ToInt32(fieldValue);
                if (rangeAttribute != null)
                {
                    slider.LowestValue = (int)rangeAttribute.min;
                    slider.HighestValue = (int)rangeAttribute.max;
                } else
                {
                    slider.LowestValue = 0;
                    slider.HighestValue = 100;
                }
            } else if (settingElement is SlideToggle toggle)
            {
                toggle.value = Convert.ToBoolean(fieldValue);
            }
            else
            {
                Debug.LogWarning($"Unsupported setting element type: {settingElement.GetType().Name}");
            }
        }
    }

    private void ValueChanged(string dataName, object newValue)
    {
        FieldInfo field = typeof(PlayerSettings).GetField(dataName, BindingFlags.Public | BindingFlags.Instance);
        newValue = Convert.ChangeType(newValue, field.FieldType);
        if (field != null)
        {
            field.SetValue(settingData, newValue);
            settingElementsDictionary[dataName] = newValue;
        }
        else
        {
            Debug.LogWarning($"Field '{dataName}' not found in PlayerSettings.");
        }
    }

    private void SaveSettings()
    {
        playerData.settings = settingData;
        SaveData.Save(playerData);
    }

    private void ResetToDefault()
    {
        SaveDefaultSettingsData();
        playerData = SaveData.Load();
        settingData = playerData.settings;
        LoadSettings();
    }

    private void ResetSettings()
    {
        playerData = SaveData.Load();
        settingData = playerData.settings;
        LoadSettings();
    }

    private void OnToggleEvent(ChangeEvent<bool> evt, string dataName)
    {
        ValueChanged(dataName, evt.newValue);
    }

    private void OnSliderEvent(ChangeEvent<int> evt, string dataName)
    {
        ValueChanged(dataName, evt.newValue);
    }

    void OnDisable()
    {
        closeButton.clicked -= SettingsTransition;
        saveButton.clicked -= SaveSettings;
        restoreDefaultsButton.clicked -= ResetToDefault;
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
    }
}
