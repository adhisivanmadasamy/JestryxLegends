using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] AudioSource AllAudioSource;
    //[Space]
    //[Header("Clips")]
    [SerializeField] AudioClip PistolClip,RiffleClip, knifeSwingClip, spikeplantedClip, runClip, jumpClip;

    private void Awake()
    {
        instance = this;
    }

    public void pistolSound()
    {
        AllAudioSource.PlayOneShot(PistolClip);
    }
    public void jumpSound()
    {
        AllAudioSource.PlayOneShot(jumpClip);
    }
    public void KnifeSound()
    {
        AllAudioSource.PlayOneShot(knifeSwingClip);
    }
    public void runSound()
    {
        //AllAudioSource.PlayOneShot(runClip);
    }
    public void spikeplantedSound()
    {
        AllAudioSource.PlayOneShot(spikeplantedClip);
    }
}
