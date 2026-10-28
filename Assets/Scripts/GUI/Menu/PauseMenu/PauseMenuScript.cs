using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.GUI.Menu
{
    public class PauseMenuScript : MonoBehaviour
    {
        private const string PAUSE_ENABLED_CLASS = "pause-enabled";
        private const string PAUSE_DISABLED_CLASS = "pause-disabled";
        [Header("UI Documents")]
        [SerializeField] private PanelRenderer pauseDocument;
        [SerializeField] private GameManager menuDocument;

        #region UI variables
        private VisualElement root;
        private VisualElement btnGroup;
        private List<Button> btns = new();
        private VisualElement container;
        #endregion
        private MenuState currentState;
        private bool isClosing = false;
        
        private readonly Dictionary<string, MenuState> buttonStates = new Dictionary<string, MenuState>()
        {
            {"continue-btn", MenuState.Playing},
            {"settings-btn", MenuState.Settings},
            {"exit-btn", MenuState.MainMenu}
        };
        void OnEnable()
        {
            pauseDocument.RegisterUIReloadCallback(OnGUIRefresh);
        }

        private void OnGUIRefresh(PanelRenderer renderer, VisualElement _root, int currentVersion)
        {
            root = _root;
            Debug.Log(root.name);
            container = root.Q<VisualElement>("container");
            btnGroup = root.Q<VisualElement>("btn-group");
            btns = btnGroup.Query<Button>().ToList();

            Debug.Log(btns.Count);
            
            foreach (Button btn in btns)
            {
                Debug.Log("Registered button");
                btn.RegisterCallback<ClickEvent>(evt =>
                {
                    if (buttonStates.TryGetValue(btn.name, out MenuState state))
                    {
                        Debug.Log("Clicked!");
                        isClosing = true;
                        currentState = state;
                        if (currentState == MenuState.MainMenu)
                        {
                            menuDocument.OnPauseEventNotifier(false);
                        }
                        EnablePauseMenu(false);
                    } else
                    {
                        Debug.LogWarning("You clicked the wrong type fool!");
                    }
                }, CallbackOptions.Removable);
            }

            menuDocument.OnMenuStateChanged += OnPlayerPause;
            container.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            bool isPaused = menuDocument.playerState == MenuState.Paused;
            root.style.display = isPaused ? DisplayStyle.Flex : DisplayStyle.None;
            EnablePauseMenu(isPaused);
        }

        void Start()
        {
            
        }

        void OnDisable()
        {
            menuDocument.OnMenuStateChanged -= OnPlayerPause;
            container?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            btns?.ForEach(btn => btn.UnregisterAllRemovableCallbacks());
            btns?.Clear();
            pauseDocument.UnregisterUIReloadCallback(OnGUIRefresh);
        }

        void OnTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target == container && isClosing)
            {
                isClosing = false;
                Debug.Log("Pause menu transition ended. Opening menu items for state: " + currentState);
                menuDocument.OnEventNotifierChanged(currentState);
                root.style.display = DisplayStyle.None;
            }
        }

        public void OnPlayerPause(MenuState state) 
        {
            if (state == MenuState.Paused)
            {
                Debug.Log("Player paused the game. Opening pause menu.");
                currentState = MenuState.Paused;
                root.style.display = DisplayStyle.Flex;
                EnablePauseMenu(true);

                menuDocument.OnPauseEventNotifier(true);
            }
        }

        private void EnablePauseMenu(bool toggle)
        {
            container?.EnableInClassList(PAUSE_ENABLED_CLASS, toggle);
            container?.EnableInClassList(PAUSE_DISABLED_CLASS, !toggle);
        }
    }
}
