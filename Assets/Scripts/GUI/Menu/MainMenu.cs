using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

using UnityEditor;
public enum MenuState
    {
        MainMenu,
        Settings,
        Credits,
        Shop,
        Playing
    }
public class GameManager : MonoBehaviour
{
    [Header("Player Scripts")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private MouseLook cameraLook; // your FPS camera script

    [Header("Other Menu Scripts")]
    [SerializeField] private ShopMenuScript shopMenuScript;

    private UnityEngine.UIElements.Button playButton;
    private UnityEngine.UIElements.Button shopButton;
    private Button quitButton;
    private UnityEngine.UIElements.UIDocument uiDocument;
    private VisualElement btnContainer;
    private Label title;

    private Camera mainCamera;
    [Header("GUI Documents and Cameras")]
    [SerializeField] private Camera[] CameraArray;
    [SerializeField] private UIDocument[] documentArray;
    [SerializeField] private UIDocument[] hudArray;
    [SerializeField] private Camera shopCamera;
    private bool isMenuOpen = true;
    private bool waitingToClose;


    
    public MenuState playerState;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        btnContainer = root.Q<VisualElement>(className: "container");
        title = root.Q<Label>(className: "title");
        mainCamera = Camera.main;
        playButton = btnContainer.Q<Button>("PlayButton");
        shopButton = btnContainer.Q<Button>("ShopButton");
        quitButton = btnContainer.Q<Button>("Quit");
        
        if (playButton != null)
        {
            playButton.clicked += () => OpenMenuItems(MenuState.Playing);
        }
        if (shopButton != null)
        {
            shopButton.clicked += () => OpenMenuItems(MenuState.Shop);
        }

        if (quitButton != null)
        {
            quitButton.clicked += QuitGame;
        }

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
        if (playButton != null)
        {
            playButton.clicked -= () => OpenMenuItems(MenuState.Playing);
        }
        if (shopButton != null)
        {
            shopButton.clicked -= () => OpenMenuItems(MenuState.Shop);
        }

        if (quitButton != null)
        {
            quitButton.clicked -= QuitGame;
        }
    }

    
    public void CloseMenu()
    {
        isMenuOpen = false;
        uiDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void OpenMenu()
    {
        isMenuOpen = true;
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
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
                shopMenuScript.OpenShop();
                break;
            case MenuState.Playing:
                OpenItemArray(hudArray, CameraArray[0]);
                CloseMenu();
                break;

        }
        
        
    }

    public void OpenMenuItems(MenuState menuState)
    {
        switch (menuState)
        {
            case MenuState.MainMenu:
                isMenuOpen = true;
                PlayerMovementManager(false);
                playerState = MenuState.MainMenu;
                waitingToClose = true;
                btnContainer.style.translate = new Translate(0, 0, 0);
                title.style.translate = new Translate(0, 0, 0);
                
                break;
            case MenuState.Playing:
                
                isMenuOpen = false;
                PlayerMovementManager(true);
                playerState = MenuState.Playing;
                waitingToClose = true;
                btnContainer.style.translate = new Translate(Length.Percent(-100), 0, 0);
                title.style.translate = new Translate(Length.Percent(-100), 0, 0);
                
                
                break;
            case MenuState.Shop:
                isMenuOpen = true;
                PlayerMovementManager(false);
                
                playerState = MenuState.Shop;
                waitingToClose = true;
                btnContainer.style.translate = new Translate(Length.Percent(-100), 0, 0);
                title.style.translate = new Translate(Length.Percent(-100), 0, 0);
                
                break;
        }
    }
    
    //change these so that it has parameters
    public void SetCamera(Camera cameraToEnable)
    {
        foreach (Camera camera in  CameraArray)
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
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        } else
        {
            playerMovement.enabled = false;
            cameraLook.enabled = false;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }
    
    public void QuitGame()
    {
        if (Application.isEditor)
        {
            EditorApplication.isPlaying = false;
        } else
        {
            Application.Quit();
        }
    }
} 
