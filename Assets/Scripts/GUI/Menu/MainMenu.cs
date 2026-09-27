using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private List<MenuStateValues> menuStateValues;
    private Dictionary<MenuState, (UIDocument[] documents, Camera camera)> menuStateDictionary = new();
    private bool isMenuOpen = true;
    private bool waitingToClose;

    private Translate menuClosedTransition = new(Length.Percent(-100), 0, 0);
    private Translate menuOpenTransition = new(Length.Percent(0), 0, 0);
    [Header("Menu State")]
    public MenuState playerState;



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

        playButton?.RegisterCallback<ClickEvent>(evt => NewOpenItem(MenuState.Playing), CallbackOptions.Removable);
        shopButton?.RegisterCallback<ClickEvent>(evt => NewOpenItem(MenuState.Shop), CallbackOptions.Removable);
        settingsButton?.RegisterCallback<ClickEvent>(evt => NewOpenItem(MenuState.Settings), CallbackOptions.Removable);
        quitButton?.RegisterCallback<ClickEvent>(evt => QuitGame(), CallbackOptions.Removable);
        inventoryBtn?.RegisterCallback<ClickEvent>(evt => NewOpenItem(MenuState.Inventory), CallbackOptions.Removable);

        btnContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

        NewOpenItem(MenuState.MainMenu);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isMenuOpen && (playerState == MenuState.Playing))
            {
                NewOpenItem(MenuState.MainMenu);
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

        CloseAllItems();
        var documentsToOpen = menuStateDictionary[playerState].documents;
        waitingToClose = false;

        foreach (UIDocument document in documentsToOpen)
        {
            document.rootVisualElement.style.display = DisplayStyle.Flex;
        }

        switch (playerState)
        {
            case MenuState.Shop:
                shopMenuScript.OpenShop();
                break;
            case MenuState.Settings:
                settingsScript.SettingsTransition();
                break;
            case MenuState.Inventory:
                inventoryScript.OpenInventory();
                break;
        }
    }

    //change these so that it has parameters
    public void SetCamera(Camera cameraToEnable)
    {
        foreach ((UIDocument[] _, Camera camera) in menuStateDictionary.Values)
        {
            camera.enabled = false;
        }
        cameraToEnable.enabled = true;
    }

    public void NewOpenItem(MenuState state)
    {
        var item = menuStateDictionary[state];
        UIDocument[] documentsToOpen = item.documents;
        Camera camera = item.camera;

        if (documentsToOpen.Length == 0 || documentsToOpen == null)
        {
            return;
        }

        waitingToClose = true;
        playerState = state;


        if (state != MenuState.MainMenu)
        {
            MenuTranslate(menuClosedTransition);
        }
        else
        {
            CloseAllItems();
            isMenuOpen = true;
            MenuTranslate(menuOpenTransition);
            foreach (UIDocument document in documentsToOpen)
            {
                document.rootVisualElement.style.display = DisplayStyle.Flex;
            }
        }

        if (state == MenuState.Playing)
        {
            PlayerMovementManager(true);
            isMenuOpen = false;
        }
        else
        {
            PlayerMovementManager(false);
        }

        SetCamera(camera);
    }

    public void CloseAllItems()
    {
        foreach (var item in menuStateDictionary)
        {
            foreach (UIDocument document in item.Value.documents)
            {
                document.rootVisualElement.style.display = DisplayStyle.None;
            }
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
