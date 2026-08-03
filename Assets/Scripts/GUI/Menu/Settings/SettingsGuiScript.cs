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
    private ScrollView[] settingsScrollView;
    private List<ISettingAttributes> settingElements = new List<ISettingAttributes>(); 
    private Dictionary<string, dynamic> settingElementsDictionary = new Dictionary<string, dynamic>(); // load in settings first, then load in the data to the settings elements
    private bool waitingTransition;
    private bool isOpen = false;
    private PlayerSettings settingData;
    private PlayerData playerData;
    void OnEnable()
    {
        settingsContainer = uIDocument.rootVisualElement.Q<VisualElement>("main-container");
        closeButton = settingsContainer.Q<Button>("close-button");
        settingsScrollView = settingsContainer.Query<ScrollView>(className: "settings-container").ToList().ToArray();

        GetSettingsElement();

        settingsContainer?.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        closeButton.clicked += SettingsTransition;
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

    private void GetSettingsElement()
    {
        foreach (ScrollView scrollView in settingsScrollView)
        {
            foreach (var element in scrollView.contentContainer.Children())
            {
                if (element is ISettingAttributes && element.ClassListContains("setting-item"))
                {
                    settingElements.Add(element as ISettingAttributes);
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

    void OnDisable()
    {
        closeButton.clicked -= SettingsTransition;
        settingsContainer?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
    }
}
