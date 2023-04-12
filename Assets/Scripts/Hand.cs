using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Hand : MonoBehaviour
{
    [SerializeField] private GameEvent playerHit;
    [SerializeField] private GameEvent screenShake;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private NetworkObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (player.IsOwner) playerHit.Raise();
        screenShake.Raise();
        Vector3 objFwd = transform.parent.parent.forward;
        float angle = Vector3.Angle(objFwd, Vector3.forward);
        float sign = Mathf.Sign(Vector3.Cross(objFwd, Vector3.forward).y);
        Instantiate(hitParticle, transform.position + transform.forward * 0.3f, transform.rotation).GetComponent<HitParticle>().y = angle * sign;
    }
}
