using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    [System.Serializable]
    public class InteractionResult
    {
        [Header("Skybox")]
        public bool changeSky;
        public Material skyboxMaterial;

        [Header("Screen Tint")]
        public bool applyScreenTint;
        public Color screenTintColor = Color.white;

        [Header("Sound")]
        public AudioClip soundEffect;

        [Header("Weather Effect")]
        [Tooltip("Optional prefab spawned at the collision midpoint")]
        public GameObject weatherEffectPrefab;
    }

    [System.Serializable]
    public class Interaction
    {
        public string label;
        public ItemDefinition itemA;
        public ItemDefinition itemB;
        public InteractionResult result;

        public bool Matches(ItemDefinition a, ItemDefinition b)
        {
            return (itemA == a && itemB == b) || (itemA == b && itemB == a);
        }
    }

    /// <summary>
    /// Central interaction controller.
    /// Attach to a single manager object and configure interactions in the Inspector.
    /// Auto-discovers all ItemInstance objects in the scene at Start.
    ///
    /// To add a new interaction:
    ///   1. Create ItemDefinition assets for each element
    ///   2. Add an Interaction entry pairing the two items
    ///   3. Configure the result (skybox, tint, sound, weather)
    /// </summary>
    public class InteractionCoordinator : MonoBehaviour
    {
        [SerializeField] private List<Interaction> _interactions = new();
        [SerializeField] private ScreenTintEffect _screenTint;

        private readonly List<ItemInstance> _items = new();
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            foreach (var item in FindObjectsByType<ItemInstance>(FindObjectsSortMode.None))
            {
                _items.Add(item);
                item.OnCollidedWith += HandleCollision;
            }
        }

        private void OnDestroy()
        {
            foreach (var item in _items)
            {
                if (item != null)
                    item.OnCollidedWith -= HandleCollision;
            }
        }

        private void HandleCollision(ItemInstance a, ItemInstance b)
        {
            if (!a.gameObject.activeSelf || !b.gameObject.activeSelf)
                return;

            foreach (var interaction in _interactions)
            {
                if (!interaction.Matches(a.Definition, b.Definition))
                    continue;

                a.gameObject.SetActive(false);
                b.gameObject.SetActive(false);
                ApplyResult(interaction.result, a.gameObject, b.gameObject);
                return;
            }
        }

        private void ApplyResult(InteractionResult result, GameObject objA, GameObject objB)
        {
            if (result.changeSky && result.skyboxMaterial != null)
            {
                RenderSettings.skybox = new Material(result.skyboxMaterial);
                DynamicGI.UpdateEnvironment();
            }

            if (result.applyScreenTint && _screenTint != null)
            {
                _screenTint.SetTint(result.screenTintColor);
            }

            if (result.soundEffect != null)
            {
                _audioSource.PlayOneShot(result.soundEffect);
            }

            if (result.weatherEffectPrefab != null)
            {
                var midpoint = (objA.transform.position + objB.transform.position) * 0.5f;
                Instantiate(result.weatherEffectPrefab, midpoint, Quaternion.identity);
            }
        }
    }
}
