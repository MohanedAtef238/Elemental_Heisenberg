using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Interactions
{
    public class AlchemyRecipeBoard : MonoBehaviour
    {
        [Header("Document")]
        [SerializeField] private PanelSettings _panelSettings;
        [SerializeField] private VisualTreeAsset _documentAsset;
        [SerializeField] private string _title = "Known Recipes";

        [Header("Data Source")]
        [Tooltip("The InteractionCoordinator that owns the recipe list.")]
        [SerializeField] private InteractionCoordinator _coordinator;

        [Header("Placement")]
        [SerializeField] private Vector3 _boardOffset = new Vector3(0.9f, 0.75f, 0f);
        [SerializeField] private Vector3 _boardRotationEuler = new Vector3(12f, -90f, 0f);
        [SerializeField] private float _worldSpaceWidth = 780f;
        [SerializeField] private float _worldSpaceHeight = 860f;

        private AlchemyZone _alchemyZone;
        public UIDocument _boardDocument;
        public GameObject _boardObject;

        private void Awake()
        {
            // Optional alchemy zone link if needed in the future, but no longer required
        }

        private void Start()
        {
            if (_coordinator == null)
            {
                Debug.LogWarning("AlchemyRecipeBoard: InteractionCoordinator is not assigned — board will be empty.");
            }

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnRecipesUpdated += PopulateBoard;
            }

            if (_panelSettings == null || _documentAsset == null)
            {
                Debug.LogWarning("AlchemyRecipeBoard: PanelSettings or document asset is missing.");
                return;
            }

            CreateBoard();
            StartCoroutine(PopulateBoardNextFrame());
        }

        private void OnDisable()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnRecipesUpdated -= PopulateBoard;
            }

            if (_boardObject != null)
            {
                Destroy(_boardObject);
                _boardObject = null;
                _boardDocument = null;
            }
        }

        private void CreateBoard()
        {
            if (TryGetComponent<UIDocument>(out _boardDocument))
            {
                // We are attached directly to the board object
                _boardObject = gameObject;
                return;
            }

            if (_boardObject != null)
                return;

            _boardObject = new GameObject("AlchemyRecipeBoard_Dynamic");
            _boardObject.transform.SetPositionAndRotation(
                transform.position + (transform.rotation * _boardOffset),
                transform.rotation * Quaternion.Euler(_boardRotationEuler));

            _boardDocument = _boardObject.AddComponent<UIDocument>();
            SetDocumentObject(_boardDocument, "panelSettings", "m_PanelSettings", _panelSettings);
            SetDocumentObject(_boardDocument, "visualTreeAsset", "sourceAsset", _documentAsset);
            SetDocumentValue(_boardDocument, "sortingOrder", "m_SortingOrder", 0);
            SetDocumentValue(_boardDocument, "position", "m_Position", 0);
            SetDocumentValue(_boardDocument, "worldSpaceSizeMode", "m_WorldSpaceSizeMode", 1);
            SetDocumentValue(_boardDocument, "worldSpaceWidth", "m_WorldSpaceWidth", _worldSpaceWidth);
            SetDocumentValue(_boardDocument, "worldSpaceHeight", "m_WorldSpaceHeight", _worldSpaceHeight);
            SetDocumentValue(_boardDocument, "pivotReferenceSize", "m_PivotReferenceSize", 0);
            SetDocumentValue(_boardDocument, "pivot", "m_Pivot", 0);
        }

        private IEnumerator PopulateBoardNextFrame()
        {
            for (int i = 0; i < 5; i++)
            {
                if (_boardDocument != null && _boardDocument.rootVisualElement != null)
                {
                    PopulateBoard();
                    yield break;
                }

                yield return null;
            }

            Debug.LogWarning("AlchemyRecipeBoard: UIDocument root was not ready for recipe population.");
        }

        private void PopulateBoard()
        {
            VisualElement root = _boardDocument.rootVisualElement;
            if (root == null)
                return;

            Label titleLabel = root.Q<Label>("recipe-title");
            if (titleLabel != null)
            {
                titleLabel.text = _title;
            }

            ScrollView listRoot = root.Q<ScrollView>("recipe-list");
            if (listRoot == null)
            {
                Debug.LogWarning("AlchemyRecipeBoard: Could not find 'recipe-list' in the document.");
                return;
            }

            // Get the content container from the ScrollView
            VisualElement contentContainer = listRoot.contentContainer;

            contentContainer.Clear();

            foreach (AlchemyReactionRecipe recipe in (_coordinator != null ? _coordinator.Recipes : System.Array.Empty<AlchemyReactionRecipe>()))
            {
                if (recipe == null)
                    continue;

                // Secret recipes only show up once discovered
                bool isDiscovered = QuestManager.Instance != null && QuestManager.Instance.IsRecipeDiscovered(recipe.resultDefinition?.name ?? recipe.label);
                
                if (recipe.isSecret && !isDiscovered)
                    continue;

                VisualElement row = new VisualElement();
                row.AddToClassList("recipe-row");

                if (!string.IsNullOrWhiteSpace(recipe.label))
                {
                    Label label = new Label(recipe.label);
                    label.AddToClassList("recipe-name");
                    row.Add(label);
                }

                Label formula = new Label(BuildRecipeFormula(recipe));
                formula.AddToClassList("recipe-formula");
                row.Add(formula);

                contentContainer.Add(row);
            }
        }

        private static string BuildRecipeFormula(AlchemyReactionRecipe recipe)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < recipe.reactingVials.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(" + ");
                }

                builder.Append(GetDefinitionName(recipe.reactingVials[i]));
            }

            builder.Append(" -> ");
            if (recipe.resultDefinition != null)
                builder.Append(GetDefinitionName(recipe.resultDefinition));
            else
                builder.Append(GetResultName(recipe.resultVialPrefab));
            return builder.ToString();
        }

        private static string GetDefinitionName(ItemDefinition definition)
        {
            if (definition == null)
                return "Unknown";

            if (!string.IsNullOrWhiteSpace(definition.DisplayName))
                return definition.DisplayName;

            return definition.name.Replace('_', ' ');
        }

        private static string GetResultName(GameObject resultPrefab)
        {
            if (resultPrefab == null)
                return "Unknown";

            ItemInstance instance = resultPrefab.GetComponent<ItemInstance>();
            if (instance == null)
            {
                instance = resultPrefab.GetComponentInParent<ItemInstance>();
            }

            if (instance?.Definition != null)
            {
                return GetDefinitionName(instance.Definition);
            }

            return resultPrefab.name.Replace('_', ' ');
        }

        private static void SetDocumentObject(UIDocument document, string propertyName, string fieldName, Object value)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            PropertyInfo property = typeof(UIDocument).GetProperty(propertyName, flags);
            if (property != null && property.CanWrite && property.PropertyType.IsAssignableFrom(value.GetType()))
            {
                property.SetValue(document, value);
                return;
            }

            FieldInfo field = typeof(UIDocument).GetField(fieldName, flags);
            if (field != null && field.FieldType.IsAssignableFrom(value.GetType()))
            {
                field.SetValue(document, value);
            }
        }

        private static void SetDocumentValue(UIDocument document, string propertyName, string fieldName, object value)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            PropertyInfo property = typeof(UIDocument).GetProperty(propertyName, flags);
            if (property != null && property.CanWrite)
            {
                property.SetValue(document, CoerceValue(property.PropertyType, value));
                return;
            }

            FieldInfo field = typeof(UIDocument).GetField(fieldName, flags);
            if (field != null)
            {
                field.SetValue(document, CoerceValue(field.FieldType, value));
            }
        }

        private static object CoerceValue(System.Type targetType, object value)
        {
            if (targetType.IsEnum)
            {
                return System.Enum.ToObject(targetType, value);
            }

            if (targetType == typeof(float))
            {
                return System.Convert.ToSingle(value);
            }

            if (targetType == typeof(int))
            {
                return System.Convert.ToInt32(value);
            }

            return value;
        }
    }
}
