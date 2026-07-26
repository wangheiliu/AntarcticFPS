using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class TooltipScript : MonoBehaviour
{
    [SerializeField] private UIDocument tooltipDocument;
    private UIDocument[] documents;
    private Label tooltipText;
    private VisualElement container;
    private IPanel panel;

    void OnEnable()
    {
        Debug.Log("Running");
        var root = tooltipDocument.rootVisualElement;
        Debug.Log($"Root = {root}");
        tooltipText = root.Q<Label>(className: "tooltip-label");
        container = root.Q<VisualElement>(className: "tooltip-container");

        panel = root.panel;
        Hide();
    }

    void Update()
    {
        float offsetX = 15f;
        float offsetY = 15f;
        if (panel == null || Mouse.current == null)
        {
            Debug.LogWarning("panel is null");
            return;
        }
        Vector2 mousePos = Mouse.current.position.ReadValue();
        mousePos.y = Screen.height - mousePos.y; // because in this unity version, the mouse origin's y is in the bottom left corner (bro why)
        mousePos = RuntimePanelUtils.ScreenToPanel(panel, mousePos);
        
        VisualElement targetElement = panel.Pick(mousePos);
        if (targetElement == null)
        {
            Hide();
            return;
        } else
        {
            ShowTooltip(targetElement);
            container.style.left = mousePos.x + offsetX;
            container.style.top = mousePos.y + offsetY;
        }
        Debug.Log($"Left: {container.resolvedStyle.left}. Top: {container.resolvedStyle.top}");

    }

    private void ShowTooltip(VisualElement element)
    {
        container.style.display = DisplayStyle.Flex;
        if (string.IsNullOrWhiteSpace(element.tooltip))
        {
            Hide();
            return;
        }
        tooltipText.text = element.tooltip;
    }

    private void Hide()
    {
        container.style.display = DisplayStyle.None;
        tooltipText.text = "";
    }
}
