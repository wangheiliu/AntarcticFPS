using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.GUI.Menu
{
    public class PauseMenuScript : MonoBehaviour
    {
        private const string PAUSE_ENABLED_CLASS = "pause-enabled";
        private const string PAUSE_DISABLED_CLASS = "pause-disabled";
        [Header("UI Documents")]
        [SerializeField] private UIDocument pauseDocument;
        [SerializeField] private GameManager menuDocument;

        #region UI variables
            private VisualElement btnGroup;
            private List<Button> btns;
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
            container = pauseDocument.rootVisualElement.Q<VisualElement>("container");
            btnGroup = container.Q<VisualElement>("btn-group");
            btns = btnGroup.Query<Button>().ToList();
            
            foreach (Button btn in btns)
            {
                btn.RegisterCallback<ClickEvent>(evt =>
                {
                    if (buttonStates.TryGetValue(btn.name, out MenuState state))
                    {
                        isClosing = true;
                        currentState = state;
                        EnablePauseMenu(false);
                    }
                }, CallbackOptions.Removable);
            }

            menuDocument.OnMenuStateChanged += OnPlayerPause;
            container.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        }

        void Start()
        {
            EnablePauseMenu(false);
        }

        void OnDisable()
        {
            menuDocument.OnMenuStateChanged -= OnPlayerPause;
            container.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
            btns.ForEach(btn => btn.UnregisterAllRemovableCallbacks());
            btns.Clear();
        }

        void OnTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target == container && isClosing)
            {
                isClosing = false;
                Debug.Log("Pause menu transition ended. Opening menu items for state: " + currentState);
                menuDocument.OnEventNotifierChanged(currentState);
                pauseDocument.rootVisualElement.style.display = DisplayStyle.None;
            }
        }

        public void OnPlayerPause(MenuState state) 
        {
            if (state == MenuState.Paused)
            {
                Debug.Log("Player paused the game. Opening pause menu.");
                currentState = MenuState.Paused;
                pauseDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                EnablePauseMenu(true);
            }
        }

        private void EnablePauseMenu(bool toggle)
        {
            container.EnableInClassList(PAUSE_ENABLED_CLASS, toggle);
            container.EnableInClassList(PAUSE_DISABLED_CLASS, !toggle);
        }
    }
}

