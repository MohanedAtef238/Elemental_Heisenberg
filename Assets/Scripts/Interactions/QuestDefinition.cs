using UnityEngine;

namespace Interactions
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Alchemy/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        public string questTitle;
        [TextArea(3, 10)]
        public string questDescription;
        [TextArea(2, 5)]
        public string hint;
        public ItemDefinition targetItem;
        public GameObject completionWeatherPrefab;
    }
}
