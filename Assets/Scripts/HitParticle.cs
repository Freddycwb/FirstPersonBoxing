using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticle : MonoBehaviour
{
    private Vector3 cameraDir;
    private float z;
    public float y;

    [SerializeField] private float timeToDestroy;
    private float currentTimeToDestroy;

    private void Start()
    {
        z = Random.Range(0, 360);
        transform.rotation = Quaternion.Euler(transform.rotation.x, y, z);
    }

    private void Update()
    {
        currentTimeToDestroy += Time.deltaTime;
        if (currentTimeToDestroy >= timeToDestroy) Destroy(gameObject);
    }
}
