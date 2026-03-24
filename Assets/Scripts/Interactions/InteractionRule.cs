using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// A rule asset that pairs two object labels with an effect.
    /// Create via: Assets > Create > Interactions > Rule
    /// Rules are order-independent: (A, B) matches the same as (B, A).
    /// </summary>
    [CreateAssetMenu(fileName = "InteractionRule", menuName = "Interactions/Rule")]
    public class InteractionRule : ScriptableObject
    {
        [SerializeField] private string _labelA;
        [SerializeField] private string _labelB;
        [SerializeField] private InteractionEffect _effect;

        public InteractionEffect Effect => _effect;

        /// <summary>Returns true if this rule covers the given label pair, regardless of order.</summary>
        public bool Matches(string labelA, string labelB)
        {
            return (_labelA == labelA && _labelB == labelB)
                || (_labelA == labelB && _labelB == labelA);
        }
    }
}
