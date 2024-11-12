using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win_cinema : MonoBehaviour
{
    AudioSource audio;
    private void Start()
    {
        audio = GetComponent<AudioSource>();

        // 라운드 체크 해제
        RoundCheckManager.Check_round = 0;
        RoundCheckManager.instance.rc1 = false;
        RoundCheckManager.instance.rc2 = false;
        RoundCheckManager.instance.rc3 = false;
        RoundCheckManager.instance.rc4 = false;

    }
    void sound()
    {
        audio.enabled = true;
    }
}
