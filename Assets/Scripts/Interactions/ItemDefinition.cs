using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Defines an item type as a reusable asset.
    /// Assign an ID, display name, and optional VFX prefabs for the effect and trail.
    /// Create via: Assets > Create > Interactions > Item Definition
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Interactions/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string _itemId;
        [SerializeField] private string _displayName;

        [Header("Visuals")]
        [Tooltip("Particle effect prefab (e.g. aura, glow)")]
        [SerializeField] private GameObject _effectPrefab;
        [Tooltip("Trail effect prefab (e.g. Vefects trail)")]
        [SerializeField] private GameObject _trailPrefab;

        public string ItemId => _itemId;
        public string DisplayName => _displayName;
        public GameObject EffectPrefab => _effectPrefab;
        public GameObject TrailPrefab => _trailPrefab;
    }
}
