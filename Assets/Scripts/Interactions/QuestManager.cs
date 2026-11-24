using UnityEngine;

namespace Interactions
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public System.Collections.Generic.List<QuestDefinition> availableQuests = new System.Collections.Generic.List<QuestDefinition>();
        [SerializeField] private int _currentQuestIndex = 0;

        private System.Collections.Generic.HashSet<string> _discoveredRecipeIds = new System.Collections.Generic.HashSet<string>();

        public event System.Action<QuestDefinition> OnQuestChanged;
        public event System.Action<QuestDefinition> OnQuestCompleted;
        public event System.Action OnRecipesUpdated;

        public QuestDefinition CurrentQuest => (availableQuests != null && _currentQuestIndex < availableQuests.Count) ? availableQuests[_currentQuestIndex] : null;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            UpdateStatus();
        }

        private void OnValidate()
        {
            if (Application.isPlaying && Instance == this)
            {
                UpdateStatus();
            }
        }

        public void CheckCompletion(ItemDefinition result)
        {
            if (result == null) return;

            // Track discovery regardless of quest
            if (!_discoveredRecipeIds.Contains(result.name))
            {
                _discoveredRecipeIds.Add(result.name);
                OnRecipesUpdated?.Invoke();
            }

            if (CurrentQuest == null) return;
            if (result == CurrentQuest.targetItem) CompleteCurrentQuest();
        }

        public bool IsRecipeDiscovered(string definitionName) => _discoveredRecipeIds.Contains(definitionName);

        private void CompleteCurrentQuest()
        {
            QuestDefinition completed = CurrentQuest;
            if (completed.completionWeatherPrefab != null)
            {
                var atmosphere = FindFirstObjectByType<AtmosphereController>();
                if (atmosphere != null) atmosphere.SetWeather(completed.completionWeatherPrefab);
            }
            OnQuestCompleted?.Invoke(completed);
            _currentQuestIndex++;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            OnQuestChanged?.Invoke(CurrentQuest);
        }
    }
}
