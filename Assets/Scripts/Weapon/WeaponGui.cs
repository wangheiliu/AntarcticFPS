using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class WeaponGui : BaseWeaponSystem
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;
    private TextElement nameLabel;
    private TextElement ammoLabel;
    private VisualElement root;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uiDocument.rootVisualElement;
        nameLabel = root.Q<TextElement>("weapon-name");
        ammoLabel = root.Q<TextElement>("ammo-count");
    }

    // Update is called once per frame
    void Update()
    {
        if (!currentWeapon)
        {
            //root.style.display = DisplayStyle.None;
            nameLabel.text = "No Weapon";
            ammoLabel.text = "0/0";
            return;
        }
        
    
    }
}
