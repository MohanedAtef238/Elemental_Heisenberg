using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    [System.Serializable]
    public class AlchemyReactionRecipe
    {
        public string label;
        public List<ItemDefinition> reactingVials = new List<ItemDefinition>();
        public GameObject resultVialPrefab;
        public GameObject reactionPrefab;
        public Material prepSkybox;
        public Color screenTint = Color.white;
        public AudioClip audioFeedback;

        [Header("Result Identity")]
        [Tooltip("The definition to apply to the spawned vial (sets its name and VFX).")]
        public ItemDefinition resultDefinition;

        [Tooltip("Optional override for the tooltip text. If empty, uses the Result Definition's name.")]
        public string resultTooltip;

        [Tooltip("Optional override for the vial's VFX. If empty, uses the Result Definition's effect.")]
        public GameObject resultEffectOverride;

        [Tooltip("Multiplies the intensity of the result vial's effect (default 1.0). Use to dim or boost brightness.")]
        public float resultIntensity = 1.0f;

        public bool Matches(IReadOnlyList<ItemDefinition> presentVials)
        {
            if (reactingVials == null || reactingVials.Count == 0) return false;
            if (presentVials == null || reactingVials.Count != presentVials.Count) return false;

            var remaining = new List<ItemDefinition>(reactingVials);
            foreach (var vial in presentVials)
            {
                if (vial == null) return false;
                if (!remaining.Remove(vial)) return false;
            }
            return remaining.Count == 0;
        }
    }

    /// <summary>
    /// Tracks ingredient vials placed inside the trigger zone and spawns reaction results.
    /// Does NOT own recipe data — InteractionCoordinator is the single source of truth.
    /// Fires OnIngredientsChanged whenever the zone contents change so the coordinator
    /// can re-evaluate recipe matches.
    /// </summary>
    public class AlchemyZone : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _vialsInZone = new List<GameObject>();
        [SerializeField] private List<GameObject> _textPrefabs = new List<GameObject>();
        [SerializeField] private float _reactionCooldown = 1.0f;

        private float _lastExecutionTime;
        private AudioSource _audioSource;

        /// <summary>Fires whenever vials enter or leave the zone.</summary>
        public event System.Action OnIngredientsChanged;

        /// <summary>Set externally by InteractionCoordinator when a recipe match is found.</summary>
        public bool IsPrepped { get; set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Vial") && !_vialsInZone.Contains(other.gameObject))
            {
                _vialsInZone.Add(other.gameObject);
                OnIngredientsChanged?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Vial") && _vialsInZone.Contains(other.gameObject))
            {
                _vialsInZone.Remove(other.gameObject);
                OnIngredientsChanged?.Invoke();
            }
        }

        /// <summary>Returns the ItemDefinitions of all vials currently in the zone.</summary>
        public List<ItemDefinition> GetPresentVialDefinitions()
        {
            var definitions = new List<ItemDefinition>();
            foreach (var vial in _vialsInZone)
            {
                if (vial == null) continue;
                var instance = vial.GetComponent<ItemInstance>()
                            ?? vial.GetComponentInParent<ItemInstance>();
                if (instance?.Definition != null)
                    definitions.Add(instance.Definition);
            }
            return definitions;
        }

        /// <summary>
        /// Destroys ingredient vials and spawns the result vial + VFX.
        /// Audio and skybox are handled externally by InteractionCoordinator.
        /// </summary>
        public void ExecuteSpawn(AlchemyReactionRecipe recipe)
        {
            if (recipe == null)
            {
                Debug.LogWarning("AlchemyZone: ExecuteSpawn called with null recipe.");
                return;
            }

            if (Time.time - _lastExecutionTime < _reactionCooldown)
            {
                Debug.Log("AlchemyZone: ExecuteSpawn on cooldown.");
                return;
            }

            _lastExecutionTime = Time.time;
            IsPrepped = false;

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Vector3 spawnPos = renderer != null ? renderer.bounds.center : transform.position;
            Debug.Log($"AlchemyZone: Spawning at {spawnPos} for recipe '{recipe.label}'.");

            // Destroy ingredient vials
            var toDestroy = new List<GameObject>(_vialsInZone);
            _vialsInZone.Clear();
            foreach (var vial in toDestroy)
            {
                if (vial != null)
                {
                    Debug.Log($"AlchemyZone: Destroying ingredient {vial.name}");
                    Destroy(vial);
                }
            }

            // Spawn result vial
            if (recipe.resultVialPrefab != null)
            {
                GameObject resultVial = Instantiate(
                    recipe.resultVialPrefab,
                    spawnPos + Vector3.up * 0.15f,
                    Quaternion.identity);
                
                // Apply the definition and optional VFX override + intensity
                var instance = resultVial.GetComponent<ItemInstance>() ?? resultVial.GetComponentInParent<ItemInstance>();
                if (instance != null && recipe.resultDefinition != null)
                {
                    instance.SetDefinition(recipe.resultDefinition, recipe.resultEffectOverride, recipe.resultIntensity);
                }

                // Update Tooltip text from the coordinator data
                var tooltip = resultVial.GetComponent<TooltipOnHover>() ?? resultVial.GetComponentInParent<TooltipOnHover>();
                if (tooltip != null)
                {
                    string finalLabel = !string.IsNullOrEmpty(recipe.resultTooltip) 
                        ? recipe.resultTooltip 
                        : (recipe.resultDefinition != null ? recipe.resultDefinition.DisplayName : "");
                    
                    if (!string.IsNullOrEmpty(finalLabel))
                        tooltip.SetTooltipText(finalLabel);
                }

                Debug.Log($"AlchemyZone: Spawned result vial '{resultVial.name}' with definition '{recipe.resultDefinition?.DisplayName ?? "None"}' and tooltip '{recipe.resultTooltip}'.");
            }
            else
            {
                Debug.LogWarning($"AlchemyZone: Recipe '{recipe.label}' has no result vial prefab.");
            }

            // Spawn reaction VFX
            if (recipe.reactionPrefab != null)
            {
                GameObject effect = Instantiate(recipe.reactionPrefab, spawnPos, Quaternion.identity);
                Destroy(effect, 3f);
            }

            // Spawn floating text
            if (_textPrefabs.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject randomText = _textPrefabs[Random.Range(0, _textPrefabs.Count)];
                    Vector3 offset = new Vector3(
                        Random.Range(-0.2f, 0.2f),
                        Random.Range(0.2f, 0.4f),
                        Random.Range(-0.2f, 0.2f));
                    GameObject textObj = Instantiate(
                        randomText,
                        spawnPos + offset,
                        Quaternion.Euler(0, Random.Range(0, 360), 0));
                    Destroy(textObj, 2f);
                }
            }

            Debug.Log("AlchemyZone: ExecuteSpawn complete.");
        }
    }
}
