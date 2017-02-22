using UnityEngine;


public class FlyCamera : MonoBehaviour
{
    public float mouseSensitivity = 4;
    public float speed = 10;
    public float speedMode = 20;

    private float rotationY;


    private void Update()
    {
        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY += Input.GetAxis("Mouse Y") * mouseSensitivity;
        rotationY = Mathf.Clamp(rotationY, -90, 90);
        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0.0f);

        Vector3 dir = GetDirection();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            dir = dir * speedMode;
        }
        else
        {
            dir = dir * speed;
        }

        dir = dir * Time.deltaTime;
        transform.Translate(dir);
    }

    private static Vector3 GetDirection()
    {
        Vector3 velocity = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.Q))
        {
            velocity += new Vector3(0.0f, -1.0f, 0.0f);
        }
        if (Input.GetKey(KeyCode.E))
        {
            velocity += new Vector3(0.0f, 1.0f, 0.0f);
        }
        return velocity;
    }
}