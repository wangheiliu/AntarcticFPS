using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 25f;
    public Transform playerBody;
    [SerializeField] private Transform characterObj;

    private float xRotation = 0f;
    private float mouseX;
    private float mouseY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;
    }

    void LateUpdate()
    {
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}