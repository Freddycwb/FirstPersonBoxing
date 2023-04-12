using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;

public class Vision : NetworkBehaviour
{
    private ILook _look;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Transform playerBody;




    private Camera camera;
    private Camera effectsCamera;

    private float defaultFOV;
    [SerializeField] private float additionalFOV;
    private float currentAdditionalFOV;
    public float fovRecovery;

    private float shakeIntensity;
    public float shakeRecovery;
    public float frenquency;
    public float magnetude;

    public void OnEnable()
    {
        yRotation = transform.parent.parent.position.x > 0 ? 315 : 135;
    }

    private void Start()
    {
        _look = GetComponent<ILook>();
        playerBody = transform.parent;
        camera = GetComponent<Camera>();
        effectsCamera = GetComponentInChildren<Camera>();
        defaultFOV = camera.fieldOfView;
    }

    private void Update()
    {
        xRotation -= _look.direction.y;
        xRotation = Mathf.Clamp(xRotation, -20f, 20f);

        yRotation += _look.direction.x;
        playerBody.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        if(shakeIntensity > 0)
        {
            shakeIntensity -= Time.deltaTime * shakeRecovery;
            if (shakeIntensity < 0) shakeIntensity = 0;
            transform.localPosition = new Vector3(Mathf.Sin(Time.time * frenquency * shakeIntensity) * magnetude * shakeIntensity, transform.localPosition.y, transform.localPosition.z);
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }

        if (currentAdditionalFOV > 0)
        {
            currentAdditionalFOV -= Time.deltaTime * fovRecovery;
            if (currentAdditionalFOV < 0) currentAdditionalFOV = 0;
            camera.fieldOfView = defaultFOV + currentAdditionalFOV;
            effectsCamera.fieldOfView = defaultFOV + currentAdditionalFOV;
        }
        else
        {
            camera.fieldOfView = defaultFOV;
            effectsCamera.fieldOfView = defaultFOV;
        }
    }

    public void ScreenShake()
    {
        shakeIntensity = 1;
    }

    public void ChangeFOV()
    {
        currentAdditionalFOV = additionalFOV;
    }
}
