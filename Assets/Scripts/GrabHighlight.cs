using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Shows a highlight material on the mesh when an XR interactor hovers over this object,
/// giving the player a visual cue that the object can be grabbed.
/// Attach alongside XRGrabInteractable. Requires a MeshRenderer on the same GameObject.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(MeshRenderer))]
public class GrabHighlight : MonoBehaviour
{
    [SerializeField] private Material _highlightMaterial;

    private XRGrabInteractable _interactable;
    private MeshRenderer _renderer;
    private Material _originalMaterial;

    private void Awake()
    {
        _interactable = GetComponent<XRGrabInteractable>();
        _renderer = GetComponent<MeshRenderer>();
        _originalMaterial = _renderer.sharedMaterial;
    }

    private void OnEnable()
    {
        _interactable.hoverEntered.AddListener(OnHoverEnter);
        _interactable.hoverExited.AddListener(OnHoverExit);
    }

    private void OnDisable()
    {
        _interactable.hoverEntered.RemoveListener(OnHoverEnter);
        _interactable.hoverExited.RemoveListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        _renderer.enabled = true;
        if (_highlightMaterial != null)
            _renderer.material = _highlightMaterial;
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        // Only hide if no longer hovered by anyone
        if (!_interactable.isHovered)
        {
            _renderer.enabled = false;
            _renderer.material = _originalMaterial;
        }
    }
}
