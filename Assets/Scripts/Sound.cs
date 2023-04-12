using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Sound : MonoBehaviour
{
    public AudioClip audio;
    [SerializeField] private AudioSource source;

    void Start()
    {
        name = audio.name + "Sound";
        source.clip = audio;
        StartCoroutine("Lifetime");
    }

    private IEnumerator Lifetime()
    {
        source.enabled = true;
        yield return new WaitForSeconds(audio.length);
        Destroy(gameObject);
    }
}
