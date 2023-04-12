using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectator : MonoBehaviour
{
    [SerializeField] private Animator anim;


    void Start()
    {
        int i = Random.Range(1, 5);
        anim.Play("Cheer" + i,0,Random.value);
    }
}
