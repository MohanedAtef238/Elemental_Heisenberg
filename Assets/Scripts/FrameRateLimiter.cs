using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class FrameRateLimiter : MonoBehaviour
{
    [SerializeField] private int targetFrameRate = 60;

    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
    }
}
