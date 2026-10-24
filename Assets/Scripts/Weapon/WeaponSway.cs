using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private CharacterController charController;
    [SerializeField] private float swayAmount = 5f;
    [SerializeField] private float maxAngleSway = 5f;

    GameObject playerObj;
    private Vector3 initialPosition;
    private Vector3 currentSway;
    private Quaternion initialRotation;
    private Quaternion lastRotation;
    private Quaternion lastCamRotation;

    void Start()
    {
        playerObj = charController.gameObject;
        initialPosition = transform.localPosition;
        initialRotation = playerCam.transform.localRotation;
        lastRotation = initialRotation;
        lastCamRotation = playerCam.transform.localRotation;
    }

    // Update is called once per frame
    protected virtual void LateUpdate()
    {
        FaceWeaponOrientation();
        SwayWeaponPosition();
    }

    protected virtual void FaceWeaponOrientation()
    {
        if (playerCam == null) // if no camera in a tag is enabled, returns nullreferenceexception
        {
            return;
        }

        if (!playerCam.enabled)
        {
            transform.rotation = Quaternion.identity;
        }

        Quaternion camRotation = playerCam.transform.rotation;
        if (camRotation.eulerAngles == lastCamRotation.eulerAngles)
        {
            currentSway = Vector3.Lerp(currentSway, Vector3.zero, 1f - Mathf.Exp(-0.1f * Time.deltaTime));
            Quaternion target = Quaternion.Euler(currentSway) * initialRotation;
            transform.localRotation = target;
            return;
        }
        Quaternion rotationDelta = Quaternion.Inverse(lastRotation) * camRotation;

        Vector3 deltaAngles = rotationDelta.eulerAngles;

        float deltaAngleX = Mathf.DeltaAngle(0f, deltaAngles.x) * swayAmount;
        float deltaAngleY = Mathf.DeltaAngle(0f, deltaAngles.y) * swayAmount;

        Vector3 targetSway = new(
            Mathf.Clamp(deltaAngleX, -maxAngleSway, maxAngleSway),
            Mathf.Clamp(-deltaAngleY, -maxAngleSway, maxAngleSway),
            Mathf.Clamp(-deltaAngleY, -maxAngleSway, maxAngleSway)
        );

        currentSway = Vector3.Lerp(currentSway, targetSway, 1f - Mathf.Exp(-0.95f * Time.deltaTime));

        Quaternion targetRotation = Quaternion.Euler(currentSway) * initialRotation;

        transform.localRotation = targetRotation;
        lastRotation = targetRotation;
        lastCamRotation = playerCam.transform.rotation;
    }

    protected void SwayWeaponPosition()
    {
        Vector3 newPosition = initialPosition + (charController.velocity * 0.01f);
        newPosition = new Vector3(
            Mathf.Clamp(newPosition.x, initialPosition.x - swayAmount, initialPosition.x + swayAmount),
            Mathf.Clamp(newPosition.y, initialPosition.y - swayAmount, initialPosition.y + swayAmount),
            Mathf.Clamp(newPosition.z, initialPosition.z - swayAmount, initialPosition.z + swayAmount)
        );
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime * swayAmount);
    }
}
