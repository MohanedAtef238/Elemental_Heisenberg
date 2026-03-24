using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Attach to any GameObject to give it a label the InteractionCoordinator can match against.
    /// Raises OnCollidedWithTagged when this object physically collides with another tagged object.
    /// </summary>
    public class InteractableTag : MonoBehaviour
    {
        [SerializeField] private string _label;
        public string Label => _label;

        /// <summary>Fired when this object's collider touches another InteractableTag's collider.</summary>
        public event System.Action<InteractableTag, InteractableTag> OnCollidedWithTagged;

        private void OnCollisionEnter(Collision collision)
        {
            var other = collision.gameObject.GetComponent<InteractableTag>();
            if (other != null)
                OnCollidedWithTagged?.Invoke(this, other);
        }
    }
}
