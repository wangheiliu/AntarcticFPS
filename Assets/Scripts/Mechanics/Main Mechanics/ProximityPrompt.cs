using System;
using Game.ScriptableObjects.Shop;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public interface IProximityPromptTriggerable
{
    public void OnProximityPromptTrigger();
    public void OnProximityPromptRelease();
    public void OnProximityPromptHold();
}

[RequireComponent(typeof(PanelRenderer))]
public class ProximityPrompt : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private PanelRenderer document;
    [Header("Players")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private GameObject character;
    [SerializeField] private IProximityPromptTriggerable triggerableScript;

    [Header("Prompt Settings")]
    [SerializeField] private Key key = Key.E;
    [SerializeField] private float visibilityDistance = 10f;
    [SerializeField] private float maxHeldTime = 3f;

    private float wasHeldFor = 0f;
    private float WasHeldFor
    {
        get => wasHeldFor;
        set
        {
            if (wasHeldFor == value) return;
            wasHeldFor = value;
            if (promptProgress == null) return;
            float progress = wasHeldFor / maxHeldTime;
            promptProgress.style.height = Length.Percent(progress * 100f);
            CalculateBorderRadius(progress);
        }
    }

    #region UI Variables
    private VisualElement root;
    private Label prompt;
    private VisualElement promptProgress;

    private Label promptTitle;
    private Label promptSubTitle;

    private int currVersion = -1;
    #endregion

    public event Action OnPromptTrigger;
    public event Action OnPromptRelease;
    public event Action OnPromptHold;

    void OnEnable()
    {
        document.RegisterUIReloadCallback(OnUIRefresh);
    }

    void OnDisable()
    {
        document.UnregisterUIReloadCallback(OnUIRefresh);
    }

    void OnUIRefresh(Renderer renderer, VisualElement _root, int version)
    {
        if (currVersion == version) return;
        currVersion = version;

        root = _root;
        prompt = root.Q<Label>(className: "prompt-box");
        promptProgress = prompt.Q<VisualElement>(className: "prompt-progress");

        promptTitle = root.Q<Label>(className: "prompt-title");
        promptSubTitle = root.Q<Label>(className: "prompt-subtitle");
        SetPromptBinding();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        FacePromptBox();

        if (root != null)
        {
            if (CalculateDistance())
            {
                root.style.display = DisplayStyle.Flex;
            }
            else
            {
                root.style.display = DisplayStyle.None;
            }
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current[key].isPressed && CalculateDistance())
            {
                if (maxHeldTime == 0f)
                {
                    OnPromptTrigger?.Invoke();
                    return;
                }
                WasHeldFor += Time.deltaTime;
            }

            if (wasHeldFor >= maxHeldTime && maxHeldTime > 0f)
            {
                WasHeldFor = 0f;
                OnPromptTrigger?.Invoke();
                // do logic here
            }

            if (Keyboard.current[key].wasReleasedThisFrame || !CalculateDistance())
            {
                WasHeldFor = 0f;
            }

        }
    }

    private void FacePromptBox()
    {

        if (playerCamera == null) return;
        if (root == null) return;

        transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward);
        root.style.display = DisplayStyle.Flex;
        /* if (Physics.Raycast(origin: transform.position, 
            direction: character.transform.position, 
            maxDistance: visibilityDistance, 
            layerMask: LayerMask.GetMask("Player"),
            hitInfo: out RaycastHit hit))
        {
            root.style.display = DisplayStyle.Flex;   
        } else
        {
            root.style.display = DisplayStyle.None;
        } */
    }

    protected virtual void SetPromptBinding()
    {
        if (promptTitle == null || promptSubTitle == null) return;

        promptTitle.SetBinding(nameof(Label.text), new DataBinding()
        {
            dataSourceType = typeof(ShopItemData),
            dataSourcePath = new(nameof(ShopItemData.Title))
        });
        promptSubTitle.SetBinding(nameof(Label.text), new DataBinding()
        {
            dataSourceType = typeof(ShopItemData),
            dataSourcePath = new(nameof(ShopItemData.promptSubtitle))
        });
    }

    private void CalculateBorderRadius(float progress)
    {
        progress = Mathf.Clamp01(progress);

        float width = prompt.resolvedStyle.width;
        float height = prompt.resolvedStyle.height;

        if (float.IsNaN(width) || float.IsNaN(height)) return;

        float targetRadius = Mathf.Max(0, 20f);
        float progressWidth = Mathf.Max(0f, width * progress);
        float maxFilledHeight = Mathf.Max(0f, height);


        float maxSafeRadius = Mathf.Min(progressWidth, maxFilledHeight) * 0.75f;
        float safeRadius = Mathf.Min(targetRadius, maxSafeRadius);

        Debug.Log($"Safe radius is {safeRadius}");
        Debug.Log($"Max Filled width is {progressWidth}");
        promptProgress.style.width = Length.Percent(progress * 100f);
        promptProgress.style.borderTopLeftRadius = safeRadius;
        promptProgress.style.borderTopRightRadius = safeRadius;
        promptProgress.style.borderBottomLeftRadius = safeRadius;
        promptProgress.style.borderBottomRightRadius = safeRadius;
    }

    private bool CalculateDistance()
    {
        Vector3 distance = character.transform.position - transform.position;
        float magnitude = distance.sqrMagnitude;

        if (magnitude < (visibilityDistance * visibilityDistance))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
