using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// Tracks world-space velocity manually to ensure reliable impact detection
    /// regardless of Rigidbody kinematic settings or collision modes.
    /// </summary>
    public class HammerVelocityTracker : MonoBehaviour
    {
        [Header("Status")]
        public float CurrentImpactSpeed;

        private Vector3 _lastPosition;

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0) return;

            Vector3 currentPosition = transform.position;
            CurrentImpactSpeed = (currentPosition - _lastPosition).magnitude / deltaTime;
            _lastPosition = currentPosition;
        }
    }
}
