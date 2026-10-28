using UnityEngine;
using UnityEngine.UIElements;

namespace BasicUIControls
{
    [UxmlElement]
    public partial class BaseSettingElement : VisualElement, ISettingAttributes
    {
        [UxmlAttribute] public string Title {get; set;}
        [UxmlAttribute] public object Value {get; set;}

        [UxmlAttribute] public string DataName { get; set; }
        protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt) {}
        protected virtual void OnAttachFromPanel(AttachToPanelEvent evt) {}

    }
}
    
