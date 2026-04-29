using System.Collections.Generic;
using System.Linq;
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
        public AudioClip audioFeedback;

        public bool Matches(IReadOnlyList<ItemDefinition> presentVials)
        {
            if (reactingVials == null || reactingVials.Count == 0)
                return false;

            if (presentVials == null || reactingVials.Count != presentVials.Count)
                return false;

            var remaining = new List<ItemDefinition>(reactingVials);
            foreach (var vial in presentVials)
            {
                if (vial == null)
                    return false;

                if (!remaining.Remove(vial))
                    return false;
            }

            return remaining.Count == 0;
        }
    }

    public class AlchemyZone : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _vialsInZone = new List<GameObject>();
        [SerializeField] private List<AlchemyReactionRecipe> _reactionRecipes = new List<AlchemyReactionRecipe>();
        [SerializeField] private List<GameObject> _textPrefabs = new List<GameObject>();
        [SerializeField] private float _reactionCooldown = 1.0f;

        private Material _defaultSkybox;
        private Material _activePrepSkybox;
        private float _lastExecutionTime;
        private AlchemyReactionRecipe _currentRecipe;
        private AudioSource _audioSource;
        public bool IsPrepped { get; private set; }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Start()
        {
            _defaultSkybox = RenderSettings.skybox;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Vial"))
            {
                if (!_vialsInZone.Contains(other.gameObject))
                {
                    _vialsInZone.Add(other.gameObject);
                    EvaluateCurrentIngredients();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Vial"))
            {
                if (_vialsInZone.Contains(other.gameObject))
                {
                    _vialsInZone.Remove(other.gameObject);
                    EvaluateCurrentIngredients();
                }
            }
        }

        public void EvaluateCurrentIngredients()
        {
            _currentRecipe = FindMatchingRecipe();
            if (_currentRecipe != null)
            {
                SetPrepped(true, _currentRecipe.prepSkybox);
            }
            else
            {
                SetPrepped(false);
            }
        }

        private void SetPrepped(bool prepped, Material prepSkybox = null, bool restoreDefaultSkybox = true)
        {
            if (IsPrepped == prepped && _activePrepSkybox == prepSkybox) return;
            
            IsPrepped = prepped;
            _activePrepSkybox = prepSkybox;
            if (IsPrepped)
            {
                if (prepSkybox != null)
                {
                    RenderSettings.skybox = prepSkybox;
                    DynamicGI.UpdateEnvironment();
                }
                else
                {
                    RenderSettings.skybox = _defaultSkybox;
                    DynamicGI.UpdateEnvironment();
                }
            }
            else
            {
                if (restoreDefaultSkybox)
                {
                    RenderSettings.skybox = _defaultSkybox;
                    DynamicGI.UpdateEnvironment();
                }
            }
        }

        public void ExecuteReaction(bool force = false)
        {
            if (Time.time - _lastExecutionTime < _reactionCooldown) return;
            
            Debug.Log($"AlchemyZone: ExecuteReaction called (force: {force}, IsPrepped: {IsPrepped}, Vials: {_vialsInZone.Count})");
            if (!IsPrepped && !force) 
            {
                Debug.LogWarning("AlchemyZone: Reaction aborted - Not prepped and not forced.");
                return;
            }

            AlchemyReactionRecipe recipeToExecute = _currentRecipe ?? FindMatchingRecipe();
            if (recipeToExecute == null)
            {
                Debug.LogWarning("AlchemyZone: Reaction aborted - No matching recipe for the current vials.");
                return;
            }

            _lastExecutionTime = Time.time;

            // Use MeshRenderer bounds to find the true center of the paper
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            Vector3 spawnPos = renderer != null ? renderer.bounds.center : transform.position;
            Debug.Log($"AlchemyZone: Spawning reaction at {spawnPos} (Renderer center: {renderer != null})");

            // Copy list to avoid modification during destruction, though we clear it anyway
            List<GameObject> toDestroy = new List<GameObject>(_vialsInZone);
            _vialsInZone.Clear();

            foreach (var vial in toDestroy)
            {
                if (vial != null) 
                {
                    Debug.Log($"AlchemyZone: Destroying ingredient {vial.name}");
                    Destroy(vial);
                }
            }

            Material reactionSkybox = recipeToExecute.prepSkybox;
            SetPrepped(false, restoreDefaultSkybox: false);
            _currentRecipe = null;

            if (reactionSkybox != null)
            {
                RenderSettings.skybox = reactionSkybox;
                DynamicGI.UpdateEnvironment();
            }

            if (recipeToExecute.resultVialPrefab != null)
            {
                // Spawn slightly higher to avoid clipping with the paper's collider
                GameObject resultVial = Instantiate(recipeToExecute.resultVialPrefab, spawnPos + Vector3.up * 0.15f, Quaternion.identity);
                if (resultVial != null)
                {
                    Debug.Log($"AlchemyZone: Successfully instantiated result vial: {resultVial.name}");
                }
                else
                {
                    Debug.LogError("AlchemyZone: Failed to instantiate result vial prefab!");
                }
            }
            else
            {
                Debug.LogWarning($"AlchemyZone: Recipe '{recipeToExecute.label}' has no result vial prefab assigned.");
            }

            if (recipeToExecute.reactionPrefab != null)
            {
                GameObject effect = Instantiate(recipeToExecute.reactionPrefab, spawnPos, Quaternion.identity);
                Debug.Log($"AlchemyZone: Instantiated reaction effect {effect.name}");
                Destroy(effect, 3f); // Reduced to 3s to avoid lingering "endless" feel
            }

            if (recipeToExecute.audioFeedback != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(recipeToExecute.audioFeedback);
            }

            if (_textPrefabs.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject randomText = _textPrefabs[Random.Range(0, _textPrefabs.Count)];
                    Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.2f, 0.4f), Random.Range(-0.2f, 0.2f));
                    Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    GameObject textObj = Instantiate(randomText, spawnPos + offset, rot);
                    Destroy(textObj, 2f); // Cleanup text after 2 seconds
                }
            }
            
            Debug.Log("Alchemy reaction execution complete!");
        }

        private AlchemyReactionRecipe FindMatchingRecipe()
        {
            if (_reactionRecipes == null || _reactionRecipes.Count == 0)
                return null;

            List<ItemDefinition> presentDefinitions = GetPresentVialDefinitions();
            if (presentDefinitions.Count == 0)
                return null;

            return _reactionRecipes.FirstOrDefault(recipe => recipe != null && recipe.Matches(presentDefinitions));
        }

        private List<ItemDefinition> GetPresentVialDefinitions()
        {
            List<ItemDefinition> definitions = new List<ItemDefinition>();

            foreach (GameObject vial in _vialsInZone)
            {
                if (vial == null)
                    continue;

                ItemInstance instance = vial.GetComponent<ItemInstance>();
                if (instance == null)
                {
                    instance = vial.GetComponentInParent<ItemInstance>();
                }

                if (instance?.Definition != null)
                {
                    definitions.Add(instance.Definition);
                }
            }

            return definitions;
        }
    }
}
