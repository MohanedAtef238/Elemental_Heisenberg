using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Place on a scene object to identify it as an interactable item.
    /// References an ItemDefinition asset so the coordinator can match collisions.
    /// Fires OnCollidedWith when this object physically contacts another ItemInstance.
    /// </summary>
    public class ItemInstance : MonoBehaviour
    {
        [SerializeField] private ItemDefinition _definition;

        public ItemDefinition Definition => _definition;

        public event System.Action<ItemInstance, ItemInstance> OnCollidedWith;

        private void OnCollisionEnter(Collision collision)
        {
            var other = collision.gameObject.GetComponent<ItemInstance>();
            if (other != null)
                OnCollidedWith?.Invoke(this, other);
        }
    }
}
