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

    
    private bool waitingForTransition;
    private bool isOpen = false;
    private bool isInfoOpen;
    private TabView tabContainer;
    private VisualElement titleContainer;
    private VisualElement infoElement;
    private Button closeButton;
    private Button infoCloseButton;
    void Start()
    {
        var root = uIDocument.rootVisualElement;
        tabContainer = root.Q<TabView>("tab-container");
        titleContainer = root.Q<VisualElement>("title-container");
        infoElement = root.Q<VisualElement>("info-container");
        infoCloseButton = infoElement.Q<Button>("info-close-button");

        root.style.display = DisplayStyle.None;
        tabContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoElement.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        StartCoroutine(InitNextFrame());
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == infoElement)
        {
            waitingForTransition = false;
            if (isInfoOpen)
                infoElement.style.display = DisplayStyle.None;
            return;
        }
        if (evt.target != tabContainer)
        {
            waitingForTransition = false;
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

    private IEnumerator InitNextFrame()
    {
        yield return null;
        var root = uIDocument.rootVisualElement;
        closeButton = root.Q<Button>("CloseButton");

        Debug.Log(closeButton);
        if (closeButton != null)
        {
            closeButton.clicked += CloseShop;
        }

        if (infoCloseButton != null)
        {
            infoCloseButton.clicked += CloseInfo;
        }
    }

    public void CloseShop()
    {
        if (waitingForTransition)
        {
            //Debug.Log("waitingForTransition was true");
            return;
        }
        waitingForTransition = true;
        isOpen = false;
        tabContainer.style.translate = new Translate(Length.Percent(-110), 0, 0);
        titleContainer.style.translate = new Translate(Length.Percent(-110), 0, 0);
    }

    public void CloseInfo()
    {
        isInfoOpen = false;
        if (waitingForTransition)
        {
            return;
        }

        waitingForTransition = true;
        
        infoElement.style.translate = new Translate(Length.Percent(120),0,0);
    }

    public void OpenInfo()
    {
        if (isInfoOpen)
        {
            return;
        }
        isInfoOpen = true;
        if (waitingForTransition)
            return;
        infoElement.style.display = DisplayStyle.Flex;
        waitingForTransition = true;
        infoElement.style.translate = new Translate(Length.Percent(0),0,0);
        
    }

    public void OpenShop()
    {
        uIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        if (waitingForTransition)
        {
            //Debug.Log("waitingForTransition was true");
            return;
        }
        waitingForTransition = true;
        isOpen = true;
        tabContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
        titleContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
        
        
    }
    

    private void OnDestroy()
    {
        if (tabContainer != null)
            tabContainer.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);

        if (infoElement != null)
            infoElement.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);  
    }
}
