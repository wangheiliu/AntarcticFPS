using UnityEngine;
using UnityEngine.UIElements;

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

    // maybe use a ternrary operator for these?
    private bool isOpen = false;
    private bool isInfoOpen = false;
    public bool isFiltersOpen = false;

    private static readonly Translate defaultTransitionValue = new(Length.Percent(0), 0, 0);
    private static readonly Translate shopClosedTransitionValue = new(Length.Percent(-120), 0, 0);
    private static readonly Translate infoClosedTransitionValue = new(Length.Percent(120), 0, 0);
    private static readonly Translate filtersClosedTransition = new(Length.Percent(-75), 0, 0);
    private TabView tabContainer;
    private VisualElement titleContainer;
    private VisualElement infoElement;
    private VisualElement shopContainer;
    private VisualElement filtersContainer;
    private Button closeButton;
    private Button infoCloseButton;
    private Button filtersButton;


    void OnEnable()
    {
        var root = uIDocument.rootVisualElement;
        infoElement = root.Q<VisualElement>("info-container");
        filtersContainer = root.Q<VisualElement>("filter-container");
        shopContainer = root.Q<VisualElement>("shop-container");

        infoCloseButton = infoElement.Q<Button>("info-close-button");
        filtersButton = root.Q<Button>("filter-button");
        closeButton = root.Q<Button>("CloseButton");


        root.style.display = DisplayStyle.None;
        shopContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoElement.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        //infoCloseButton.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

        if (filtersButton != null && filtersContainer != null)
        {
            filtersButton.clicked += FiltersTransition;
        }

        if (closeButton != null)
        {
            closeButton.clicked += CloseShop;
        }

        if (infoCloseButton != null)
        {
            infoCloseButton.clicked += CloseInfo;
        }

    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == shopContainer)
        {
            waitingShopTransition = false;
            if (!isOpen)
            {
                gameManager.OpenMenuItems(MenuState.MainMenu);
            }
        } else if (evt.target == infoElement)
        {
            waitingInfoTransition = false;
            if (!isInfoOpen)
            {
                infoElement.style.display = DisplayStyle.None;
            }
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

        shopContainer.style.translate = shopClosedTransitionValue;


        isInfoOpen = false;
        infoElement.style.translate = infoClosedTransitionValue;

        filtersContainer.style.translate = filtersClosedTransition;
        isFiltersOpen = false;
    }

    public void CloseInfo()
    {

        if (waitingInfoTransition)
        {
            return;
        }

        waitingInfoTransition = true;
        infoElement.style.translate = infoClosedTransitionValue;
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
        infoElement.style.translate = defaultTransitionValue;

    }

    public void OpenShop()
    {
        uIDocument.rootVisualElement.style.display = DisplayStyle.Flex;
        if (waitingShopTransition)
        {
            return;
        }
        waitingShopTransition = true;
        isOpen = true;
        shopContainer.style.translate = defaultTransitionValue;
    }

    public void FiltersTransition()
    {
        if (!isFiltersOpen)
        {
            filtersContainer.style.translate = defaultTransitionValue;
            isFiltersOpen = true;
        }
        else
        {
            filtersContainer.style.translate = filtersClosedTransition;
            isFiltersOpen = false;
            shopFilterScript.ResetFilters();
        }
    }

    private void OnDisable()
    {
        shopContainer?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);

        infoElement?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
    }
}
