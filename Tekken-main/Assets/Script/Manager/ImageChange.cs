using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageChange : MonoBehaviour
{
    public Sprite Change_img;
    public GameObject RoundCheck1;
    public GameObject RoundCheck2;
    public GameObject RoundCheck3;
    public GameObject RoundCheck4;
    public TextMeshProUGUI Round2text;
    public TextMeshProUGUI FinalRoundtext;


    int T = 0;
    int F = 0;
    bool flag;
    // Start is called before the first frame update
    private void Awake()
    {

    }
    void check(bool RC, GameObject GO)
    {
        if (RC == true)
        {
            GO.SetActive(true);
        }
    }
    void Start()
    {
        flag = true;
        check(RoundCheckManager.instance.rc1, RoundCheck1);
        check(RoundCheckManager.instance.rc2, RoundCheck2);
        check(RoundCheckManager.instance.rc3, RoundCheck3);
        check(RoundCheckManager.instance.rc4, RoundCheck4);
        

    }

    // Update is called once per frame
    public void Update()
    {
        if (M_HP.m_hp.hp <= 0 && flag)
        {
            //rc1이 true일때 rc2가 true가 됌
            if (RoundCheckManager.instance.rc1 == true)
                RoundCheckManager.instance.rc2 = true;

            RoundCheckManager.instance.rc1 = true;
            flag = false;
        }

        if (G_HP.g_hp.hp <= 0 && flag)
        {
            if (RoundCheckManager.instance.rc3 == true)
                RoundCheckManager.instance.rc4 = true;
           
            RoundCheckManager.instance.rc3 = true;
            flag = false;
        }
    }
}
