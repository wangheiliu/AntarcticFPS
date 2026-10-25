using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;
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

    private Label healthLabel;
    private Label staminaLabel;
    public Label cooldownLabel;
    private VisualElement healthProgressElement;
    private VisualElement staminaProgressElement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OnEnable()
    {
        var root = uIDocument.rootVisualElement;
        healthLabel = root.Q<Label>("health-label");
        staminaLabel = root.Q<Label>("stamina-label");
        healthProgressElement = root.Q<VisualElement>("health-progress-bar");
        staminaProgressElement = root.Q<VisualElement>("stamina-progress-bar");
        cooldownLabel = root.Q<Label>("cooldown-text");

        OnHealthChanged += UpdateHealthGUI;
        OnStaminaChanged += UpdateStaminaGUI;
    }
    void Start()
    {
        uIDocument.rootVisualElement.style.display = DisplayStyle.None;
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
