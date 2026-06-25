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

    
    private bool waitingShopTransition;
    private bool waitingInfoTransition;
    private bool isOpen = false;
    private bool isInfoOpen;
    private TabView tabContainer;
    private VisualElement titleContainer;
    private VisualElement infoElement;
    private Button closeButton;
    private Button infoCloseButton;
    private VisualElement shopContainer;
    void Start()
    {
        var root = uIDocument.rootVisualElement;
        infoElement = root.Q<VisualElement>("info-container");
        infoCloseButton = infoElement.Q<Button>("info-close-button");
        shopContainer = root.Q<VisualElement>("shop-container");

        root.style.display = DisplayStyle.None;
        shopContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoCloseButton.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        StartCoroutine(InitNextFrame());
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == infoCloseButton)
        {
            if (!waitingInfoTransition)
            {
                return;
            }
            waitingInfoTransition = false;
            if (!isInfoOpen)
            {
                infoElement.style.display = DisplayStyle.None;
            }
        } else if (evt.target == shopContainer)
        {
            waitingShopTransition = false;
        }

        if (!isOpen && !waitingShopTransition)
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
        if (waitingShopTransition)
        {
            //Debug.Log("waitingShopTransition was true");
            return;
        }
        waitingShopTransition = true;
        isOpen = false;
        shopContainer.style.translate = new Translate(Length.Percent(-120), 0, 0);
    }

    public void CloseInfo()
    {
        
        if (waitingInfoTransition)
        {
            return;
        }

        waitingInfoTransition = true;
        infoElement.style.translate = new Translate(Length.Percent(120),0,0);
        isInfoOpen = false;
    }

    public void OpenInfo()
    {
        if (isInfoOpen)
        {
            return;
        }
        isInfoOpen = true;
        if (waitingInfoTransition)
            return;
        infoElement.style.display = DisplayStyle.Flex;
        waitingInfoTransition = true;
        infoElement.style.translate = new Translate(Length.Percent(0),0,0);
        
    }

    public void OpenShop()
    {
        uIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        if (waitingShopTransition)
        {
            //Debug.Log("waitingShopTransition was true");
            return;
        }
        waitingShopTransition = true;
        isOpen = true;
        shopContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
    }
    

    private void OnDestroy()
    {
        if (shopContainer != null)
            shopContainer.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);

        if (infoElement != null)
            infoElement.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);  
    }
}
