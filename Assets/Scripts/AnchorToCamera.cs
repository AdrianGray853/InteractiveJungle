using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorToCamera : MonoBehaviour
{
    public enum AnchorSide { Left, Right, Top, Bottom, Center }
    public AnchorSide anchorSide;
    private Camera mainCamera;
    private float xOffset;
    private float yOffset;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found. Please make sure there's a camera tagged as MainCamera in the scene.");
            return;
        }

        Vector3 cameraPos = mainCamera.transform.position;
        float cameraWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float cameraHeight = mainCamera.orthographicSize;

        // Calculate offset based on the initial position of the object
        switch (anchorSide)
        {
            case AnchorSide.Left:
                xOffset = transform.position.x - (cameraPos.x - cameraWidth);
                break;
            case AnchorSide.Right:
                xOffset = (cameraPos.x + cameraWidth) - transform.position.x;
                break;
            case AnchorSide.Top:
                yOffset = (cameraPos.y + cameraHeight) - transform.position.y;
                break;
            case AnchorSide.Bottom:
                yOffset = transform.position.y - (cameraPos.y - cameraHeight);
                break;
            case AnchorSide.Center:
                xOffset = transform.position.x - cameraPos.x;
                yOffset = transform.position.y - cameraPos.y;
                break;
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        Vector3 newPos = transform.position;
        Vector3 cameraPos = mainCamera.transform.position;
        float cameraWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float cameraHeight = mainCamera.orthographicSize;

        // Adjust position based on camera movement while keeping the initial offset
        switch (anchorSide)
        {
            case AnchorSide.Left:
                newPos.x = cameraPos.x - cameraWidth + xOffset;
                break;
            case AnchorSide.Right:
                newPos.x = cameraPos.x + cameraWidth - xOffset;
                break;
            case AnchorSide.Top:
                newPos.y = cameraPos.y + cameraHeight - yOffset;
                break;
            case AnchorSide.Bottom:
                newPos.y = cameraPos.y - cameraHeight + yOffset;
                break;
            case AnchorSide.Center:
                newPos.x = cameraPos.x + xOffset;
                newPos.y = cameraPos.y + yOffset;
                break;
        }

        transform.position = newPos;
    }
}
