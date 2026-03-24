using System.Collections.Generic;
using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Listens for physical collisions between tagged objects in the scene.
    /// When two tagged objects collide and their labels match a rule, both objects
    /// are deactivated and the rule's effect is applied.
    ///
    /// To add a new interaction type:
    ///   1. Create a new class that extends InteractionEffect
    ///   2. Create a new SO asset of that type (right-click > Create > Interactions > Effects)
    ///   3. Create a new InteractionRule SO and assign labelA, labelB, and the effect asset
    ///   4. Add the rule to this component's Rules list in the Inspector
    /// </summary>
    public class InteractionCoordinator : MonoBehaviour
    {
        [SerializeField] private List<InteractionRule> _rules = new();

        private readonly List<InteractableTag> _tags = new();

        private void Start()
        {
            foreach (var tag in FindObjectsByType<InteractableTag>(FindObjectsSortMode.None))
            {
                _tags.Add(tag);
                tag.OnCollidedWithTagged += HandleCollision;
            }
        }

        private void OnDestroy()
        {
            foreach (var tag in _tags)
            {
                if (tag != null)
                    tag.OnCollidedWithTagged -= HandleCollision;
            }
        }

        private void HandleCollision(InteractableTag a, InteractableTag b)
        {
            // Dedup: both collision partners fire the event — skip if already consumed
            if (!a.gameObject.activeSelf || !b.gameObject.activeSelf)
                return;

            foreach (var rule in _rules)
            {
                if (!rule.Matches(a.Label, b.Label)) continue;

                a.gameObject.SetActive(false);
                b.gameObject.SetActive(false);
                rule.Effect.Apply(a.gameObject, b.gameObject);
                return;
            }
        }

    }
}
