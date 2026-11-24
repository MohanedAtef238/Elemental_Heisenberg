using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Interactions
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestHUDController : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private InputActionReference _toggleAction;

        private UIDocument _uiDocument;
        private Label _titleLabel;
        private Label _descLabel;
        private Label _targetLabel;
        private Label _hintLabel;
        
        private VisualElement _root;
        private bool _isVisible = false;
        private InputAction _manualAction;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_uiDocument != null && _uiDocument.rootVisualElement != null)
            {
                _root = _uiDocument.rootVisualElement;
                _titleLabel = _root.Q<Label>("quest-title");
                _descLabel = _root.Q<Label>("quest-desc");
                _targetLabel = _root.Q<Label>("quest-target");
                _hintLabel = _root.Q<Label>("quest-hint");
            }

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestChanged += RefreshDisplay;
                RefreshDisplay(QuestManager.Instance.CurrentQuest);
            }

            if (_toggleAction != null && _toggleAction.action != null)
            {
                _toggleAction.action.Enable();
                _toggleAction.action.performed += OnToggleAction;
            }
            else
            {
                _manualAction = new InputAction(type: InputActionType.Button, binding: "<XRController>{LeftHand}/primaryButton");
                _manualAction.Enable();
                _manualAction.performed += OnToggleAction;
            }
            
            UpdateVisibility();
        }

        private void Start()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestChanged -= RefreshDisplay;
                QuestManager.Instance.OnQuestChanged += RefreshDisplay;
                RefreshDisplay(QuestManager.Instance.CurrentQuest);
            }
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestChanged -= RefreshDisplay;

            if (_toggleAction != null && _toggleAction.action != null)
            {
                _toggleAction.action.performed -= OnToggleAction;
            }
            
            if (_manualAction != null)
            {
                _manualAction.performed -= OnToggleAction;
                _manualAction.Disable();
            }
        }

        private void OnToggleAction(InputAction.CallbackContext context)
        {
            _isVisible = !_isVisible;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (_root != null)
            {
                _root.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            var colliders = GetComponents<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = _isVisible;
            }

            var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (interactable != null)
                interactable.enabled = _isVisible;
        }

        private void RefreshDisplay(QuestDefinition quest)
        {
            if (_root == null) return;

            if (quest != null)
            {
                if (_titleLabel != null) _titleLabel.text = quest.questTitle;
                if (_descLabel != null) _descLabel.text = quest.questDescription;
                if (_hintLabel != null) _hintLabel.text = "Hint: " + quest.hint;
                if (_targetLabel != null) _targetLabel.text = "Objective: " + (quest.targetItem != null ? quest.targetItem.DisplayName : "???");
            }
            else
            {
                if (_titleLabel != null) _titleLabel.text = "Quests Completed!";
                if (_descLabel != null) _descLabel.text = "Master Alchemist Achievement Unlocked.";
                if (_hintLabel != null) _hintLabel.text = "";
                if (_targetLabel != null) _targetLabel.text = "";
            }
        }
    }
}
