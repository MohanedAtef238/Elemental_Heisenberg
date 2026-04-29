using UnityEngine;
using UnityEngine.InputSystem;

namespace Interactions
{
    public class AnvilSurface : MonoBehaviour
    {
        [SerializeField] private AlchemyZone _alchemyZone;
        [Tooltip("Optional debug action that force-triggers alchemy without an anvil hit.")]
        [SerializeField] private InputActionReference debugTriggerAction;
        [SerializeField] private float requiredImpactForce = 3f;
        [SerializeField] private float strikeRepeatCooldown = 0.2f;
        private InputAction _resolvedDebugAction;
        private float _lastStrikeTime;

        [ContextMenu("Force Trigger Reaction")]
        public void ForceTrigger()
        {
            if (_alchemyZone != null)
            {
                Debug.Log("AnvilSurface: ContextMenu Force Trigger fired!");
                _alchemyZone.ExecuteReaction(true);
            }
        }

        private void OnEnable()
        {
            if (debugTriggerAction != null)
            {
                _resolvedDebugAction = debugTriggerAction.action;
                _resolvedDebugAction.performed += OnDebugTrigger;
                if (!_resolvedDebugAction.enabled)
                {
                    _resolvedDebugAction.Enable();
                }
            }
        }

        private void OnDisable()
        {
            if (_resolvedDebugAction != null)
            {
                _resolvedDebugAction.performed -= OnDebugTrigger;
                _resolvedDebugAction = null;
            }
        }

        private void OnDebugTrigger(InputAction.CallbackContext ctx)
        {
            if (_alchemyZone != null)
            {
                Debug.Log("AnvilSurface: Debug controller trigger fired!");
                _alchemyZone.ExecuteReaction(true);
            }
        }

        private void Update()
        {
            // Fallback for editor testing if the controller action isn't working
            if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
            {
                Debug.Log("AnvilSurface: Keyboard 'O' pressed - Forcing reaction!");
                if (_alchemyZone != null) _alchemyZone.ExecuteReaction(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryProcessStrike(other, "enter");
        }

        private void OnTriggerStay(Collider other)
        {
            TryProcessStrike(other, "stay");
        }

        private void TryProcessStrike(Collider other, string phase)
        {
            if (_alchemyZone == null)
            {
                Debug.LogWarning("AnvilSurface: AlchemyZone is not assigned!");
                return;
            }

            if (Time.time - _lastStrikeTime < strikeRepeatCooldown)
                return;

            HammerVelocityTracker tracker = other.GetComponentInParent<HammerVelocityTracker>();
            GameObject strikeObject = tracker != null
                ? tracker.gameObject
                : other.attachedRigidbody != null
                    ? other.attachedRigidbody.gameObject
                    : other.gameObject;

            bool looksLikeWeapon = strikeObject.CompareTag("Weapon") || strikeObject.name.Contains("Elven_Hammer");
            if (!looksLikeWeapon)
                return;

            if (tracker == null)
            {
                Debug.LogWarning($"AnvilSurface: Weapon {strikeObject.name} touched on {phase}, but no HammerVelocityTracker was found.");
                return;
            }

            float impactForce = tracker.CurrentImpactSpeed;
            if (impactForce < requiredImpactForce)
            {
                Debug.Log($"AnvilSurface: {strikeObject.name} {phase} speed {impactForce:F2} below required {requiredImpactForce:F2}.");
                return;
            }

            _lastStrikeTime = Time.time;
            if (_alchemyZone.IsPrepped)
            {
                Debug.Log($"AnvilSurface: Valid strike from {strikeObject.name} on {phase} at speed {impactForce:F2}.");
                _alchemyZone.ExecuteReaction();
            }
            else
            {
                Debug.Log($"AnvilSurface: Valid strike from {strikeObject.name}, but no valid alchemy recipe is currently prepped.");
            }
        }
    }
}
