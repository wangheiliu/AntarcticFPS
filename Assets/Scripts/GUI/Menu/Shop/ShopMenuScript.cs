using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Threading.Tasks;
using System.Collections;

public class ShopMenuScript : MonoBehaviour
{
    [Header("Shop Scripts")]
    [SerializeField] private ShopFilter shopFilterScript;
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
    private bool isInfoOpen = false;
    public bool isFiltersOpen = false;
    private TabView tabContainer;
    private VisualElement titleContainer;
    private VisualElement infoElement;
    private VisualElement shopContainer;
    private VisualElement filtersContainer;
    private Button closeButton;
    private Button infoCloseButton;
    private Button filtersButton;
    

    void Start()
    {
        var root = uIDocument.rootVisualElement;
        infoElement = root.Q<VisualElement>("info-container");
        infoCloseButton = infoElement.Q<Button>("info-close-button");
        shopContainer = root.Q<VisualElement>("shop-container");
        filtersButton = root.Q<Button>("filter-button");
        filtersContainer = root.Q<VisualElement>("filter-container");

        root.style.display = DisplayStyle.None;
        shopContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoElement.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        //infoCloseButton.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

        if (filtersButton != null && filtersContainer != null)
        {
            filtersButton.clicked += FiltersTransition;
        }
        
        StartCoroutine(InitNextFrame());
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == infoElement)
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
            gameManager.OpenMenuItems(MenuState.MainMenu);
        }
    }

    private IEnumerator InitNextFrame()
    {
        yield return null;
        var root = uIDocument.rootVisualElement;
        closeButton = root.Q<Button>("CloseButton");

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

        waitingInfoTransition = true;
        isInfoOpen = false;
        infoElement.style.translate = new Translate(Length.Percent(120), 0, 0);
        filtersContainer.style.translate = new Translate(Length.Percent(-75), 0, 0);
        isFiltersOpen = false;
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

    public void FiltersTransition()
    {
        if (!isFiltersOpen)
        {
            filtersContainer.style.translate = new Translate(Length.Percent(0), 0, 0);
            isFiltersOpen = true;
        } else
        {
            filtersContainer.style.translate = new Translate(Length.Percent(-75), 0, 0);
            isFiltersOpen = false;
            shopFilterScript.ResetFilters();
        }
    }

    private void OnDestroy()
    {
        shopContainer?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);

        infoElement?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);  
    }
}
