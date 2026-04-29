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

        private Material _defaultSkybox;
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

        public void ExecuteReaction()
        {
            if (!IsPrepped) return;

            Vector3 spawnPos = transform.position;

            foreach (var vial in _vialsInZone)
            {
                if (vial != null) Destroy(vial);
            }
            _vialsInZone.Clear();

            SetPrepped(false);

            if (_resultVialPrefab != null)
            {
                Instantiate(_resultVialPrefab, spawnPos + Vector3.up * 0.05f, Quaternion.identity);
            }

            if (_reactionPrefab != null)
            {
                Instantiate(_reactionPrefab, spawnPos, Quaternion.identity);
            }

            if (_textPrefabs.Count > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject randomText = _textPrefabs[Random.Range(0, _textPrefabs.Count)];
                    Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.3f), Random.Range(-0.2f, 0.2f));
                    Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    Instantiate(randomText, spawnPos + offset, rot);
                }
            }
            
            Debug.Log("Alchemy reaction executed!");
        }
    }
}
