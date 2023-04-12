using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    private Rigidbody rb;
    private IInput _input;

    public float maxSpeed;
    public float maxAccel;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        _input = GetComponent<IInput>();
        LobbyPosition();
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;
        HorizontalMove();
    }

    public void LobbyPosition()
    {
        if (IsHost)
        {
            transform.position = new Vector3(4, 1.5f, -4);
            transform.GetChild(0).localRotation = Quaternion.Euler(0, 315, 0);
        }
        else
        {
            transform.position = new Vector3(-4, 1.5f, 4);
            transform.GetChild(0).localRotation = Quaternion.Euler(0, 135, 0);
        }
    }

    void HorizontalMove()
    {
        Vector3 goalVel = _input.direction.normalized * maxSpeed;
        Vector3 neededAccel = goalVel - rb.velocity;
        neededAccel -= Vector3.up * neededAccel.y;
        neededAccel = Vector3.ClampMagnitude(neededAccel, maxAccel);
        rb.AddForce(neededAccel, ForceMode.Impulse);
    }
}
