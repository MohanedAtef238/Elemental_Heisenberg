using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

namespace Interactions
{
    /// <summary>
    /// Modernized tooltip handler: uses a UI-based TextMeshPro tooltip prefab,
    /// positions it above the object, and fades it in/out on hover.
    /// </summary>
    public class TooltipOnHover : MonoBehaviour
    {
        [Tooltip("The UI prefab to use for the tooltip. Should contain a TextMeshProUGUI component.")]
        public GameObject tooltipPrefab;
        public Vector3 offset = new Vector3(0, 0.2f, 0);
        public float fadeDuration = 0.15f;
        
        [Tooltip("Text to display on the tooltip. If empty, it will try to use the ItemInstance DisplayName instead.")]
        public string tooltipText = "";

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable _interactable;
        private Camera _camera;
        private ItemInstance _itemInstance;
        private GameObject _tooltipInstance;
        private TextMeshProUGUI _textMeshPro;
        private CanvasGroup _canvasGroup;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            _interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
            _camera = Camera.main;
            _itemInstance = GetComponent<ItemInstance>();

            if (_tooltipInstance == null && tooltipPrefab != null)
            {
                _tooltipInstance = Instantiate(tooltipPrefab, transform);
                _tooltipInstance.name = $"Tooltip_{gameObject.name}";
                _tooltipInstance.transform.localPosition = offset;
                
                _textMeshPro = _tooltipInstance.GetComponentInChildren<TextMeshProUGUI>(true);
                _canvasGroup = _tooltipInstance.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = _tooltipInstance.AddComponent<CanvasGroup>();
                
                _canvasGroup.alpha = 0f;
                _tooltipInstance.SetActive(false);
            }
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
            if (_tooltipInstance != null && _tooltipInstance.activeSelf)
                PositionTooltip();
        }

        private void ShowTooltip()
        {
            if (_tooltipInstance == null) return;

            if (_textMeshPro != null)
            {
                if (!string.IsNullOrEmpty(tooltipText))
                    _textMeshPro.text = tooltipText;
                else if (_itemInstance != null && _itemInstance.Definition != null)
                    _textMeshPro.text = _itemInstance.Definition.DisplayName;
            }

            PositionTooltip();
            if (!_tooltipInstance.activeSelf) _tooltipInstance.SetActive(true);
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTo(1f));
        }

        private void HideTooltip()
        {
            if (!this.gameObject.activeInHierarchy) return;
            if (_tooltipInstance == null) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeTo(0f, deactivateOnEnd: true));
        }

        private void PositionTooltip()
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null || _tooltipInstance == null) return;

            _tooltipInstance.transform.position = transform.position + offset;
            Vector3 dir = _tooltipInstance.transform.position - _camera.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                _tooltipInstance.transform.rotation = Quaternion.LookRotation(dir);
        }

        private IEnumerator FadeTo(float targetAlpha, bool deactivateOnEnd = false)
        {
            if (_canvasGroup == null) yield break;

            float start = _canvasGroup.alpha;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(start, targetAlpha, fadeDuration <= 0f ? 1f : t / fadeDuration);
                _canvasGroup.alpha = a;
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            if (deactivateOnEnd && Mathf.Approximately(targetAlpha, 0f))
                _tooltipInstance.SetActive(false);

            _fadeCoroutine = null;
        }
    }
}

