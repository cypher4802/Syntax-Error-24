using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP_ani : MonoBehaviour
{

    Animator ani;

    private void Start()
    {
        ani = GetComponent<Animator>();
    }
    void hp_shake_ani(string str)
    {
        if (str == "guard")
        {
            Debug.Log("실행");
            ani.SetTrigger("HP_ani");

        }
        if (str == "maskman")
        {
            Debug.Log("마스크맨 실행.");
            ani.SetTrigger("HP_ani");
        }
    }

}
