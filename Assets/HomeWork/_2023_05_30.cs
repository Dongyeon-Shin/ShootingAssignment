using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class _2023_05_30 : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 moveDirection;
    [SerializeField]
    private float moveSpeed;
    private float ySpeed = 0;
    [SerializeField]
    private float jumpPower;
    [SerializeField]
    private Transform cameraRoot;
    [SerializeField]
    private float mouseSensitivity;
    private Vector2 lookDelta;
    private float xRotation;
    private float yRotation;
    [SerializeField]
    private CinemachineVirtualCamera FPSCam;
    [SerializeField]
    private CinemachineVirtualCamera TPSCam;
    private bool isFPS;
    [SerializeField]
    private float lookDistance;
    [SerializeField]
    private Transform cameraRootTPS;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        isFPS = true;
        cameraRoot = FPSCam.transform;
        FPSCam.gameObject.SetActive(true);
        TPSCam.gameObject.SetActive(false);
    }
    private void Update()
    {
        Move();
        Jump();
        Rotate();
    }
    private void LateUpdate()
    {
        Look();
    }
    private void Look()
    {
        if (isFPS)
        {
            yRotation += lookDelta.x * mouseSensitivity * Time.deltaTime;
            xRotation -= lookDelta.y * mouseSensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            cameraRoot.localRotation = Quaternion.Euler(xRotation, 0, 0);
            transform.localRotation = Quaternion.Euler(0, yRotation, 0);
        }
        else
        {
            yRotation += lookDelta.x * mouseSensitivity * Time.deltaTime;
            xRotation -= lookDelta.y * mouseSensitivity * Time.deltaTime;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f);
            cameraRoot.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        }
    }
    private void Rotate()
    {
        Vector3 lookPoint = Camera.main.transform.position + Camera.main.transform.forward * lookDistance;
        lookPoint.y = transform.position.y;
        transform.LookAt(lookPoint);
    }
    private void OnLook(InputValue value)
    {
        lookDelta = value.Get<Vector2>();
    }
    private void OnChangePOV(InputValue value)
    {
        isFPS = !isFPS;
        FPSCam.gameObject.SetActive(!FPSCam.gameObject.active);
        TPSCam.gameObject.SetActive(!TPSCam.gameObject.active);
        if (cameraRoot == FPSCam.transform)
        {
            cameraRoot = cameraRootTPS;
        }
        else
        {
            cameraRoot = FPSCam.transform;
        }
    }
    private void Move()
    {
        controller.Move(transform.forward * moveDirection.z * moveSpeed * Time.deltaTime);
        controller.Move(transform.right * moveDirection.x * moveSpeed * Time.deltaTime);
    }
    private void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        moveDirection = new Vector3(input.x, 0, input.y);
    }
    private void Jump()
    {
        ySpeed += Physics.gravity.y * Time.deltaTime;
        if (GroundCheck() && ySpeed < 0)
        {
            ySpeed = -1;
        }

        controller.Move(Vector3.up * ySpeed * Time.deltaTime);
    }
    private void OnJump(InputValue value)
    {
        if (GroundCheck())
        {
            ySpeed = jumpPower;
        }
    }
    private bool GroundCheck()
    {
        RaycastHit hit;
        return Physics.SphereCast(transform.position + Vector3.up * 1, transform.localScale.y / 2f, Vector3.down, out hit, transform.localScale.y);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 1, transform.localScale.y / 2f);
    }
}
