using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class _2023_06_01 : MonoBehaviour
{
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
    [SerializeField]
    private Transform aimPoint;


    [SerializeField] 
    private float maxDistance;
    [SerializeField]
    private ParticleSystem muzzleEffect;
    [SerializeField]
    private ParticleSystem bulletEffect;
    [SerializeField]
    private TrailRenderer bulletTrail;

    private Stack<ParticleSystem> bulletEffects = new Stack<ParticleSystem>();
    private Stack<TrailRenderer> bulletTrails = new Stack<TrailRenderer>();

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        isFPS = false;
        cameraRoot = cameraRootTPS;
        FPSCam.gameObject.SetActive(false);
        TPSCam.gameObject.SetActive(true);
    }
    private void Update()
    {
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
        aimPoint.position = lookPoint;
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

    public void Fire()
    {
        muzzleEffect.Play();
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, maxDistance))
        {
            if (bulletEffects.Count < 1)
            {
                StartCoroutine(BulletEffectRoutine(hit));
            }
            else
            {
                bulletEffects.Peek().transform.position = hit.transform.position;
                bulletEffects.Peek().gameObject.SetActive(true);
                bulletEffects.Pop();
            }
            if (bulletTrails.Count < 1)
            {
                TrailRenderer currentRenderer = Instantiate(bulletTrail, muzzleEffect.transform.position, Quaternion.identity);
                StartCoroutine(BulletTrailRoutine(currentRenderer, hit));
            }
            else
            {
                bulletTrails.Peek().transform.position = muzzleEffect.transform.position;
                bulletTrails.Peek().gameObject.SetActive(true);
                StartCoroutine(BulletTrailRoutine(bulletTrails.Peek(), hit));
                bulletTrails.Pop();
            }
        }
    }
    IEnumerator BulletEffectRoutine(RaycastHit hit)
    {
        ParticleSystem currentEffect = Instantiate(bulletEffect, hit.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        bulletEffects.Push(currentEffect);
        currentEffect.gameObject.SetActive(false);
    }
    IEnumerator BulletTrailRoutine(TrailRenderer currentTrail, RaycastHit hit)
    {
        while (Vector3.Distance(currentTrail.transform.position, hit.transform.position) < 0.1f)
        {
            currentTrail.transform.position = Vector3.MoveTowards(currentTrail.transform.position, hit.transform.position, 5f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        bulletTrails.Push(currentTrail);
        currentTrail.gameObject.SetActive(false);
    }
}
