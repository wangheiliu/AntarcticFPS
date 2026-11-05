using System.ComponentModel;
using System;
using Game.ScriptableObjects.Settings;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace BasicUIControls
{
    [UxmlElement]
    public partial class SlideToggle : BaseField<bool>, ISettingAttributes // base refers to the parent class you inherit from, in this case, it's BaseField
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

        public DataBinding titleBinding = new()
        {
            dataSourcePath = new(nameof(ToggleInfoObj.Title)),
            bindingMode = BindingMode.ToTarget
        };
        [UxmlAttribute, CreateProperty] public string TitleName
        {
            get => title;
            set
            {
                if (title == value)
                {
                    return;
                }
                title = value;
                titleBinding.MarkDirty();
                if (titleElement != null)
                {
                    titleElement.text = title;
                }
            }
        }

        public DataBinding toggleTextBinding = new()
        {
            dataSourcePath = new(nameof(ToggleInfoObj.ToggledText)),
            bindingMode = BindingMode.ToTarget
        };
        [UxmlAttribute, CreateProperty] public string ToggleText
        {
            get => toggled;
            set
            {
                if (toggled == value)
                {
                    return;
                }
                string prevValue = toggled;
                toggled = value;
                toggleTextBinding.MarkDirty();
                UpdateVisuals();
            }
        }

        public DataBinding untoggleTextBinding = new()
        {
            dataSourcePath = new(nameof(ToggleInfoObj.UntoggledText)),
            bindingMode = BindingMode.ToTarget
        };
        [UxmlAttribute, CreateProperty] public string UnToggleText
        {
            get => untoggled;
            set
            {
                if (untoggled == value)
                {
                    return;
                }
                untoggled = value;
                untoggleTextBinding.MarkDirty();
                UpdateVisuals();
            }
        }

        [CreateProperty] public string ValueDisplay => value ? ToggleText : UnToggleText;
        
        
        private string dataName = "";
        public DataBinding dataNameBinding = new()
        {
            dataSourcePath = new(nameof(ToggleInfoObj.DataName)),
            bindingMode = BindingMode.ToTarget
        };
        [UxmlAttribute, CreateProperty] public string DataName
        {
            get => dataName;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Debug.LogWarning("DataName cannot be null or empty.");
                    return;
                }
                if (dataName == value)
                {
                    return;
                }
                dataNameBinding.MarkDirty();
                dataName = value;
            }
        }

        //constructor class
        public SlideToggle(): base(null, new VisualElement()) // the basefield provides us with the label and the input visual element
        {
            dataSourceType = typeof(ToggleInfoObj);
            SetBinding(nameof(TitleName), titleBinding);

            SetBinding(nameof(UnToggleText), untoggleTextBinding);

            SetBinding(nameof(ToggleText), toggleTextBinding);

            /* SetBinding(nameof(value), new DataBinding()
            {
               dataSourcePath = new(nameof(ToggleInfoObj.Value)),
               bindingMode = BindingMode.TwoWay
            }); */

            SetBinding(nameof(DataName), dataNameBinding);
            
            AddToClassList(className);
            titleElement = new Label()
            {
                text = TitleName
            };

            titleElement.SetBinding(nameof(Label.text), titleBinding);
            titleElement.AddToClassList(titleClassName);
            Add(titleElement);

            inputContainer = new VisualElement();
            inputContainer.AddToClassList(inputContainerClassName);
            inputContainer.style.flexDirection = FlexDirection.Row;
            
            Add(inputContainer);

            valueElement = new Label{
                text = ValueDisplay
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
            valueElement.text = ValueDisplay;
        }
    }
}

