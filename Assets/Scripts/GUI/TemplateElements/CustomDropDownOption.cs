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
        private static readonly string optionClass = "custom-horizontal-dropdown__option";
        private static readonly string selectedClass = "custom-horizontal-dropdown__option-selected";
        private VisualElement container;
        private Label dropDown;

        private ObservableCollection<string> options = new();
        private List<Label> buttonOptions = new();
        [UxmlAttribute]
        public List<string> Options
        {
            get => options.ToList();
            set
            {
                options.Clear();
                if (value != null)
                {
                    foreach (string option in value)
                    {
                        options.Add(option);
                    }

                    OnListChanged();
                }

            }
        }
        private string selected;
        private Label selectedLabel;
        [UxmlAttribute]
        public string Selected
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


            if (Options is null || !Options.Any())
            {
                return;
            }

            OnListChanged();
            Add(container);
        }
        private void OnListChanged()
        {
            container.Clear();
            buttonOptions.Clear();
            foreach (string option in options)
            {
                dropDown = new();
                dropDown.AddToClassList(optionClass);
                dropDown.name = option;
                buttonOptions.Add(dropDown);
                container.Add(dropDown);
            }
        }
    }
}
