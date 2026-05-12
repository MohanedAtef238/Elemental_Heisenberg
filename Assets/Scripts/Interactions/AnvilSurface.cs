using UnityEngine;
using UnityEngine.InputSystem;

namespace Interactions
{
    /// <summary>
    /// Pure input receiver for hammer strikes on the anvil surface.
    /// Detects valid impacts via HammerVelocityTracker and delegates
    /// all reaction logic to InteractionCoordinator.
    /// </summary>
    public class AnvilSurface : MonoBehaviour
    {
        [SerializeField] private InteractionCoordinator _coordinator;

        private void Awake()
        {
            if (_coordinator == null)
            {
                _coordinator = Object.FindAnyObjectByType<InteractionCoordinator>();
            }
        }

        [Tooltip("Optional debug action that force-triggers alchemy without an anvil hit.")]
        [SerializeField] private InputActionReference debugTriggerAction;

        [SerializeField] private float requiredImpactForce = 3f;
        [SerializeField] private float strikeRepeatCooldown = 0.2f;

        private InputAction _resolvedDebugAction;
        private float _lastStrikeTime;

        [ContextMenu("Force Trigger Reaction")]
        public void ForceTrigger()
        {
            if (_coordinator != null)
            {
                Debug.Log("AnvilSurface: ContextMenu Force Trigger fired!");
                _coordinator.ForceTriggerReaction();
            }
        }

        private void OnEnable()
        {
            if (debugTriggerAction != null)
            {
                _resolvedDebugAction = debugTriggerAction.action;
                _resolvedDebugAction.performed += OnDebugTrigger;
                if (!_resolvedDebugAction.enabled)
                    _resolvedDebugAction.Enable();
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
            if (_coordinator != null)
            {
                Debug.Log("AnvilSurface: Debug controller trigger fired!");
                _coordinator.ForceTriggerReaction();
            }
        }

        private void Update()
        {
            // Editor fallback: press O to force-trigger
            if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
            {
                Debug.Log("AnvilSurface: Keyboard 'O' pressed - Forcing reaction!");
                if (_coordinator != null) _coordinator.ForceTriggerReaction();
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
            if (_coordinator == null)
            {
                Debug.LogWarning("AnvilSurface: InteractionCoordinator is not assigned!");
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
            Debug.Log($"AnvilSurface: Valid strike from {strikeObject.name} on {phase} at speed {impactForce:F2}. Notifying coordinator.");
            _coordinator.TryExecuteReaction();
        }
    }
}
