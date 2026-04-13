using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Up movement")]
    public float riseSpeed = 1.5f;

    [Header("Sway movement")]
    public float swayAmount = 0.5f;
    public float swaySpeed = 1.2f;

    [Header("Tilt rotation")]
    public float rotationAmount = 8f;
    public float rotationSpeed = 1f;

    [Header("Y sway rotation")]
    public float yRotationAmount = 10f;
    public float yRotationSpeed = 1f;

    private Vector3 startPosition;
    private float startYRotation;

    void Start()
    {
        startPosition = transform.position;
        startYRotation = transform.eulerAngles.y;
    }

    void Update()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float tilt = Mathf.Sin(Time.time * rotationSpeed) * rotationAmount;
        float yTilt = Mathf.Sin(Time.time * yRotationSpeed) * yRotationAmount;

        transform.position = new Vector3(
            startPosition.x + sway,
            transform.position.y + riseSpeed * Time.deltaTime,
            startPosition.z
        );

        transform.rotation = Quaternion.Euler(0f, startYRotation + yTilt, tilt);
    }
}