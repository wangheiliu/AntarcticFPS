using UnityEngine.UIElements;

namespace Game.GUI.AttributeElements
{
    [UxmlElement]
    public partial class FilterAttributes : VisualElement
    {
        [UxmlAttribute] public string key { get; set; }

        public void Display()
        {

        } // don't add to this unless if you want to intiialize something

        public FilterAttributes()
        {
            
        }
    }
}
// this script creates the custom "keys" element for a lookup function in ShopFilter

