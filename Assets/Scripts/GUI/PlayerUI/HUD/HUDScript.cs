using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private PanelRenderer uIDocument;
    private event Action OnHealthChanged;
    private event Action OnStaminaChanged;
    public float health = 100;
    public float stamina = 100;

    public float Health
    {
        get => health;
        set
        {
            if (health == value)
            {
                return;
            }
            health = Mathf.Clamp(value, 0f, 100f);
            OnHealthChanged?.Invoke();
        }
    }

    public float Stamina
    {
        get => stamina;
        set
        {
            if (stamina == value)
            {
                return;
            }
            stamina = Mathf.Clamp(value, 0f, 100f);
            OnStaminaChanged?.Invoke();
        }
    }

    private VisualElement root;
    private Label healthLabel;
    private Label staminaLabel;
    [NonSerialized] public Label cooldownLabel;
    private VisualElement healthProgressElement;
    private VisualElement staminaProgressElement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OnEnable()
    {
        uIDocument.RegisterUIReloadCallback(OnGUIReload);
    }

    private void OnGUIReload(PanelRenderer renderer, VisualElement _root, int currentVersion)
    {
        root = _root;
        healthLabel = root.Q<Label>("health-label");
        staminaLabel = root.Q<Label>("stamina-label");
        healthProgressElement = root.Q<VisualElement>("health-progress-bar");
        staminaProgressElement = root.Q<VisualElement>("stamina-progress-bar");
        cooldownLabel = root.Q<Label>("cooldown-text");

        OnHealthChanged += UpdateHealthGUI;
        OnStaminaChanged += UpdateStaminaGUI;
        root.style.display = DisplayStyle.None;
        UpdateHealthGUI();
        UpdateStaminaGUI();
    }

    public void DamageHealth(float damage)
    {
        Health -= damage;
    }

    private void UpdateHealthGUI()
    {
        healthLabel.text = $"HP: {Mathf.Clamp(Mathf.FloorToInt(health), 0f, 100f)}";
        healthProgressElement.style.width = new StyleLength(Length.Percent(Mathf.Clamp(health, 0f, 100f)));
    }

    private void UpdateStaminaGUI()
    {
        staminaLabel.text = $"STA: {Mathf.Clamp(Mathf.FloorToInt(stamina), 0f, 100f)}";
        staminaProgressElement.style.width = new StyleLength(Length.Percent(Mathf.Clamp(stamina, 0f, 100f)));
    }
}
