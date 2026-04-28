using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Interactions
{
    /// <summary>
    /// Minimal tooltip handler: creates a simple 3D TextMesh tooltip if none assigned,
    /// positions it above the object, and fades it in/out on hover.
    /// </summary>
    public class TooltipOnHover : MonoBehaviour
    {
        public GameObject tooltip;
        public Vector3 offset = new Vector3(0, 0.2f, 0);
        public float fadeDuration = 0.15f;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable _interactable;
        private Camera _camera;
        private ItemInstance _itemInstance;
        private TextMesh _textMesh;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
            _camera = Camera.main;
            _itemInstance = GetComponent<ItemInstance>();

            if (tooltip == null)
                CreateDefaultTooltip();

            if (tooltip != null)
                _textMesh = tooltip.GetComponentInChildren<TextMesh>(true);
        }

        private void OnEnable()
        {
            if (_interactable != null)
            {
                _interactable.hoverEntered.AddListener(OnHoverEnter);
                _interactable.hoverExited.AddListener(OnHoverExit);
            }
        }

        private void OnDisable()
        {
            if (_interactable != null)
            {
                _interactable.hoverEntered.RemoveListener(OnHoverEnter);
                _interactable.hoverExited.RemoveListener(OnHoverExit);
            }
        }

        private void OnHoverEnter(HoverEnterEventArgs args) => ShowTooltip();
        private void OnHoverExit(HoverExitEventArgs args) => HideTooltip();
        private void OnMouseEnter() => ShowTooltip();
        private void OnMouseExit() => HideTooltip();

        private void LateUpdate()
        {
            if (tooltip != null && tooltip.activeSelf)
                PositionTooltip();
        }

        private void ShowTooltip()
        {
            if (tooltip == null) return;

            if (_itemInstance != null && _itemInstance.Definition != null && _textMesh != null)
                _textMesh.text = _itemInstance.Definition.DisplayName;

            PositionTooltip();
            if (!tooltip.activeSelf) tooltip.SetActive(true);
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTo(1f));
        }

        private void HideTooltip()
        {
            if (tooltip == null) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTo(0f, deactivateOnEnd: true));
        }

        private void PositionTooltip()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null || tooltip == null) return;

            tooltip.transform.position = transform.position + offset;
            Vector3 dir = tooltip.transform.position - _camera.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                tooltip.transform.rotation = Quaternion.LookRotation(dir);
        }

        private void CreateDefaultTooltip()
        {
            tooltip = new GameObject($"Tooltip_{gameObject.name}");
            tooltip.transform.SetParent(transform, false);
            tooltip.transform.localPosition = offset;

            var tm = tooltip.AddComponent<TextMesh>();
            tm.text = _itemInstance != null && _itemInstance.Definition != null ? _itemInstance.Definition.DisplayName : gameObject.name;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.02f;
            tm.fontSize = 128;
            tm.color = new Color(1f, 1f, 1f, 0f);

            tooltip.SetActive(false);
            _textMesh = tm;
        }

        private IEnumerator FadeTo(float targetAlpha, bool deactivateOnEnd = false)
        {
            if (_textMesh == null) yield break;

            float start = _textMesh.color.a;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(start, targetAlpha, fadeDuration <= 0f ? 1f : t / fadeDuration);
                var c = _textMesh.color; c.a = a; _textMesh.color = c;
                yield return null;
            }

            var endCol = _textMesh.color; endCol.a = targetAlpha; _textMesh.color = endCol;
            if (deactivateOnEnd && Mathf.Approximately(targetAlpha, 0f))
                tooltip.SetActive(false);

            _fadeCoroutine = null;
        }
    }
}
