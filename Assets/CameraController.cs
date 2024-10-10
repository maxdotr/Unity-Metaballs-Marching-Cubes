using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public CinemachineFreeLook freeLookCamera;
    public float rotationSpeed = 2.0f;
    public float zoomSpeed = 2.0f;
    public float minZoom = 5.0f;
    public float maxZoom = 20.0f;

    public bool isRotating = false;

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            isRotating = true;
        } else {
            isRotating = false;
        }

        HandleCameraRotation();
        HandleCameraZoom();
    }

    private void HandleCameraRotation()
    {
        if (isRotating)
        {
            freeLookCamera.m_YAxis.m_MaxSpeed = 2;
            freeLookCamera.m_XAxis.m_MaxSpeed = 300;
        } else {
            freeLookCamera.m_YAxis.m_MaxSpeed = 0;
            freeLookCamera.m_XAxis.m_MaxSpeed = 0;
        }
    }

    private void HandleCameraZoom()
    {
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(zoomInput) > 0.01f)
        {
            float newZoom = freeLookCamera.m_Orbits[1].m_Radius - zoomInput * zoomSpeed;
            float newZoomEndOrbits = newZoom * 0.33f;
            freeLookCamera.m_Orbits[1].m_Radius = Mathf.Clamp(newZoom, minZoom, maxZoom);
            freeLookCamera.m_Orbits[0].m_Radius = Mathf.Clamp(newZoomEndOrbits, minZoom * 0.33f, maxZoom * 0.33f);
            freeLookCamera.m_Orbits[2].m_Radius = Mathf.Clamp(newZoomEndOrbits, minZoom * 0.33f, maxZoom * 0.33f);
        }
    }
}
