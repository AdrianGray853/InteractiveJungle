using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraAutoScaler : MonoBehaviour
{
    public float referenceWidth = 1080f;
    public float referenceHeight = 1920f;

    private float initialOrthoSize;
    private float targetAspect;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        initialOrthoSize = cam.orthographicSize;
        targetAspect = referenceWidth / referenceHeight;

        AdjustCamera();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying)
            AdjustCamera();
    }
#endif

    void AdjustCamera()
    {
        if (cam == null) cam = GetComponent<Camera>();

        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect >= targetAspect)
        {
            // Wider screens — keep height fixed
            cam.orthographicSize = initialOrthoSize;
        }
        else
        {
            // Taller screens — adjust height to match width
            float scaleFactor = targetAspect / currentAspect;
            cam.orthographicSize = initialOrthoSize * scaleFactor;
        }
    }
}
