using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


namespace BasicUIControls
{
    [UxmlElement]
    public partial class CustomIntSlider : VisualElement
    {
        private readonly string className = "custom-int-slider";
        private readonly string intSliderClassName = "custom-int-slider__input";
        private readonly string intSliderTextFieldClass = "custom-int-slider__input-field";
        private readonly string fieldContainerClass = "custom-int-slider-field-container";
        private readonly string containerClass = "custom-int-slider-container";
        private readonly string intSliderTitleClass = "custom-int-slider-title";

        private VisualElement container;
        private VisualElement fieldContainer;
        private SliderInt slider;
        private IntegerField intField;
        private Label titleLabel;

        [Header("Title")]
        private string title = "Title";
        [UxmlAttribute] public string Title
        {
            get => title;
            set
            {
                if (title == value)
                {
                    return;
                }

                title = value;
                if (titleLabel != null)
                {
                    titleLabel.text = title;
                }
            }
        }

        [Header("Values")]
        private int intValue = 10;
        private int lowestValue = 0;
        private int highestValue = 100;
        [UxmlAttribute]
        public int Value
        {
            get => intValue;
            set
            {
                if (intValue == value)
                {
                    return;
                }

                intValue = Mathf.Clamp(value, lowestValue, highestValue);

                if (intField != null)
                {
                    intField.value = intValue;
                }

                if (slider != null)
                {
                    slider.value = intValue;
                }
            }
        }

        [UxmlAttribute]
        public int LowestValue
        {
            get => lowestValue;
            set
            {
                if (lowestValue == value)
                {
                    return;
                }
                lowestValue = value;

                if (lowestValue >= highestValue)
                {
                    lowestValue = highestValue - 1;
                }

                if (slider != null)
                {
                    slider.lowValue = lowestValue;
                }
            }
        }
        [UxmlAttribute]
        public int HighestValue
        {
            get => highestValue;
            set
            {
                if (highestValue == value)
                {
                    return;
                }
                
                highestValue = value;

                if (highestValue <= lowestValue)
                {
                    highestValue = lowestValue + 1;
                }

                if (slider != null)
                {
                    slider.highValue = highestValue;
                }
            }
        }

        [UxmlAttribute] public string SettingName { get; set; }

        public void OnFieldChanged(ChangeEvent<int> evt)
        {
            intValue = Mathf.Clamp(evt.newValue, slider.lowValue, slider.highValue);
            if (slider != null)
            {
                slider.value = intValue;
            }

            if (intField != null)
            {
                intField.value = intValue;
            }

        }
        public CustomIntSlider()
        {
            AddToClassList(className);
            container = new();
            container.AddToClassList(containerClass);


            titleLabel = new()
            {
                text = "Title"
            };
            titleLabel.AddToClassList(intSliderTitleClass);
            container.Add(titleLabel);

            fieldContainer = new();
            fieldContainer.AddToClassList(fieldContainerClass);
            container.Add(fieldContainer);



            slider = new()
            {
                lowValue = lowestValue,
                highValue = highestValue,
                value = intValue,
                focusable = false
            };
            
            slider.AddToClassList(intSliderClassName);
            fieldContainer.Add(slider);

            intField = new()
            {
                value = intValue
            };
            intField.AddToClassList(intSliderTextFieldClass);
            fieldContainer.Add(intField);
            Add(container);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            slider?.RegisterValueChangedCallback(OnFieldChanged);
            intField?.RegisterValueChangedCallback(OnFieldChanged);
        }
        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            slider?.UnregisterValueChangedCallback(OnFieldChanged);
            intField?.UnregisterValueChangedCallback(OnFieldChanged);
        }
    }
}



