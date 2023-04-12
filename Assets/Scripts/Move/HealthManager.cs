using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Unity.Mathematics;

public class HealthManager : NetworkBehaviour
{
    [SerializeField] private GameEvent updateHPBar, takeDamage, defeated;
    public int hp;
    [HideInInspector] public int maxHp;
    [SerializeField] private Animator head;
    [SerializeField] private Material blueHead;
    [SerializeField] private Animator torso;
    [SerializeField] private Material bluePants;

    [SerializeField] private float defenseAngle;

    [SerializeField] private GameObject sound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip defendSound;


    private void Start()
    {
        maxHp = hp;
        if ((!IsHost && IsOwner) || (IsHost && !IsOwner))
        {
            head.GetComponent<MeshRenderer>().material = blueHead;
            torso.transform.GetChild(1).GetComponent<MeshRenderer>().material = bluePants;
        }
        if (IsOwner)
        {
            torso.transform.GetChild(0).gameObject.layer = 9;
            torso.transform.GetChild(1).gameObject.layer = 9;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        if (other.CompareTag("FastPunch"))
        {
            Vector3 dif = other.transform.parent.parent.parent.position - transform.position;
            float rotY = Mathf.Atan2(dif.z, dif.x) * Mathf.Rad2Deg;

            Vector3 objFwd = transform.GetChild(0).forward;
            float angle = Vector3.Angle(objFwd, Vector3.forward);
            float sign = Mathf.Sign(Vector3.Cross(objFwd, Vector3.forward).y);

            float otherAngle = Mathf.Abs(-(rotY - 90)) - Mathf.Abs(angle * sign);

            if (Mathf.Abs(otherAngle) < defenseAngle)
            {
                if (!IsHost) TakeDamageServerRpc();
                else TakeDamageClientRpc();
            }
            else
            {
                if (!IsHost) DefendDamageServerRpc();
                else DefendDamageClientRpc();
            }
        }
    }

    [ServerRpc]
    private void TakeDamageServerRpc()
    {
        TakeDamageClientRpc();
    }


    [ClientRpc]
    private void TakeDamageClientRpc()
    {
        hp -= 1;
        takeDamage.Raise();
        Vector3 objFwd = head.gameObject.transform.forward;
        float angle = Vector3.Angle(objFwd, Vector3.forward);
        float sign = Mathf.Sign(Vector3.Cross(objFwd, Vector3.forward).y);
        torso.gameObject.transform.rotation = Quaternion.Euler(0, angle * sign * -1, 0);
        if (hp <= 0)
        {
            defeated.Raise();
            head.Play("Defeated");
            torso.Play("TorsoDefeated");
        }
        else
        {
            head.Play("Damage");
            torso.Play("TorsoDamage");
        }
        UpdateUI();
        Instantiate(sound, transform.position, transform.rotation).GetComponent<Sound>().audio = hitSound;
    }

    [ServerRpc]
    private void DefendDamageServerRpc()
    {
        DefendDamageClientRpc();
    }


    [ClientRpc]
    private void DefendDamageClientRpc()
    {
        Instantiate(sound, transform.position, transform.rotation).GetComponent<Sound>().audio = defendSound;
    }

    public void UpdateUI()
    {
        updateHPBar.Raise();
    }

    public void ResetHP()
    {
        hp = maxHp;
        head.Play("HeadIdle");
        torso.Play("TorsoIdle");
        updateHPBar.Raise();
    }
}
