using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
[RequireComponent(typeof(BoxCollider))]
public class WorldSpaceMenuFollower : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;
    [SerializeField] private float distanceFromCamera = 5f;
    [SerializeField] private Vector3 localOffset = new(0f, -0.08f, 0f);
    [SerializeField] private Vector3 rotationOffsetEuler = new(0f, 180f, 0f);
    [SerializeField] private float followLerpSpeed = 12f;
    [SerializeField] private float rotationLerpSpeed = 12f;
    [SerializeField] private float colliderDepth = 0.03f;

    private UIDocument _document;
    private BoxCollider _boxCollider;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        EnsureTargetCamera();
        SyncWorldSpaceCollider();
        SnapToCamera();
    }

    private void LateUpdate()
    {
        if (!EnsureTargetCamera())
        {
            return;
        }

        var anchorPosition = targetCamera.position
            + (targetCamera.forward * distanceFromCamera)
            + targetCamera.TransformVector(localOffset);

        transform.position = Vector3.Lerp(
            transform.position,
            anchorPosition,
            1f - Mathf.Exp(-followLerpSpeed * Time.unscaledDeltaTime));

        var lookDirection = targetCamera.position - transform.position;
        if (lookDirection.sqrMagnitude <= 0.0001f)
        {
            lookDirection = -targetCamera.forward;
        }

        var targetRotation = Quaternion.LookRotation(lookDirection.normalized, targetCamera.up)
            * Quaternion.Euler(rotationOffsetEuler);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            1f - Mathf.Exp(-rotationLerpSpeed * Time.unscaledDeltaTime));
    }

    private bool EnsureTargetCamera()
    {
        if (targetCamera != null)
        {
            return true;
        }

        if (Camera.main != null)
        {
            targetCamera = Camera.main.transform;
        }

        return targetCamera != null;
    }

    private void SnapToCamera()
    {
        if (!EnsureTargetCamera())
        {
            return;
        }

        var anchorPosition = targetCamera.position
            + (targetCamera.forward * distanceFromCamera)
            + targetCamera.TransformVector(localOffset);

        transform.position = anchorPosition;
        transform.rotation = Quaternion.LookRotation((targetCamera.position - transform.position).normalized, targetCamera.up)
            * Quaternion.Euler(rotationOffsetEuler);
    }

    private void SyncWorldSpaceCollider()
    {
        if (_document == null || _boxCollider == null)
        {
            return;
        }

        var pixelsPerUnit = 100f;
        if (_document.panelSettings != null)
        {
            pixelsPerUnit = Mathf.Max(0.0001f, _document.panelSettings.referenceSpritePixelsPerUnit);
        }

        var worldSpaceWidth = ReadDocumentFloat(_document, "worldSpaceWidth", "m_WorldSpaceWidth", 793.2f);
        var worldSpaceHeight = ReadDocumentFloat(_document, "worldSpaceHeight", "m_WorldSpaceHeight", 384.9f);
        var width = Mathf.Max(0.01f, worldSpaceWidth / pixelsPerUnit);
        var height = Mathf.Max(0.01f, worldSpaceHeight / pixelsPerUnit);
        var depth = Mathf.Max(0.005f, colliderDepth);

        _boxCollider.size = new Vector3(width, height, depth);
        _boxCollider.center = Vector3.zero;

        AssignDocumentCollider(_document, _boxCollider);
    }

    private static float ReadDocumentFloat(UIDocument document, string propertyName, string fieldName, float fallback)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var property = typeof(UIDocument).GetProperty(propertyName, flags);
        if (property != null && property.CanRead && property.PropertyType == typeof(float))
        {
            return (float)property.GetValue(document);
        }

        var field = typeof(UIDocument).GetField(fieldName, flags);
        if (field != null && field.FieldType == typeof(float))
        {
            return (float)field.GetValue(document);
        }

        return fallback;
    }

    private static void AssignDocumentCollider(UIDocument document, BoxCollider collider)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        var property = typeof(UIDocument).GetProperty("worldSpaceCollider", flags);
        if (property != null && property.CanWrite && typeof(Collider).IsAssignableFrom(property.PropertyType))
        {
            property.SetValue(document, collider);
            return;
        }

        var field = typeof(UIDocument).GetField("m_WorldSpaceCollider", flags);
        if (field != null && typeof(Collider).IsAssignableFrom(field.FieldType))
        {
            field.SetValue(document, collider);
        }
    }
}
