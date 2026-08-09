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
    [SerializeField] private UIDocument nameUI;
    private TextElement nameElement;
    [SerializeField] private UIDocument UIInputDocument;
    private Button submitButton;
    private TextField usernameInputField;
    private TextElement errorMessageText;
    private bool isValidUsername;
    private readonly string pattern = @"^[a-zA-Z0-9]{3,15}$";

    void OnEnable()
    {
        // set the username and last username to be the player's preferred username if possible
        UIInputDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        nameElement = nameUI.rootVisualElement.Q<TextElement>("username-text");
        submitButton = UIInputDocument.rootVisualElement.Q<Button>("submit-button");
        usernameInputField = UIInputDocument.rootVisualElement.Q<TextField>("username-input");
        errorMessageText = UIInputDocument.rootVisualElement.Q<TextElement>("error-message");
        submitButton.RegisterCallback<ClickEvent>(OnSubmit);
        usernameInputField.RegisterValueChangedCallback(CheckUsername);

        GeneralPlayerDataManager.OnUsernameChanged += SetUsernameDisplay;
    }

    void Start()
    {
        if (CurrentPlayerData.Data != null)
        {
            if (CurrentPlayerData.Data.hasSetUserName)
            {
                UIInputDocument.rootVisualElement.style.display = DisplayStyle.None;
                nameElement.text = CurrentPlayerData.Data.username;
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

        UIInputDocument.rootVisualElement.style.display = DisplayStyle.None;


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

    void OnDisable()
    {
        GeneralPlayerDataManager.OnUsernameChanged -= SetUsernameDisplay;
        submitButton.UnregisterCallback<ClickEvent>(OnSubmit);
        usernameInputField.UnregisterValueChangedCallback(CheckUsername);
    }
}
