using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Sound : MonoBehaviour
{
    // »ç¿îµå
    public AudioClip[] Audioclip;
    AudioSource soundSource;

    void Guard_Sound()
    {
        soundSource.clip = Audioclip[0];
        soundSource.PlayOneShot(Audioclip[0]);
    }
    void R_jab_sound()
    {
        soundSource.PlayOneShot(Audioclip[6]);
    }
    void L_jab_sound()
    {
        soundSource.PlayOneShot(Audioclip[7]);
    }
    void RL_jab_sound()
    {
        soundSource.PlayOneShot(Audioclip[8]);
    }
    void R_kick_sound()
    {
        soundSource.PlayOneShot(Audioclip[9]);
    }
    void L_kick_sound()
    {
        soundSource.PlayOneShot(Audioclip[10]);
    }
    void strongAttack_sound()
    {
        soundSource.PlayOneShot(Audioclip[11]);
    }
    void Start()
    {       
        soundSource = gameObject.GetComponent<AudioSource>();
    }
}
