using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float mouseSensitivity = 10;
    public Transform player;
    public float distanceFromTarget = 8;
    public float rotationTime = .12f;
    Vector3 rotationVelocity;
    Vector3 currentRotation;
    public Vector2 xAxisMinMax = new Vector2(-20, 20);
    public bool lockCursor;
    float yAxis;
    float xAxis;
    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    void LateUpdate()
    {
        yAxis += Input.GetAxis("Mouse X") * mouseSensitivity;
        xAxis -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        xAxis = Mathf.Clamp(xAxis, xAxisMinMax.x, xAxisMinMax.y);


        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(xAxis, yAxis), ref rotationVelocity, rotationTime);

        transform.eulerAngles = currentRotation;

        transform.position = player.position - transform.forward * distanceFromTarget;
    }
}
