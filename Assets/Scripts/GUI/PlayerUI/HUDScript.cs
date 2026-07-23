using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDScript : MonoBehaviour
{
    [SerializeField] private UIDocument uIDocument;

    public float health = 100;
    public float stamina = 100;
    private float lastHealth;
    private float lastStamina;

    private Label healthLabel;
    private Label staminaLabel;
    public Label cooldownLabel;
    private VisualElement healthProgressElement;
    private VisualElement staminaProgressElement;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var root = uIDocument.rootVisualElement;
        healthLabel = root.Q<Label>("health-label");
        staminaLabel = root.Q<Label>("stamina-label");
        healthProgressElement = root.Q<VisualElement>("health-progress-bar");
        staminaProgressElement = root.Q<VisualElement>("stamina-progress-bar");
        cooldownLabel = root.Q<Label>("cooldown-text");

        root.style.display = DisplayStyle.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (health != lastHealth)
        {
            healthLabel.text = $"HP: {Mathf.Clamp(Mathf.FloorToInt(health), 0f, 100f)}";
            healthProgressElement.style.width = new StyleLength(Length.Percent(Mathf.Clamp(health, 0f, 100f)));
            lastHealth = health;
        }
        
        if (stamina != lastStamina)
        {
            staminaLabel.text = $"STA: {Mathf.Clamp(Mathf.FloorToInt(stamina), 0f, 100f)}";
            staminaProgressElement.style.width = new StyleLength(Length.Percent(Mathf.Clamp(stamina, 0f, 100f)));
            lastStamina = stamina;
        }
    }

    public void DamageHealth(float damage)
    {
        health -= damage;
    }

}
