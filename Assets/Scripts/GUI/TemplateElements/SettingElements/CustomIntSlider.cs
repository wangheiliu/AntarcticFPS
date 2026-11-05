using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Game.ScriptableObjects.Settings;
using Player.PlayerData;
using Unity.Properties;


namespace BasicUIControls
{
    [UxmlElement]
    public partial class CustomIntSlider : BaseSettingElement
    {
        #region Class Names
        private readonly string className = "custom-int-slider";
        private readonly string intSliderClassName = "custom-int-slider__input";
        private readonly string intSliderTextFieldClass = "custom-int-slider__input-field";
        private readonly string fieldContainerClass = "custom-int-slider-field-container";
        private readonly string containerClass = "custom-int-slider-container";
        private readonly string intSliderTitleClass = "custom-int-slider-title";
        #endregion

        #region UI Elements
        private VisualElement container;
        private VisualElement fieldContainer;
        private SliderInt slider;
        private IntegerField intField;
        private Label titleLabel;
        #endregion

        #region UI Attributes
        [Header("Title")]

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

        public DataBinding lowestValueBinding = new()
        {
            dataSourcePath = new(nameof(SliderInfoObj.LowerRange)),
            bindingMode = BindingMode.ToTarget
        };
        [UxmlAttribute, CreateProperty]
        public int LowestValue
        {
            get => lowestValue;
            set
            {
                if (lowestValue == value)
                {
                    return;
                }
                lowestValueBinding.MarkDirty();
                lowestValue = value;

                if (lowestValue >= highestValue)
                {
                    lowestValue = highestValue - 1;
                }

                if (slider != null)
                {
                    slider.lowValue = lowestValue;
                    UpdateVisuals();
                }
            }
        }

        public DataBinding highestValueBinding = new()
        {
            dataSourcePath = new(nameof(SliderInfoObj.HighRange)),
            bindingMode = BindingMode.ToTarget
        };
        [UxmlAttribute, CreateProperty]
        public int HighestValue
        {
            get => highestValue;
            set
            {
                if (highestValue == value)
                {
                    return;
                }
                highestValueBinding.MarkDirty();
                highestValue = value;

                if (highestValue <= lowestValue)
                {
                    highestValue = lowestValue + 1;
                }

                if (slider != null)
                {
                    slider.highValue = highestValue;
                    UpdateVisuals();
                }
            }
        }

        #endregion

        public void OnFieldChanged(ChangeEvent<int> evt)
        {
            Value = Mathf.Clamp(evt.newValue, slider.lowValue, slider.highValue);
            if (slider != null)
            {
                slider.value = Value;
            }

            if (intField != null)
            {
                intField.value = Value;
            }

        }
        public CustomIntSlider()
        {
            InitBaseDataBinding();
            dataSourceType = typeof(SliderInfoObj);

            SetBinding(nameof(Title), titleBinding);
            
            SetBinding(nameof(DataName), dataNameBinding);

            SetBinding(nameof(LowestValue), lowestValueBinding);

            SetBinding(nameof(HighestValue), highestValueBinding);

            AddToClassList(className);
            container = new();
            container.AddToClassList(containerClass);


            titleLabel = new()
            {
                text = Title
            };

            titleLabel.SetBinding(nameof(Label.text), titleBinding);
            titleLabel.AddToClassList(intSliderTitleClass);
            container.Add(titleLabel);

            fieldContainer = new();
            fieldContainer.AddToClassList(fieldContainerClass);
            container.Add(fieldContainer);

            slider = new()
            {
                lowValue = LowestValue,
                highValue = HighestValue,
                value = intValue,
                focusable = false
            };

            slider.SetBinding(nameof(SliderInt.lowValue), lowestValueBinding);
            slider.SetBinding(nameof(SliderInt.highValue), highestValueBinding);

            slider.AddToClassList(intSliderClassName);
            fieldContainer.Add(slider);

            intField = new()
            {
                value = Value
            };
            intField.AddToClassList(intSliderTextFieldClass);
            fieldContainer.Add(intField);
            Add(container);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        protected override void UpdateVisuals()
        {
            if (slider != null)
            {
                slider.value = Mathf.Clamp(Value, slider.lowValue, slider.highValue);
            }
        }

        protected override void OnAttachToPanel(AttachToPanelEvent evt)
        {
            slider?.RegisterValueChangedCallback(OnFieldChanged);
            intField?.RegisterValueChangedCallback(OnFieldChanged);
        }
        protected override void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            slider?.UnregisterValueChangedCallback(OnFieldChanged);
            intField?.UnregisterValueChangedCallback(OnFieldChanged);
        }
    }
}



