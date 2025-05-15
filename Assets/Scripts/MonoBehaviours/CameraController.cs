using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private float fieldOfViewMin;
    [SerializeField] private float fieldOfViewMax;

    private float targetFieldOfView;

    private void Start()
    {
        targetFieldOfView = cinemachineCamera.Lens.FieldOfView;
    }

    private void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        if(Input.GetKey(KeyCode.W))
        {
            moveDirection.z += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection.z -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection.x += 1;
        }

        Transform cameraTransform = Camera.main.transform;
        moveDirection = cameraTransform.forward * moveDirection.z + cameraTransform.right * moveDirection.x;
        moveDirection.y = 0;
        moveDirection.Normalize();

        float moveSpeed = 30f;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float rotationAngle = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            rotationAngle -= 1;
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotationAngle += 1;
        }

        float rotationSpeed = 200f;
        transform.eulerAngles += new Vector3(0f , rotationAngle * rotationSpeed * Time.deltaTime, 0f);

        float zoomAmount = 4f;
        
        if(Input.mouseScrollDelta.y > 0f)
        {
            targetFieldOfView -= zoomAmount;
        }
        if (Input.mouseScrollDelta.y < 0f)
        {
            targetFieldOfView += zoomAmount;
        }

        targetFieldOfView = Mathf.Clamp(targetFieldOfView, fieldOfViewMin, fieldOfViewMax);

        float zoomSpeed = 10f;
        cinemachineCamera.Lens.FieldOfView = Mathf.Lerp(cinemachineCamera.Lens.FieldOfView, targetFieldOfView, zoomSpeed * Time.deltaTime); 

    }
}
