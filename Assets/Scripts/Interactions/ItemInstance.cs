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

        public void SetDefinition(ItemDefinition definition)
        {
            _definition = definition;
            // Optionally refresh visuals if needed
        }

        public event System.Action<ItemInstance, ItemInstance> OnCollidedWith;

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
