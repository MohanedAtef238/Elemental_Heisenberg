using UnityEngine;
using UnityEngine.UIElements;

namespace Interactions
{
    public class QuestBoard : MonoBehaviour
    {
        [SerializeField] private PanelSettings _panelSettings;
        [SerializeField] private VisualTreeAsset _documentAsset;

        [Header("Placement")]
        [SerializeField] private Vector3 _boardOffset = new Vector3(-0.9f, 0.75f, 0f);
        [SerializeField] private Vector3 _boardRotationEuler = new Vector3(12f, 90f, 0f);
        [SerializeField] private float _worldSpaceWidth = 600f;
        [SerializeField] private float _worldSpaceHeight = 800f;

        private UIDocument _boardDocument;
        private GameObject _boardObject;

        private void Start()
        {
            if (_panelSettings == null || _documentAsset == null) return;

            SetupQuestDisplay();
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestChanged += RefreshDisplay;
                RefreshDisplay(QuestManager.Instance.CurrentQuest);
            }
        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestChanged -= RefreshDisplay;
        }

        private void SetupQuestDisplay()
        {
            _boardObject = new GameObject("QuestBoard");
            _boardObject.transform.SetPositionAndRotation(
                transform.position + (transform.rotation * _boardOffset),
                transform.rotation * Quaternion.Euler(_boardRotationEuler));

            _boardDocument = _boardObject.AddComponent<UIDocument>();
            SetDocValue(_boardDocument, "panelSettings", "m_PanelSettings", _panelSettings);
            SetDocValue(_boardDocument, "visualTreeAsset", "sourceAsset", _documentAsset);
            SetDocValue(_boardDocument, "worldSpaceSizeMode", "m_WorldSpaceSizeMode", 1);
            SetDocValue(_boardDocument, "worldSpaceWidth", "m_WorldSpaceWidth", _worldSpaceWidth);
            SetDocValue(_boardDocument, "worldSpaceHeight", "m_WorldSpaceHeight", _worldSpaceHeight);
        }

        private void RefreshDisplay(QuestDefinition quest)
        {
            if (_boardDocument == null || _boardDocument.rootVisualElement == null) return;

            var root = _boardDocument.rootVisualElement;
            var title = root.Q<Label>("quest-title");
            var desc = root.Q<Label>("quest-desc");
            var hint = root.Q<Label>("quest-hint");
            var target = root.Q<Label>("quest-target");

            if (quest != null)
            {
                if (title != null) title.text = quest.questTitle;
                if (desc != null) desc.text = quest.questDescription;
                if (hint != null) hint.text = "Hint: " + quest.hint;
                if (target != null) target.text = "Objective: " + (quest.targetItem != null ? quest.targetItem.DisplayName : "???");
            }
            else
            {
                if (title != null) title.text = "Quests Completed!";
                if (desc != null) desc.text = "Master Alchemist Achievement Unlocked.";
                if (hint != null) hint.text = "";
                if (target != null) target.text = "";
            }
        }

        private void SetDocValue(UIDocument document, string propertyName, string fieldName, object value)
        {
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var prop = typeof(UIDocument).GetProperty(propertyName, flags);
            if (prop != null && prop.CanWrite) { prop.SetValue(document, value); return; }
            var field = typeof(UIDocument).GetField(fieldName, flags);
            if (field != null) field.SetValue(document, value);
        }
    }
}
