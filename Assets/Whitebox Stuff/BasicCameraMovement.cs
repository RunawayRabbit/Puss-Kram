using UnityEngine;

public class BasicCameraMovement : MonoBehaviour
{
    [SerializeField, Range(1.0f, 30.0f)] float cameraSpeed = 7.0f;
    [SerializeField, Range(1.0f, 30.0f)] float fastCameraSpeed = 12.0f;
    [SerializeField, Range(0.5f, 4.0f)] float mouseSensitivity = 1.00f;

    private void Update()
    {
        if(Input.GetKey(KeyCode.Mouse1))
        { 
            float pitch = Input.GetAxis("Mouse Y") * mouseSensitivity;
            float yaw = Input.GetAxis("Mouse X") * mouseSensitivity;

            Transform t = Camera.main.transform;
            t.eulerAngles = new Vector3(t.eulerAngles.x - pitch, t.eulerAngles.y + yaw, t.eulerAngles.z);
        }
        float speed = cameraSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
            speed = fastCameraSpeed;

        Vector3 movementInput = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            movementInput.z = 1.0f;
        else if (Input.GetKey(KeyCode.S))
            movementInput.z = -1.0f;

        if (Input.GetKey(KeyCode.A))
            movementInput.x = -1.0f;
        else if (Input.GetKey(KeyCode.D))
            movementInput.x = 1.0f;

        if (Input.GetKey(KeyCode.Q))
            movementInput.y = -1.0f;
        else if (Input.GetKey(KeyCode.E))
            movementInput.y = 1.0f;

        movementInput.Normalize();
        movementInput *= speed * Time.deltaTime;
        movementInput = Camera.main.transform.rotation * movementInput;
        transform.position += movementInput;
    }
}
