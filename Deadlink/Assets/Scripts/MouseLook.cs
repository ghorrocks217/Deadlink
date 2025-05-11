using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 100f;
    public float clampAngle = 90f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Mouse.current == null) return;

        Vector2 mouseDelta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * sensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
