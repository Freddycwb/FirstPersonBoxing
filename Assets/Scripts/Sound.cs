using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Sound : MonoBehaviour
{
    public bool isMusic;
    public AudioClip audio;
    [SerializeField] private AudioSource source;

    void Start()
    {
        name = audio.name + "Sound";
        source.clip = audio;
        if(!isMusic)
        {
            StartCoroutine("Lifetime");
        }
        else
        {
            source.enabled = true;
        }
    }

    private IEnumerator Lifetime()
    {
        source.enabled = true;
        source.volume = PlayerPrefs.GetFloat("sfxVolume");
        yield return new WaitForSeconds(audio.length);
        Destroy(gameObject);
    }
}
