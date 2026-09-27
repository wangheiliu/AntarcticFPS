using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

using UnityEditor;
using System;
using System.Collections.Generic;
public enum MenuState
{
    MainMenu,
    Settings,
    Credits,
    Shop,
    Playing,
    Inventory,
    LevelSelection,
    Paused
}
public class GameManager : MonoBehaviour
{
    [Serializable]
    public struct MenuStateValues
    {
        public MenuState key;
        public UIDocument[] documentVal;
        public Camera cameraVal;
    }
    [Header("Player Scripts")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook cameraLook; // your FPS camera script

    [Header("Other Menu Scripts")]
    [SerializeField] private ShopMenuScript shopMenuScript;
    [SerializeField] private SettingsScript settingsScript;
    [SerializeField] private InventoryScript inventoryScript;

    private Button playButton;
    private Button shopButton;
    private Button settingsButton;
    private Button quitButton;
    private Button inventoryBtn;
    private UIDocument uiDocument;
    private VisualElement btnContainer;
    private Label title;


    [Header("GUI Documents and Cameras")]
    [SerializeField] private Camera[] CameraArray;
    [SerializeField] private UIDocument[] documentArray;
    [SerializeField] private UIDocument[] hudArray;
    [SerializeField] private UIDocument profileDisplay;
    [SerializeField] private Camera shopCamera;
    private bool isMenuOpen = true;
    private bool waitingToClose;

    private Translate menuClosedTransition = new(Length.Percent(-100), 0, 0);
    private Translate menuOpenTransition = new(Length.Percent(0), 0, 0);
    public MenuState playerState;

    [SerializeField] private List<MenuStateValues> menuStateValues;
    private Dictionary<MenuState, (UIDocument[] documents, Camera camera)> menuStateDictionary = new();

    void Awake()
    {
        foreach (MenuStateValues values in menuStateValues)
        {
            menuStateDictionary.TryAdd(values.key, (values.documentVal, values.cameraVal));
        }
    }

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        btnContainer = root.Q<VisualElement>(className: "container");
        title = root.Q<Label>(className: "title");
        playButton = btnContainer.Q<Button>("PlayButton");
        shopButton = btnContainer.Q<Button>("ShopButton");
        inventoryBtn = btnContainer.Q<Button>("CustomizeButton");
        settingsButton = btnContainer.Q<Button>("SettingsButton");
        quitButton = btnContainer.Q<Button>("Quit");

        playButton?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.Playing), CallbackOptions.Removable);
        shopButton?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.Shop), CallbackOptions.Removable);
        settingsButton?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.Settings), CallbackOptions.Removable);
        quitButton?.RegisterCallback<ClickEvent>(evt => QuitGame(), CallbackOptions.Removable);
        inventoryBtn?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.Inventory), CallbackOptions.Removable);

        btnContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

        OpenMenuItems(MenuState.MainMenu);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isMenuOpen && (playerState == MenuState.Playing))
            {
                OpenMenuItems(MenuState.MainMenu);
            }
        }
    }

    void OnDisable()
    {
        playButton?.UnregisterAllRemovableCallbacks();
        shopButton?.UnregisterAllRemovableCallbacks();
        settingsButton?.UnregisterAllRemovableCallbacks();
        quitButton?.UnregisterAllRemovableCallbacks();
        inventoryBtn?.UnregisterAllRemovableCallbacks();
    }


    public void CloseMenu()
    {
        isMenuOpen = false;
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void OpenMenu()
    {
        isMenuOpen = true;
        EnableMouse(true);
        uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
    }
    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target != btnContainer)
        {
            return;
        }
        if (!waitingToClose)
        {
            return;
        }

        waitingToClose = false;
        switch (playerState)
        {
            case MenuState.MainMenu:
                OpenItem(documentArray[0], CameraArray[0]);
                OpenMenu();
                break;
            case MenuState.Shop:
                OpenItem(documentArray[1], CameraArray[1]);
                CloseMenu();
                EnableMouse(true);
                shopMenuScript.OpenShop();
                break;
            case MenuState.Settings:
                OpenItem(documentArray[2], CameraArray[0]);
                CloseMenu();
                EnableMouse(true);
                settingsScript.SettingsTransition();
                break;
            case MenuState.Inventory:
                OpenItem(documentArray[2], CameraArray[0]);
                CloseMenu();
                EnableMouse(true);
                inventoryScript.OpenInventory();
                break;
            case MenuState.Playing:
                OpenItemArray(hudArray, CameraArray[0]);
                EnableMouse(false);
                CloseMenu();
                break;

        }


    }

    // this method handles translation and player states while OpenItems() handle opening individual ui documents
    public void OpenMenuItems(MenuState menuState)
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                isMenuOpen = true;
                PlayerMovementManager(false);
                playerState = MenuState.MainMenu;
                waitingToClose = true;
                CloseAllItems();
                uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                MenuTranslate(menuOpenTransition);
                profileDisplay.rootVisualElement.style.display = DisplayStyle.Flex;
                break;
            case MenuState.Playing:

                isMenuOpen = false;
                PlayerMovementManager(true);
                playerState = MenuState.Playing;
                waitingToClose = true;
                MenuTranslate(menuClosedTransition);


                break;
            case MenuState.LevelSelection:
                isMenuOpen = false;
                playerState = MenuState.LevelSelection;
                waitingToClose = true;
                MenuTranslate(menuClosedTransition);
                break;
            case MenuState.Shop:
                isMenuOpen = true;
                PlayerMovementManager(false);

                playerState = MenuState.Shop;
                waitingToClose = true;
                MenuTranslate(menuClosedTransition);
                profileDisplay.rootVisualElement.style.display = DisplayStyle.None;
                break;
            case MenuState.Settings:
                isMenuOpen = true;
                PlayerMovementManager(false);

                playerState = MenuState.Settings;
                waitingToClose = true;
                MenuTranslate(menuClosedTransition);
                profileDisplay.rootVisualElement.style.display = DisplayStyle.Flex;
                break;
            case MenuState.Inventory:
                isMenuOpen = true;
                PlayerMovementManager(false);

                playerState = MenuState.Inventory;
                waitingToClose = true;
                MenuTranslate(menuClosedTransition);
                profileDisplay.rootVisualElement.style.display = DisplayStyle.None;
                break;
        }
    }

    //change these so that it has parameters
    public void SetCamera(Camera cameraToEnable)
    {
        foreach (Camera camera in CameraArray)
        {
            camera.enabled = false;
        }
        cameraToEnable.enabled = true;
    }

    public void SetUiDocument(UIDocument documentToOpen)
    {
        CloseAllItems();
        documentToOpen.rootVisualElement.style.display = DisplayStyle.Flex;
    }
    public void NewOpenItem(MenuState state)
    {
        var item = menuStateDictionary[state];
        UIDocument[] documentsToOpen = item.documents;
        Camera camera = item.camera;
        playerState = state;
        
        if (state != MenuState.MainMenu)
        {
            MenuTranslate(menuClosedTransition);
        } else
        {
            MenuTranslate(menuOpenTransition);
        }

        CloseAllItems();

        foreach (UIDocument document in documentsToOpen)
        {
            document.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        SetCamera(camera);
    }
    public void OpenItem(UIDocument document, Camera camera)
    {
        SetUiDocument(document);
        SetCamera(camera);
    }

    public void OpenItemArray(UIDocument[] documents, Camera camera)
    {
        CloseAllItems();

        foreach (UIDocument document in documents)
        {
            document.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        SetCamera(camera);
    }

    public void CloseAllItems()
    {
        foreach (UIDocument document in documentArray)
        {
            document.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    public void PlayerMovementManager(bool canMove)
    {
        if (canMove)
        {
            playerMovement.enabled = true;
            cameraLook.enabled = true;
            EnableMouse(false);
        }
        else
        {
            playerMovement.enabled = false;
            cameraLook.enabled = false;
            EnableMouse(true);
        }
    }

    private void EnableMouse(bool state)
    {
        UnityEngine.Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        UnityEngine.Cursor.visible = state;
    }

    public void MenuTranslate(Translate translate)
    {
        btnContainer.style.translate = translate;
        title.style.translate = translate;
    }

    public void QuitGame()
    {
        if (Application.isEditor)
        {
            EditorApplication.isPlaying = false;
        }
        else
        {
            Application.Quit();
        }
    }
}
