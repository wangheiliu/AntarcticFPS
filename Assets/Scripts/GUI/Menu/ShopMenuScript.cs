using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Threading.Tasks;
using System.Collections;

public class ShopMenuScript : MonoBehaviour
{
    [Header("UI Documents")]
    [SerializeField] private UIDocument uIDocument;
    [SerializeField] private UIDocument menuDocument;
    [Header("Main Menu Camera")]
    [SerializeField] private Camera menuCamera;
    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;

    private Button closeButton;
    private bool waitingForTransition;
    private bool isOpen = false;
    private TabView tabContainer;
    private VisualElement titleContainer;
    void Start()
    {
        var root = uIDocument.rootVisualElement;
        tabContainer = root.Q<TabView>("tab-container");
        titleContainer = root.Q<VisualElement>("title-container");
        root.style.display = DisplayStyle.None;
        tabContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        StartCoroutine(InitNextFrame());
    }


    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        Debug.Log("Transition Started!");
        if (evt.target != tabContainer)
        {
            return;
        }

        if (!waitingForTransition)
        {
            return;
        }

        waitingForTransition = false;

        if (isOpen)
        {
            return;
        } else
        {
            gameManager.OpenMenuItems(GameManager.MenuState.MainMenu);
        }
    }

    public void CloseShop()
    {
        Debug.Log("Clicked!");
        if (waitingForTransition)
        {
            Debug.Log("waitingForTransition was true");
            return;
        }
        waitingForTransition = true;
        isOpen = false;
        tabContainer.style.translate = new Translate(Length.Percent(-110), 0, 0);
        titleContainer.style.translate = new Translate(Length.Percent(-110), 0, 0);
        
    }

    public void OpenShop()
    {
        uIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        if (waitingForTransition)
        {
            Debug.Log("waitingForTransition was true");
            return;
        }
        waitingForTransition = true;
        isOpen = true;
        tabContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
        titleContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
        
        
    }
    private IEnumerator InitNextFrame()
    {
        yield return null;
        var root = uIDocument.rootVisualElement;
        closeButton = root.Q<Button>("CloseButton");

        Debug.Log(closeButton);
        if (closeButton != null)
        {
            closeButton.clicked += CloseShop;
            Debug.Log("Button Hooked!");
        } 
    }

    private void OnDestroy()
    {
        if (tabContainer != null)
            tabContainer.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);        
    }
}
