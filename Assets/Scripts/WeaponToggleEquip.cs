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
    private InputAction _resolvedTriggerAction;

    // -------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------

    private void OnEnable()
    {
        _resolvedTriggerAction = triggerAction != null
            ? triggerAction.action
            : InputSystem.actions?.FindAction("XRI Left Interaction/Activate");

        if (_resolvedTriggerAction != null)
        {
            _resolvedTriggerAction.performed += OnTriggerPerformed;
            if (!_resolvedTriggerAction.enabled)
            {
                _resolvedTriggerAction.Enable();
            }
        }
        else
        {
            Debug.LogWarning("WeaponToggleEquip: No trigger action assigned or found for 'XRI Left Interaction/Activate'.");
        }
    }

    private void OnDisable()
    {
        if (_resolvedTriggerAction != null)
        {
            _resolvedTriggerAction.performed -= OnTriggerPerformed;
            _resolvedTriggerAction = null;
        }
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
        if (_isEquipped)
        {
            UnequipWeapon();
        }
        else
        {
            EquipWeapon();
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    private void EquipWeapon()
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning("WeaponToggleEquip: No weapon prefab assigned.");
            return;
        }

        Transform targetAttachPoint = attachPoint != null ? attachPoint : transform;
        _spawnedWeapon = Instantiate(weaponPrefab, targetAttachPoint);
        _spawnedWeapon.transform.localPosition = Vector3.zero;
        _spawnedWeapon.transform.localRotation = Quaternion.identity;
        _isEquipped = true;
    }

    private void UnequipWeapon()
    {
        if (_spawnedWeapon != null)
        {
            Destroy(_spawnedWeapon);
        }

        _spawnedWeapon = null;
        _isEquipped = false;
    }
}
