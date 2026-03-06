using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SquareCamera : MonoBehaviour
{
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        Apply();
    }

    void OnEnable()
    {
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }

    void Update()
    {
        // In case of resize / rotation (safe but cheap)
        Apply();
    }

    void Apply()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > 1f)
        {
            // Screen is wider than tall → pillarbox
            float width = 1f / screenAspect;
            cam.rect = new Rect(
                (1f - width) / 2f,
                0f,
                width,
                1f
            );
        }
        else
        {
            // Screen is taller than wide → letterbox
            float height = screenAspect;
            cam.rect = new Rect(
                0f,
                (1f - height) / 2f,
                1f,
                height
            );
        }
    }
}