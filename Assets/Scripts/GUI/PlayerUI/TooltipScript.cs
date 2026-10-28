using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class TooltipScript : MonoBehaviour
{
    [SerializeField] private PanelRenderer tooltipDocument;
    private VisualElement root;
    private Label tooltipText;
    private VisualElement container;
    private IPanel panel;

    void OnEnable()
    {
        tooltipDocument.RegisterUIReloadCallback(OnGUIReload);
        
        Hide();
    }

    private void OnGUIReload(PanelRenderer renderer, VisualElement _root, int currentVersion)
    {
        root = _root;
        tooltipText = root.Q<Label>(className: "tooltip-label");
        container = root.Q<VisualElement>(className: "tooltip-container");

        panel = root.panel;
    }

    private void OnDisable()
    {
        tooltipDocument.UnregisterUIReloadCallback(OnGUIReload);
    }

    void Update()
    {
        float offsetX = 15f;
        float offsetY = 15f;
        if (panel == null || Mouse.current == null)
        {
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
            container.style.left = Mathf.Clamp(mousePos.x + offsetX, 0, Screen.width);
            container.style.top = Mathf.Clamp(mousePos.y + offsetY, 0, Screen.height);
        }

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
        if (container != null)
        {
            container.style.display = DisplayStyle.None;
        }
        
        if (tooltipText != null)
        {
            tooltipText.text = "";
        }
        
    }
}
