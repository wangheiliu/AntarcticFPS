using System.Reflection.Metadata;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopMenuScript : MonoBehaviour
{
    [Header("Shop Scripts")]
    [SerializeField] private ShopFilter shopFilterScript;
    [Header("UI Documents")]
    [SerializeField] private PanelRenderer uIDocument;
    [SerializeField] private PanelRenderer menuDocument;
    [Header("Main Menu Camera")]
    [SerializeField] private Camera menuCamera;
    [Header("Game Manager")]
    [SerializeField] private GameManager gameManager;


    private bool waitingShopTransition = false;

    // maybe use a ternrary operator for these?
    private bool isOpen = false;
    public bool isInfoOpen = false;
    public bool isFiltersOpen = false;
    private bool isPromptOpen = false; 

    private static readonly Translate defaultTransitionValue = new(Length.Percent(0), 0, 0);
    private static readonly Translate shopClosedTransitionValue = new(Length.Percent(-120), 0, 0);
    private static readonly Translate infoClosedTransitionValue = new(Length.Percent(120), 0, 0);
    private static readonly Translate filtersClosedTransition = new(Length.Percent(-75), 0, 0);
    private static readonly Translate promptClosedTransition = new(0, Length.Percent(105), 0);

    private VisualElement root;
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
        uIDocument.RegisterUIReloadCallback(OnUIReloaded);
    }

    void OnUIReloaded(PanelRenderer renderer, VisualElement _root, int currentVersion)
    {
        root = _root;
        infoElement = root.Q<VisualElement>("info-container");
        filtersContainer = root.Q<VisualElement>("filter-container");
        shopContainer = root.Q<VisualElement>("shop-container");
        promptContainer = root.Q<VisualElement>("prompt-container");

        infoCloseButton = infoElement.Q<Button>("info-close-button");
        filtersButton = root.Q<Button>("filter-button");
        closeButton = root.Q<Button>("CloseButton");
        purchaseButton = infoElement.Q<Button>("purchase-button");
        cancelPurchase = promptContainer.Q<Button>("prompt-cancel-button");

        shopContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoElement.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        promptContainer.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        //infoCloseButton.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);

        closeButton.RegisterCallback<ClickEvent>(CloseShop);
        infoCloseButton.RegisterCallback<ClickEvent>(CloseInfo);
        filtersButton?.RegisterCallback<ClickEvent>(FiltersTransition);
        purchaseButton?.RegisterCallback<ClickEvent>(evt => OnPurchasePrompt(evt, true), CallbackOptions.Removable);
        cancelPurchase?.RegisterCallback<ClickEvent>(evt => OnPurchasePrompt(evt, false), CallbackOptions.Removable);

        gameManager.OnMenuStateChanged += OnShopOpen;
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (evt.target == shopContainer)
        {
            if (!isOpen)
            {
                Debug.Log("Shop isn't open");
                gameManager.OnEventNotifierChanged(MenuState.MainMenu);
            }
        } else if (evt.target == infoElement)
        {
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

    public void CloseShop(ClickEvent _)
    {
        isOpen = false;

        shopContainer.style.translate = shopClosedTransitionValue;


        isInfoOpen = false;
        infoElement.style.translate = infoClosedTransitionValue;

        filtersContainer.style.translate = filtersClosedTransition;
        isFiltersOpen = false;
    }

    public void CloseInfo(ClickEvent _)
    {
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
        infoElement.style.display = DisplayStyle.Flex;
        infoElement.style.translate = defaultTransitionValue;
    }

    public void OpenShop()
    {
        Debug.Log("Running...");
        isOpen = true;
        root.style.display = DisplayStyle.Flex;
        
        
        shopContainer.style.translate = defaultTransitionValue;
    }

    public void FiltersTransition(ClickEvent _)
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
            shopContainer.style.translate = shopClosedTransitionValue;
            infoElement.style.translate = infoClosedTransitionValue;
            isFiltersOpen = true;
            FiltersTransition(null);

            isPromptOpen = true;
            promptContainer.style.display = DisplayStyle.Flex;
            promptContainer.style.translate = defaultTransitionValue;
        } else
        {
            shopContainer.style.translate = defaultTransitionValue;
            infoElement.style.translate = defaultTransitionValue;

            isPromptOpen = false;
            promptContainer.style.translate = promptClosedTransition;
        }
    }

    public void OnShopOpen(MenuState state)
    {
        if (state == MenuState.Shop)
        {
            Debug.Log("Opening shop...");
            uIDocument.enabled = true;
            root.style.display = DisplayStyle.Flex;
            OpenShop();
        }
    }

    private void OnDisable()
    {
        
        shopContainer?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        infoElement?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);

        filtersButton?.UnregisterAllRemovableCallbacks();
        closeButton?.UnregisterAllRemovableCallbacks();
        infoCloseButton?.UnregisterAllRemovableCallbacks();
        purchaseButton?.UnregisterAllRemovableCallbacks();
        cancelPurchase?.UnregisterAllRemovableCallbacks();
        gameManager.OnMenuStateChanged -= OnShopOpen;
        uIDocument.UnregisterUIReloadCallback(OnUIReloaded);
    }
}
