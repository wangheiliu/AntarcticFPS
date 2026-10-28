using System.Reflection;
using Player.PlayerData;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ProfileDisplayScript : MonoBehaviour
{
    [SerializeField] private PanelRenderer document;
    private VisualElement root;
    private Label usernameLabel;
    private VisualElement textContainer;
    void OnEnable()
    {
        document.RegisterUIReloadCallback(OnGUIRefresh);
        
    }

    private void OnGUIRefresh(PanelRenderer renderer, VisualElement _root, int currentVersion)
    {
        root = _root;
        usernameLabel = root.Q<Label>("username-label");
        textContainer = root.Q<VisualElement>("text-container");
        GeneralPlayerDataManager.OnUsernameChanged += DisplayStats;
        DisplayStats();
    }

    void Start()
    {
        
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
                if (displayName.Location != "ProfileDisplay")
                {
                    continue;
                }
                Label statLabel = new Label();
                statLabel.AddToClassList("stat-display-text");
                statLabel.text = $"{displayName.DisplayName}: {prop.GetValue(null)}"; // static field values use null
                statLabel.style.display = DisplayStyle.Flex;
                textContainer.Add(statLabel);
                
            }
        }
    }

    void OnDisable()
    {
        GeneralPlayerDataManager.OnUsernameChanged -= DisplayStats;
        document.UnregisterUIReloadCallback(OnGUIRefresh);
    }
}
