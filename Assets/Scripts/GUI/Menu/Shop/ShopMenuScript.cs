using NUnit.Framework;
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
    private bool isPromptOpen = false; 

    private static readonly Translate defaultTransitionValue = new(Length.Percent(0), 0, 0);
    private static readonly Translate shopClosedTransitionValue = new(Length.Percent(-120), 0, 0);
    private static readonly Translate infoClosedTransitionValue = new(Length.Percent(120), 0, 0);
    private static readonly Translate filtersClosedTransition = new(Length.Percent(-75), 0, 0);
    private static readonly Translate promptClosedTransition = new(0, Length.Percent(105), 0);
    private TabView tabContainer;
    private VisualElement titleContainer;
    private VisualElement infoElement;
    private VisualElement shopContainer;
    private VisualElement filtersContainer;
    private VisualElement promptContainer;
    private Button closeButton;
    private Button infoCloseButton;
    private Button filtersButton;
    private Button purchaseButton;
    private Button cancelPurchase;


    void OnEnable()
    {
        var root = uIDocument.rootVisualElement;
        infoElement = root.Q<VisualElement>("info-container");
        filtersContainer = root.Q<VisualElement>("filter-container");
        shopContainer = root.Q<VisualElement>("shop-container");
        promptContainer = root.Q<VisualElement>("prompt-container");

        infoCloseButton = infoElement.Q<Button>("info-close-button");
        filtersButton = root.Q<Button>("filter-button");
        closeButton = root.Q<Button>("CloseButton");
        purchaseButton = infoElement.Q<Button>("purchase-button");
        cancelPurchase = promptContainer.Q<Button>("prompt-cancel-button");


        root.style.display = DisplayStyle.None;
        shopContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoElement.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        promptContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
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

        purchaseButton?.RegisterCallback<ClickEvent>(evt => OnPurchasePrompt(evt, true), CallbackOptions.Removable);
        cancelPurchase?.RegisterCallback<ClickEvent>(evt => OnPurchasePrompt(evt, false), CallbackOptions.Removable);
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
        } else if (evt.target == promptContainer)
        {
            if (!isPromptOpen)
            {
                promptContainer.style.display = DisplayStyle.None;
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

    public void OnPurchasePrompt(ClickEvent _, bool closeItems)
    {
        HandlePrompt(closeItems);
    }

    public void HandlePrompt(bool closeItems)
    {
        if (closeItems)
        {
            if (waitingShopTransition)
            {
                return;
            }
            waitingShopTransition = true;
            shopContainer.style.translate = shopClosedTransitionValue;
            infoElement.style.translate = infoClosedTransitionValue;
            isFiltersOpen = true;
            FiltersTransition();

            isPromptOpen = true;
            promptContainer.style.display = DisplayStyle.Flex;
            promptContainer.style.translate = defaultTransitionValue;
        } else
        {
            waitingShopTransition = true;
            shopContainer.style.translate = defaultTransitionValue;
            infoElement.style.translate = defaultTransitionValue;

            isPromptOpen = false;
            promptContainer.style.translate = promptClosedTransition;
        }
    }

    private void OnDisable()
    {
        shopContainer?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoElement?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);

        purchaseButton.UnregisterAllRemovableCallbacks();
        cancelPurchase.UnregisterAllRemovableCallbacks();
    }
}
