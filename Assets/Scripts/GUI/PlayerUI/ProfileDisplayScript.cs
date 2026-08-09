using System.Reflection;
using Player.PlayerData;
using UnityEngine;
using UnityEngine.UIElements;

public class ProfileDisplayScript : MonoBehaviour
{
    [SerializeField] private UIDocument document;
    private Label usernameLabel;
    private VisualElement textContainer;
    void OnEnable()
    {
        usernameLabel = document.rootVisualElement.Q<Label>("username-label");
        textContainer = document.rootVisualElement.Q<VisualElement>("text-container");
        GeneralPlayerDataManager.OnUsernameChanged += DisplayStats;
    }

    void Start()
    {
        DisplayStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DisplayStats()
    {
        usernameLabel.text = GeneralPlayerDataManager.Username;

        PropertyInfo[] properties = typeof(ProgressionDataManager).GetProperties(BindingFlags.Public | BindingFlags.Static);

        foreach (var prop in properties)
        {
            var displayName = prop.GetCustomAttribute<DataStatDisplay>();
            if (displayName != null)
            {
                Label statLabel = new Label();
                statLabel.AddToClassList("stat-display-text");
                statLabel.text = $"{displayName.DisplayName}: {prop.GetValue(null)}"; // static field values use null
                Debug.Log(statLabel.text);
                statLabel.style.display = DisplayStyle.Flex;
                textContainer.Add(statLabel);
                
            }
        }
    }

    void OnDisable()
    {
        GeneralPlayerDataManager.OnUsernameChanged -= DisplayStats;
    }
}
