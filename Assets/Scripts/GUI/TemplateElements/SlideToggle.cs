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
        private static readonly string valueLabelClassName = "slide-toggle__value";
        private static readonly string inputContainerClassName = "slide-toggle-container";

        VisualElement inputContainer;
        VisualElement inputElement;
        VisualElement knobElement;
        Label valueElement;
        Label titleElement;

        Color disabledColor = new Color32(0, 0, 0, 255);
        Color enabledColor = new Color32(150,150,150,255);
        string title = "Title";

        string toggled = "On";
        string untoggled = "Off";
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

        [UxmlAttribute] public string ToggleText
        {
            get => toggled;
            set
            {
                if (toggled == value)
                {
                    return;
                }
                toggled = value;
                UpdateVisuals();
            }
        }

        [UxmlAttribute] public string UnToggleText
        {
            get => untoggled;
            set
            {
                if (untoggled == value)
                {
                    return;
                }
                untoggled = value;

                UpdateVisuals();
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

            inputContainer = new VisualElement();
            inputContainer.AddToClassList(inputContainerClassName);
            inputContainer.style.flexDirection = FlexDirection.Row;
            Add(inputContainer);

            valueElement = new Label{
                text = untoggled
            };
            valueElement.AddToClassList(valueLabelClassName);
            inputContainer.Add(valueElement);

            inputElement = this.Q(className: inputUssClassName);
            inputElement.AddToClassList(inputClassName);
            inputElement.style.backgroundColor = disabledColor;

            knobElement = new VisualElement();
            knobElement.AddToClassList(knobClassName);
            inputElement.Add(knobElement);
            inputContainer.Add(inputElement);

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
            
            valueElement.text = value ? toggled : untoggled;
        }
    }
}

