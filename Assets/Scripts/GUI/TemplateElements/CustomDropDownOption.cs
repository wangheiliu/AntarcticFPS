using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace BasicUIControls
{
    [UxmlElement]
    public partial class CustomDropDownOption : VisualElement
    {
        private static readonly string className = "custom-horizontal-dropdown";
        private static readonly string containerClass = "custom-horizontal-dropdown-container";
        private static readonly string dropdownContainerClass = "custom-horizontal-dropdown__option-container";
        private static readonly string optionClass = "custom-horizontal-dropdown__option";
        private static readonly string selectedClass = "custom-horizontal-dropdown__option-selected";
        private static readonly string titleClass = "custom-horizontal-dropdown-title";

        // GUIs
        private VisualElement container;
        private VisualElement dropdownContainer;
        private Label dropDown;
        private Label titleLabel;

        private List<string> options = new();
        private List<Label> labelOptions = new();

        private Color selectedColor = new Color32(0, 0, 0, 255);
        [UxmlAttribute]
        public Color SelectedColor
        {
            get
            {
                return selectedColor;
            }
            set
            {
                if (selectedColor == value)
                {
                    return;
                }
                selectedColor = value;

                if (selected != null)
                {
                    selected.style.backgroundColor = selectedColor;
                    selected.style.color = GetTextColor(selectedColor);
                    SetBorderColor(selected);
                }
            }
        }

        private Color defaultColor = new Color32(255, 255, 255, 255);
        [UxmlAttribute]
        public Color UncheckedColor
        {
            get
            {
                return defaultColor;
            }
            set
            {
                if (defaultColor == value)
                {
                    return;
                }

                defaultColor = value;

                foreach (Label label in labelOptions)
                {
                    if (label == selected)
                    {
                        continue;
                    }

                    label.style.backgroundColor = defaultColor;
                    label.style.color = GetTextColor(defaultColor);
                    SetBorderColor(label);
                }

                dropdownContainer.style.backgroundColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.5f);
            }
        }
        private string title = "Title";
        [UxmlAttribute]
        public string Title
        {
            get => title;
            set
            {
                if (title == value)
                {
                    return;
                }

                title = value;
                titleLabel.text = title;
            }
        }

        [UxmlAttribute]
        public List<string> Options
        {
            get => options.ToList();
            set
            {
                options = value ?? new List<string>();  
                OnListChanged();
            }
        }
        private Label selected;
        public Label Selected
        {
            get => selected;
            set
            {
                if (selected == value)
                    return;

                selected = value;
                // runs styling function
                // selectedLabel = (something)
            }
        }

        public CustomDropDownOption()
        {
            AddToClassList(className);

            container = new();
            container.AddToClassList(containerClass);

            titleLabel = new()
            {
                text = "Title"
            };
            titleLabel.AddToClassList(titleClass);
            container.Add(titleLabel);

            dropdownContainer = new();
            dropdownContainer.AddToClassList(dropdownContainerClass);
            dropdownContainer.style.backgroundColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.5f);
            container.Add(dropdownContainer);

            Add(container);

            if (Options is null || !Options.Any())
            {
                return;
            }

            OnListChanged();
        }
        private void OnListChanged()
        {
            if (labelOptions.Count > 0 && labelOptions != null)
            {
                foreach (Label label in labelOptions)
                {
                    label.UnregisterCallback<ClickEvent>(OnSelect);
                }
            }
            dropdownContainer.Clear();
            labelOptions.Clear();
            foreach (string option in options) 
            {
                dropDown = new();
                dropDown.AddToClassList(optionClass);
                dropDown.text = option;
                dropDown.style.backgroundColor = defaultColor;
                dropDown.style.color = GetTextColor(defaultColor);
                SetBorderWidth(dropDown, 0);
                dropDown.RegisterCallback<ClickEvent>(OnSelect);
                labelOptions.Add(dropDown);
                dropdownContainer.Add(dropDown);
            }

            if (selected == null && labelOptions.Count > 0 && labelOptions != null)
            {
                Select(labelOptions[0]);
            }
        }

        private void Select(Label label)
        {
            foreach (Label option in labelOptions)
            {
                option.style.backgroundColor = defaultColor;
                option.style.color = GetTextColor(defaultColor);
                SetBorderWidth(option, 0);
            }

            if (label == null)
            {
                selected = labelOptions[0];
            }
            selected = label;
            selected.style.backgroundColor = selectedColor;
            selected.EnableInClassList(selectedClass, true);
            selected.style.color = GetTextColor(selectedColor);

            SetBorderWidth(selected, 2);
        }

        private void OnSelect(ClickEvent evt)
        {
            if (evt.target is Label label)
            {
                Select(label);
            }
        }

        

        private Color GetTextColor(Color32 color)
        {
            float luminance = (0.299f * color.r) + (0.587f * color.g) + (0.114f * color.b);
            if (luminance < 128f)
            {
                return new Color32(255, 255, 255, 255);
            }
            else
            {
                return new Color32(0, 0, 0, 255);
            }

        }

        private void SetBorderColor(VisualElement element)
        {
            Color borderColor = GetTextColor(element.style.backgroundColor.value);
            element.style.borderBottomColor = borderColor;
            element.style.borderLeftColor = borderColor;
            element.style.borderTopColor = borderColor;
            element.style.borderRightColor = borderColor;
        }

        private void SetBorderWidth(VisualElement element,float value)
        {
            element.style.borderLeftWidth = value;
            element.style.borderTopWidth = value;
            element.style.borderRightWidth = value;
            element.style.borderBottomWidth = value;
        }
    }
}
