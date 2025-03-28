using UnityEngine;


/*
 * This script automatically adjusts the camera postion with respect to the size of the grid, making sure the view is not disrupted
 */
public class CameraHandler : MonoBehaviour
{
    private GameManager gameManager;
    private Camera mainCamera;
    float padding = 1.5f;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.OnGridInitialized += AdjustCamera;
    }

    // Unsubscribe to event OnGridInitialized
    private void OnDisable()
    {
        gameManager.OnGridInitialized -= AdjustCamera;
    }

    private void AdjustCamera(int width, int height)
    {
        // Center the camera
        mainCamera.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10f);

        // Adjust orthographic size based on aspect ratio
        float aspectRatio = (float)Screen.width / Screen.height;
        float verticalSize = height / 2f + padding;
        float horizontalSize = (width / 2f + padding) / aspectRatio;

        mainCamera.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}
