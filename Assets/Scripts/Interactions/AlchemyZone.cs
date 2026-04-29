using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    public class AlchemyZone : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _vialsInZone = new List<GameObject>();
        [SerializeField] private Material _prepSkybox;
        [SerializeField] private GameObject _resultVialPrefab;
        [SerializeField] private GameObject _reactionPrefab;
        [SerializeField] private List<GameObject> _textPrefabs = new List<GameObject>();
        [SerializeField] private float _reactionCooldown = 1.0f;

        private Material _defaultSkybox;
        private float _lastExecutionTime;
        public bool IsPrepped { get; private set; }

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
            if (_vialsInZone.Count >= 2)
            {
                SetPrepped(true);
            }
            else
            {
                SetPrepped(false);
            }
        }

        private void SetPrepped(bool prepped)
        {
            if (IsPrepped == prepped) return;
            
            IsPrepped = prepped;
            if (IsPrepped)
            {
                if (_prepSkybox != null)
                {
                    RenderSettings.skybox = _prepSkybox;
                    DynamicGI.UpdateEnvironment();
                }
            }
            else
            {
                RenderSettings.skybox = _defaultSkybox;
                DynamicGI.UpdateEnvironment();
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

            SetPrepped(false);

            if (_resultVialPrefab != null)
            {
                // Spawn slightly higher to avoid clipping with the paper's collider
                GameObject resultVial = Instantiate(_resultVialPrefab, spawnPos + Vector3.up * 0.15f, Quaternion.identity);
                if (resultVial != null)
                {
                    Debug.Log($"AlchemyZone: Successfully instantiated result vial: {resultVial.name}");
                    resultVial.name = "Result_HighTier_Vial";
                }
                else
                {
                    Debug.LogError("AlchemyZone: Failed to instantiate result vial prefab!");
                }
            }
            else
            {
                Debug.LogError("AlchemyZone: _resultVialPrefab is NULL!");
            }

            if (_reactionPrefab != null)
            {
                GameObject effect = Instantiate(_reactionPrefab, spawnPos, Quaternion.identity);
                Debug.Log($"AlchemyZone: Instantiated reaction effect {effect.name}");
                Destroy(effect, 3f); // Reduced to 3s to avoid lingering "endless" feel
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
    }
}
