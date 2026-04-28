using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class WorldSpaceMenuFollower : MonoBehaviour
{
    [SerializeField] private Transform targetCamera;
    [SerializeField] private float distanceFromCamera = 0.9f;
    [SerializeField] private Vector3 localOffset = new(0f, -0.08f, 0f);
    [SerializeField] private float followLerpSpeed = 12f;
    [SerializeField] private float rotationLerpSpeed = 12f;

    private void OnEnable()
    {
        EnsureTargetCamera();
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

        var targetRotation = Quaternion.LookRotation(lookDirection.normalized, targetCamera.up);
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
        transform.rotation = Quaternion.LookRotation((targetCamera.position - transform.position).normalized, targetCamera.up);
    }
}
