using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    private Camera mainCamera;
    public float zoomSpeed = 1f;
    public float minSize = 50f;
    public float maxSize = 100f;
    private float targetSize;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        targetSize = mainCamera.orthographicSize;
        if (targetSize < minSize) targetSize = minSize;
        mainCamera.orthographicSize = targetSize;
    }

    void Update()
    {
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * 10f);
    }

    public void Zoom(float scrollInput)
    {
        if (scrollInput != 0)
        {
            targetSize -= scrollInput * zoomSpeed * 100f;
            targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
        }
    }
}