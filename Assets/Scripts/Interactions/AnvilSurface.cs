using UnityEngine;
using UnityEngine.InputSystem;

namespace Interactions
{
    public class AnvilSurface : MonoBehaviour
    {
        [SerializeField] private AlchemyZone _alchemyZone;
        [SerializeField] private InputActionReference debugTriggerAction;
        [SerializeField] private float requiredImpactForce = 3f;

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
                debugTriggerAction.action.performed += OnDebugTrigger;
            }
            else
            {
                // Robust fallback: Try to find the Activate action on the left hand directly
                // Correct path from XRI Default Input Actions: "XRI Left Interaction/Activate"
                var activateAction = InputSystem.actions.FindAction("XRI Left Interaction/Activate");
                if (activateAction != null)
                {
                    activateAction.performed += OnDebugTrigger;
                    Debug.Log("AnvilSurface: Successfully hooked 'XRI Left Interaction/Activate' fallback.");
                }
                else
                {
                    Debug.LogWarning("AnvilSurface: debugTriggerAction is null AND 'XRI Left Interaction/Activate' not found. VR Trigger won't work.");
                }
            }
        }

        private void OnDisable()
        {
            if (debugTriggerAction != null)
                debugTriggerAction.action.performed -= OnDebugTrigger;
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
            if (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame)
            {
                Debug.Log("AnvilSurface: Keyboard 'L' pressed - Forcing reaction!");
                if (_alchemyZone != null) _alchemyZone.ExecuteReaction(true);
            }
        }

        /*
        private void OnTriggerEnter(Collider other)
        {
            // Verify the impacting object: Weapon tag or Elven_Hammer name
            if (other.CompareTag("Weapon") || other.name.Contains("Elven_Hammer"))
            {
                // Use the velocity tracker for manual speed detection
                var tracker = other.GetComponent<HammerVelocityTracker>();
                if (tracker == null) tracker = other.GetComponentInParent<HammerVelocityTracker>();

                if (tracker != null)
                {
                    float impactForce = tracker.CurrentImpactSpeed;
                    
                    if (impactForce >= requiredImpactForce)
                    {
                        if (_alchemyZone != null && _alchemyZone.IsPrepped)
                        {
                            _alchemyZone.ExecuteReaction();
                        }
                        else if (_alchemyZone == null)
                        {
                            Debug.LogWarning("AnvilSurface: AlchemyZone is not assigned!");
                        }
                    }
                }
            }
        }
        */
    }
}
