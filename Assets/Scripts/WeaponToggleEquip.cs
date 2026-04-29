using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Toggles a weapon prefab on/off on a VR controller attach point
/// when the configured Input Action fires. Designed for use on the
/// Left Controller GameObject inside the XR Origin hierarchy.
/// </summary>
public class WeaponToggleEquip : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("The Input Action Reference whose 'performed' event triggers the toggle. " +
             "Assign the XRI Left Interaction / Activate action from the XRI Default Input Actions asset.")]
    [SerializeField] private InputActionReference triggerAction;

    [Header("Weapon")]
    [Tooltip("The weapon prefab to instantiate/destroy on toggle.")]
    [SerializeField] private GameObject weaponPrefab;

    [Tooltip("The Transform that the weapon will be parented to (typically the controller's own Transform).")]
    [SerializeField] private Transform attachPoint;

    // ----- private runtime state -----
    private bool _isEquipped;
    private GameObject _spawnedWeapon;

    // -------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------

    private void OnEnable()
    {
        /*
        if (triggerAction != null)
            triggerAction.action.performed += OnTriggerPerformed;
        */
    }

    private void OnDisable()
    {
        /*
        if (triggerAction != null)
            triggerAction.action.performed -= OnTriggerPerformed;
        */
    }

    // -------------------------------------------------------
    // Toggle callback
    // -------------------------------------------------------

    private void OnTriggerPerformed(InputAction.CallbackContext ctx)
    {
        Toggle();
    }

    private void Toggle()
    {
        // Method disabled to prevent interference with alchemy system
    }

    private void EquipWeapon_DISABLED()
    {
        // Method disabled to prevent interference with alchemy system
    }

    private void UnequipWeapon()
    {
        // Method disabled to prevent interference with alchemy system
    }
}
