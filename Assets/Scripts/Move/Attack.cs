using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Attack : NetworkBehaviour
{
    private IInput _input;
    [SerializeField] private Animator hands;
    [SerializeField] private Material blueGlove;

    public float stamina;
    [HideInInspector] public float maxStamina;
    [SerializeField] private float staminaPerAttack;
    [SerializeField] private float recoverStaminaSpeed;
    public bool isBurnout;

    [SerializeField] private float timeBetweenAttacks;
    private float currentTBA;
    public bool busted;

    public bool inRange;
    [SerializeField] private Transform sensor;
    [SerializeField] private LayerMask mask;
    [SerializeField] private float radius;

    private bool isLeft;

    [SerializeField] private GameObject sound;
    [SerializeField] private AudioClip punchAirSound;

    void Start()
    {
        _input = GetComponent<IInput>();
        maxStamina = stamina;
        if ((!IsHost && IsOwner) || (IsHost && !IsOwner))
        {
            hands.transform.GetChild(0).GetComponent<MeshRenderer>().material = blueGlove;
            hands.transform.GetChild(1).GetComponent<MeshRenderer>().material = blueGlove;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        SetVariables();
        if (_input.attack1 && currentTBA <= 0 && stamina > 0 && !isBurnout)
        {
            FastPunch();
        }
        else if(_input.attack1 && currentTBA > 0)
        {
            busted = true;
        }
    }

    private void SetVariables()
    {
        // TimeBetweenAttacks
        if (currentTBA > 0)
        {
            currentTBA -= Time.deltaTime;
            if (currentTBA <= 0 && busted && stamina > 0 && !isBurnout)
            {
                busted = false;
                FastPunch();
            }
        }

        // Stamina
        if (stamina < maxStamina && currentTBA <= 0)
        {
            stamina += Time.deltaTime * recoverStaminaSpeed;
            if (stamina == maxStamina)
            {
                isBurnout = false;
            }
            if (stamina > maxStamina)
            {
                stamina = maxStamina;
                isBurnout = false;
            }
        }

        // InRange
        inRange = Physics.OverlapSphere(sensor.position, radius, mask).Length > 0;
    }

    private void FastPunch()
    {
        currentTBA = timeBetweenAttacks;
        stamina -= staminaPerAttack;
        if (stamina < 0) isBurnout = true;
        if (!IsHost) FastPunchServerRpc();
        else FastPunchClientRpc();
    }

    public void PlayHandDefeat()
    {
        if (GetComponent<HealthManager>().hp <= 0)
        {
            hands.SetTrigger("Defeat");
        }
    }

    public void PlayHandIdle()
    {
        hands.Play("Idle");
    }

    [ServerRpc]
    void FastPunchServerRpc()
    {
        FastPunchClientRpc();
    }

    [ClientRpc]
    void FastPunchClientRpc()
    {
        hands.SetTrigger("FastPunch");
        Instantiate(sound, transform.position, transform.rotation).GetComponent<Sound>().audio = punchAirSound;
        isLeft = !isLeft;
    }
}