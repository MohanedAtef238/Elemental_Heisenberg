using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Base class for all interaction effects.
    /// Subclass this ScriptableObject to define what happens when a rule is matched.
    /// Examples: change skybox color, spawn a prefab, play audio, unlock a door, etc.
    /// </summary>
    public abstract class InteractionEffect : ScriptableObject
    {
        /// <summary>Called when a matching pair of labelled objects are both selected.</summary>
        public abstract void Apply(GameObject objectA, GameObject objectB);

        /// <summary>Called when the pair is broken (one object released). Override to clean up.</summary>
        public virtual void Revert() { }
    }
}
