using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine.Rendering;

public class NametagScript : MonoBehaviour
{
    public string _username = "GuestPenguin";
    public string Username
    {
        get
        {
            return _username;
        }
        set
        {
            if (!Regex.IsMatch(value, pattern))
            {
                return;
            }
            _username = value;
            nameElement.text = _username;
        }
    }
    [SerializeField] private PanelRenderer nameUI;
    private TextElement nameElement;
    [SerializeField] private PanelRenderer UIInputDocument;

    private VisualElement nameVisualRoot;
    private VisualElement nameInputRoot;
    private Button submitButton;
    private TextField usernameInputField;
    private Label errorMessageText;
    private bool isValidUsername;
    private readonly string pattern = @"^[a-zA-Z0-9]{3,15}$";

    void OnEnable()
    {
        // set the username and last username to be the player's preferred username if possible
        nameUI.RegisterUIReloadCallback(OnUIRefresh);
        UIInputDocument.RegisterUIReloadCallback(OnUIRefresh);
    }

    void OnUIRefresh(PanelRenderer renderer, VisualElement _root, int currentVersion)
    {
        nameInputRoot = _root;
        nameVisualRoot = _root;

        nameInputRoot.style.display = DisplayStyle.None;
        nameElement = nameVisualRoot.Q<TextElement>("username-text");
        submitButton = nameInputRoot.Q<Button>("submit-button");
        usernameInputField = nameInputRoot.Q<TextField>("username-input");
        errorMessageText = nameInputRoot.Q<Label>("error-message");
        submitButton?.RegisterCallback<ClickEvent>(OnSubmit);
        usernameInputField?.RegisterValueChangedCallback(CheckUsername);

        GeneralPlayerDataManager.OnUsernameChanged += SetUsernameDisplay;

        if (CurrentPlayerData.Data != null)
        {
            if (!CurrentPlayerData.Data.hasSetUserName)
            {
                if (nameInputRoot == null) return;
                if (nameElement == null) return;
                nameInputRoot.style.display = DisplayStyle.Flex;
            }
        } else
        {
            Debug.LogWarning("Player data not loaded in yet");
        }
    }

    void Start()
    {
        if (CurrentPlayerData.Data != null)
        {
            if (CurrentPlayerData.Data.hasSetUserName)
            {
                if (nameInputRoot != null)
                {
                    nameInputRoot.style.display = DisplayStyle.None;
                }
                
                if (nameElement != null)
                {
                    nameElement.text = CurrentPlayerData.Data.username;
                }
                
            }
        }
    }

    void OnValidate()
    {
        if (nameElement == null)
        {
            return;
        }

        if (!Regex.IsMatch(_username, pattern))
        {
            return;
        }
        nameElement.text = _username;
    }

    private void OnSubmit(ClickEvent evt)
    {
        if (!isValidUsername)
        {
            return;
        }
        Username = usernameInputField.text;

        if (GeneralPlayerDataManager.Username != null)
        {
            GeneralPlayerDataManager.Username = Username;
            CurrentPlayerData.Data.hasSetUserName = true;
            CurrentPlayerData.Save();
        }

        nameInputRoot.style.display = DisplayStyle.None;


    }

    private void CheckUsername(ChangeEvent<string> evt)
    {
        if (!Regex.IsMatch(evt.newValue, pattern))
        {
            isValidUsername = false;
            ChangeBorderColors(usernameInputField.Q<VisualElement>("unity-text-input"), Color.red);
            errorMessageText.style.color = Color.red;
            errorMessageText.text = "Username should be 3-25 characters long, should not contain any unique symbols, and no spaces";
        }
        else
        {
            isValidUsername = true;
            ChangeBorderColors(usernameInputField.Q<VisualElement>("unity-text-input"), Color.white);
            errorMessageText.style.color = Color.white;
            errorMessageText.text = "Welcome! Please enter your username to start playing!";
        }
    }

    #region UI Display
    private void SetUsernameDisplay()
    {
        nameElement.text = GeneralPlayerDataManager.Username;
    }

    private void ChangeBorderColors(VisualElement element, Color color)
    {
        element.style.borderBottomColor = color;
        element.style.borderTopColor = color;
        element.style.borderLeftColor = color;
        element.style.borderRightColor = color;
    }

    #endregion

    void OnDisable()
    {
        GeneralPlayerDataManager.OnUsernameChanged -= SetUsernameDisplay;
        submitButton.UnregisterCallback<ClickEvent>(OnSubmit);
        usernameInputField.UnregisterValueChangedCallback(CheckUsername);

        nameUI.RegisterUIReloadCallback(OnUIRefresh);
        UIInputDocument.RegisterUIReloadCallback(OnUIRefresh);
    }
}
