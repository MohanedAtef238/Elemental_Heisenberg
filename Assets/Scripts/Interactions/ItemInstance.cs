using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Place on a scene object to identify it as an interactable item.
    /// References an ItemDefinition asset so the coordinator can match collisions.
    /// Fires OnCollidedWith ONLY when this object physically contacts another ItemInstance
    /// that is also tagged "Vial". This prevents proximity/shelf overlaps from
    /// triggering combination logic prematurely.
    /// </summary>
    public class ItemInstance : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _definition;

        public ItemDefinition Definition => _definition;

        private void Awake()
        {
            // Force Instantaneous movement so objects stay locked to the hand during WASD movement
            var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab != null)
            {
                grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Instantaneous;
            }
        }

        public void SetDefinition(ItemDefinition definition, GameObject vfxOverride = null, float intensityMultiplier = 1.0f)
        {
            _definition = definition;
            
            // Refresh visuals via the VFX container if present
            var container = GetComponent<VialVFXContainer>();
            if (container != null && _definition != null)
            {
                // Use the override if provided, otherwise fallback to the definition's effect
                GameObject effectToUse = vfxOverride != null ? vfxOverride : _definition.EffectPrefab;
                
                if (effectToUse != null)
                    container.InjectEffect(effectToUse, intensityMultiplier);
            }
        }


        // private void OnCollisionEnter(Collision collision)
        // {
        //     // Only consider collisions with objects also tagged "Vial" —
        //     // this prevents shelf colliders or the environment from accidentally
        //     // triggering combination logic.
        //     if (!collision.gameObject.CompareTag("Vial"))
        //         return;
        //
        //     var other = collision.gameObject.GetComponent<ItemInstance>();
        //     if (other != null)
        //         OnCollidedWith?.Invoke(this, other);
        // }
    }
}
