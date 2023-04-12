using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;

public class PlayerInput : NetworkBehaviour, IInput
{
    [SerializeField] private GameEvent spawn, ready, leave, shutdown;
    [SerializeField] private GameObject camera;
    private Transform head;
    private bool canControl;

    private void Start()
    {
        head = transform.GetChild(0);
        spawn.Raise();
    }

    public void ReadyCliked()
    {
        if (!IsOwner) return;
        if (IsHost)
        {
            PlayerReadyClientRpc();
        }
        else
        {
            PlayerReadyServerRpc();
        }
    }

    [ServerRpc]
    public void PlayerReadyServerRpc()
    {
        PlayerReadyClientRpc();
    }

    [ClientRpc]
    public void PlayerReadyClientRpc()
    {
        ready.Raise();
    }

    public void LeaveCliked()
    {
        if (!IsOwner) return;
        if (IsHost)
        {
            ShutdownClientRpc();
        }
        else
        {
            PlayerLeaveServerRpc();
        }
    }

    [ServerRpc]
    public void PlayerLeaveServerRpc()
    {
        PlayerLeaveClientRpc();
        Debug.Log("Escutou o click");
    }

    [ClientRpc]
    public void PlayerLeaveClientRpc()
    {
        leave.Raise();
        Debug.Log("Disparou o leave");
    }

    [ClientRpc]
    public void ShutdownClientRpc()
    {
        shutdown.Raise();
        Debug.Log("Disparou o leave");
    }

    public void EnableControls(bool enable)
    {
        if (!enable)
        {
            GetComponent<Attack>().busted = false;
        }
        if (!IsOwner) return;
        camera.SetActive(enable);
        camera.transform.GetChild(0).GetComponent<Camera>().enabled = !enable;
        camera.transform.GetChild(0).GetComponent<Camera>().enabled = enable;
        canControl = enable;
    }

    public Vector3 direction
    {
        get
        {
            if (canControl)
            {
                float x = Input.GetAxisRaw("Horizontal");
                float z = Input.GetAxisRaw("Vertical");

                Vector3 move = new Vector3(x, 0, z);

                float headAngle = Mathf.Deg2Rad * (360 - head.rotation.eulerAngles.y);

                Vector3 a = new Vector3(Mathf.Cos(headAngle), 0, Mathf.Sin(headAngle));
                Vector3 b = new Vector3(-Mathf.Sin(headAngle), 0, Mathf.Cos(headAngle));

                Vector3 rotatedMove = move.x * a + move.z * b;

                return rotatedMove;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public bool attack1
    {
        get
        {
            if (canControl)
            {
                return Input.GetMouseButtonDown(0);
            }
            else
            {
                return false;
            }
        }
    }

    public bool attack2
    {
        get
        {
            if (canControl) 
            { 
                return Input.GetMouseButtonDown(1);
            }
            else
            {
                return false;
            }
        }
    }

    public bool defending
    {
        get
        {
            if (canControl) 
            { 
                return Input.GetMouseButton(0) && Input.GetMouseButton(1);
            }
            else
            {
                return false;
            }
        }
    }
}
