using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

[Serializable]
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
        public PanelRenderer[] documents;
        public Camera camera;
    }
    [Header("Player Scripts")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook cameraLook; // your FPS camera script

    [Header("Other Menu Scripts")]
    [SerializeField] private ShopMenuScript shopMenuScript;
    [SerializeField] private SettingsScript settingsScript;
    [SerializeField] private InventoryScript inventoryScript;

    #region GUI elements
    private VisualElement root;
    private Button playButton;
    private Button shopButton;
    private Button settingsButton;
    private Button quitButton;
    private Button inventoryBtn;
    private VisualElement btnContainer;
    private Label title;
    #endregion


    [Header("GUI Documents and Cameras")]
    [SerializeField] private PanelRenderer document;
    [SerializeField] private Dictionary<MenuState, MenuStateValues> menuStateDictionary = new();
    private bool isMenuOpen = true;
    private bool waitingToClose;
    private bool isPaused;
    public bool IsPaused { get => isPaused; }

    private Translate menuClosedTransition = new(Length.Percent(-100), 0, 0);
    private Translate menuOpenTransition = new(Length.Percent(0), 0, 0);
    [Header("Menu State")]
    public MenuState playerState;
    public event Action<MenuState> OnMenuStateChanged;
    public event Action<bool> OnPlayerPause;

    void OnEnable()
    {
        #region UI Elements
        document = GetComponent<PanelRenderer>();
        document.RegisterUIReloadCallback(OnUIReloaded);
        #endregion

        PlayerMovementManager(false);

        #region Register Button Callbacks
        
        #endregion
        
    }

    void OnUIReloaded(PanelRenderer renderer, VisualElement _root, int currentVer)
    {
        root = _root;

        btnContainer = root.Q<VisualElement>(className: "container");
        title = root.Q<Label>(className: "title");
        playButton = btnContainer.Q<Button>("PlayButton");
        shopButton = btnContainer.Q<Button>("ShopButton");
        inventoryBtn = btnContainer.Q<Button>("CustomizeButton");
        settingsButton = btnContainer.Q<Button>("SettingsButton");
        quitButton = btnContainer.Q<Button>("Quit");

        playButton?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.LevelSelection), CallbackOptions.Removable);
        shopButton?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.Shop), CallbackOptions.Removable);
        settingsButton?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.Settings), CallbackOptions.Removable);
        quitButton?.RegisterCallback<ClickEvent>(evt => QuitGame(), CallbackOptions.Removable);
        inventoryBtn?.RegisterCallback<ClickEvent>(evt => OpenMenuItems(MenuState.Inventory), CallbackOptions.Removable);
        

        btnContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

        OnMenuStateChanged += OnMenuOpen;
        OnMenuStateChanged += OnPlayerPlay;
        OnPlayerPause += OnPlayerPausedEvent;
        OpenMenuItems(MenuState.MainMenu);
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isMenuOpen && (playerState == MenuState.Playing))
            {
                Debug.Log("Escape key pressed. Opening pause menu.");
                OpenMenuItems(MenuState.Paused);
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

        OnMenuStateChanged -= OnMenuOpen;
        OnMenuStateChanged -= OnPlayerPlay;

        document.UnregisterUIReloadCallback(OnUIReloaded);
        menuStateDictionary.Clear();
    }


    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target != btnContainer)
        {
            return;
        }
        if (!waitingToClose)
        {
            Debug.LogWarning("Transition ended but not waiting to close. Ignoring.");
            return;
        }

        CloseAllItems();
        var documentsToOpen = menuStateDictionary[playerState].documents;
        waitingToClose = false;

        foreach (PanelRenderer document in documentsToOpen)
        {
            Debug.Log($"Enabling a document... {document.gameObject.name}");
            document.enabled = true;
        }

        OnMenuStateChanged?.Invoke(playerState);
    }


    #region Helper Functions
    public void OpenMenuItems(MenuState state)
    {
        var item = menuStateDictionary[state];
        PanelRenderer[] documentsToOpen = item.documents;
        Camera camera = item.camera;

        if (documentsToOpen.Length == 0 || documentsToOpen == null)
        {
            Debug.LogWarning($"No documents found for MenuState: {state}. Please check the menuStateValues in the inspector.");
            return;
        }

        playerState = state;
        waitingToClose = true;

        if (state != MenuState.MainMenu)
        {
            MenuTranslate(menuClosedTransition);

            if (state == MenuState.Paused)
            {
                CloseAllItems();

                foreach (PanelRenderer document in documentsToOpen)
                {
                    document.enabled = true;
                }
                OnMenuStateChanged?.Invoke(playerState);
            }
        }
        else
        {
            CloseAllItems();
            isMenuOpen = true;
            foreach (PanelRenderer document in documentsToOpen)
            {
                document.enabled = true;
            }
            root.style.display = DisplayStyle.Flex;
            root.schedule.Execute(() =>
            {
                if (!isMenuOpen) return;
                MenuTranslate(menuOpenTransition);
            }).StartingIn(100);
        }



        if (state == MenuState.Playing)
        {
            PlayerMovementManager(true);
            isMenuOpen = false;
            foreach (PanelRenderer document in documentsToOpen)
            {
                document.enabled = true;
            }
        }
        else
        {
            PlayerMovementManager(false);
        }

        SetCamera(camera);
    }
    public void SetCamera(Camera cameraToEnable)
    {
        foreach (MenuStateValues stateValue in menuStateDictionary.Values)
        {
            stateValue.camera.enabled = false;
        }
        cameraToEnable.enabled = true;
    }

    public void CloseAllItems()
    {
        foreach (var item in menuStateDictionary)
        {
            foreach (PanelRenderer document in item.Value.documents)
            {
                document.enabled = false;
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

    private void OnMenuOpen(MenuState state)
    {
        if (state == MenuState.MainMenu)
        {
            document.enabled = true;
            OpenMenuItems(MenuState.MainMenu);
        }
    }
    #endregion

    #region Event Handlers
    private void OnPlayerPlay(MenuState state)
    {
        if (state == MenuState.Playing)
        {
            OpenMenuItems(MenuState.Playing);
        }
    }

    public void OnEventNotifierChanged(MenuState state)
    {
        OnMenuStateChanged?.Invoke(state);
    }

    public void OnPauseEventNotifier(bool state)
    {
        OnPlayerPause?.Invoke(state);
    }

    private void OnPlayerPausedEvent(bool state)
    {
        isPaused = state;
    }
    #endregion

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
