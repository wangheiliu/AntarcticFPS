using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;
using Game.ScriptableObjects.Settings;

namespace BasicUIControls
{
    [UxmlElement]
    public partial class BaseSettingElement : VisualElement, ISettingAttributes
    {
        private string title = "Title";
        public DataBinding titleBinding;
        [UxmlAttribute, CreateProperty]
        public string Title
        {
            get => title;
            set
            {
                if (title == value) return;
                title = value;
                UpdateVisuals();
                titleBinding?.MarkDirty();
            }
        }

        private string dataName = "";
        public DataBinding dataNameBinding;
        [UxmlAttribute, CreateProperty]
        public string DataName
        {
            get => dataName;
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Debug.LogWarning("DataName cannot be null or empty.");
                    return;
                }
                if (dataName == value) return;
                dataName = value;
                UpdateVisuals();
                dataNameBinding?.MarkDirty();
            }
        }
        protected void InitBaseDataBinding()
        {
            dataNameBinding = new DataBinding()
            {
                dataSourceType = typeof(BasicSettingsObject),
                dataSourcePath = new(nameof(BasicSettingsObject.DataName)),
                bindingMode = BindingMode.ToTarget
            };

            titleBinding = new DataBinding()
            {
                dataSourceType = typeof(BasicSettingsObject),
                dataSourcePath = new(nameof(BasicSettingsObject.Title)),
                bindingMode = BindingMode.ToTarget
            };
        }
        protected virtual void OnDetachFromPanel(DetachFromPanelEvent evt) { }
        protected virtual void OnAttachToPanel(AttachToPanelEvent evt) { }
        protected virtual void UpdateVisuals() { }
    }
}

