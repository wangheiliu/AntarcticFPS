using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace BasicUIControls
{
    [UxmlElement]
    public partial class SlideToggle : BaseField<bool> // base refers to the parent class you inherit from, in this case, it's BaseField
    {
        private static readonly string className = "slide-toggle";
        private static readonly string inputClassName = "slide-toggle__input";
        private static readonly string checkedInputClassName = "slide-toggle__input--checked";
        private static readonly string knobClassName = "slide-toggle__input-knob"; 
        private static readonly string titleClassName = "slide-toggle-title";

        VisualElement inputElement;
        VisualElement knobElement;
        Label titleElement;

        Color disabledColor = new Color32(0, 0, 0, 255);
        Color enabledColor = new Color32(150,150,150,255);
        string title = "Title";
        [UxmlAttribute] public Color DisabledColor {
            get => disabledColor;
            set => disabledColor = value;
        }
        [UxmlAttribute] public Color EnabledColor { get => enabledColor; set => enabledColor = value; }
        [UxmlAttribute] public string TitleName
        {
            get => title;
            set
            {
                if (title == value)
                {
                    return;
                }
                title = value;
                if (titleElement != null)
                {
                    titleElement.text = title;
                }
            }
        }

        //constructor class
        public SlideToggle(): base(null, new VisualElement()) // the basefield provides us with the label and the input visual element
        {
            AddToClassList(className);
            titleElement = new Label
            {
                text = title
            };
            titleElement.AddToClassList(titleClassName);
            Add(titleElement);

            inputElement = this.Q(className: inputUssClassName);
            inputElement.AddToClassList(inputClassName);
            inputElement.style.backgroundColor = disabledColor;

            knobElement = new VisualElement();
            knobElement.AddToClassList(knobClassName);
            inputElement.Add(knobElement);
            Add(inputElement);

            RegisterCallback<ClickEvent>(OnToggle);
            UpdateVisuals();
        }

        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            UpdateVisuals();
        }

        void OnToggle(ClickEvent _)
        {
            value = !value;
            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            inputElement.EnableInClassList(checkedInputClassName, value);
            inputElement.style.backgroundColor = value ? EnabledColor : DisabledColor;
        }
    }
}

