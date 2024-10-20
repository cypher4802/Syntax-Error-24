using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class M_HP : MonoBehaviour
{
    static public M_HP m_hp = null;

    public int hp = 9999;
    public int maxHp = 5;
    public Slider hpUI;
    public Animator M_hp_ani;

    private void Awake()
    {
        m_hp = this;
    }
    private void Start()
    {
        hpUI.maxValue = maxHp;
        hp = maxHp;
        flag = false;
    }
    bool flag = false;
    private void Update()
    {
        if (hp <= 0 && !flag)
        {
            gameObject.GetComponent<Animator>().Play("Die");

        }
    }
    public void Head_hit(int value) //hp가 바뀌는 부분
    {
        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.H_attack) 
            value = 3;
        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.K_attack)
            value = 5;

        hp = GetHP() - value;

        hpUI.value = hp;
        M_hp_ani.SetTrigger("HP_ani");
    }
    void Middle_hit(int value)
    {
        // SPECIAL ATTACK 여기서 수정하기
        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.H_attack)
            value = 3;

        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.K_attack)
            value = 5;

        hp = GetHP() - value;

        hpUI.value = hp;
        M_hp_ani.SetTrigger("HP_ani");
    }
    void special_hit(int value)
    {
        Debug.Log("dfsf");
        value = 10;
        hp = GetHP() - value;

        hpUI.value = hp;
    }
    public int GetHP()
    {
        return hp;
    }
}
