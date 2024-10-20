using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class G_HP : MonoBehaviour
{
    static public G_HP g_hp = null;

    public int hp = 9999;
    public int maxHp = 5;
    public Slider hpUI;
    public Animator G_hp_ani;
    

    private void Awake()
    {
        g_hp=this; 
    }
    private void Start()
    {
        hpUI.maxValue = maxHp;
        hp = maxHp;
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
        if (Maskman_ani_Setting.M_A_T == Maskman_ani_Setting.ani_state.H_attack)
            value = 3;
        if (Maskman_ani_Setting.M_A_T == Maskman_ani_Setting.ani_state.K_attack)
            value = 5;
        hp = GetHP() - value;    
        hpUI.value = hp;
        G_hp_ani.SetTrigger("HP_ani");

    }
    void Middle_hit(int value)
    {       
        if (Maskman_ani_Setting.M_A_T == Maskman_ani_Setting.ani_state.H_attack)
        {
            value = 3;
        }
        if (Maskman_ani_Setting.M_A_T == Maskman_ani_Setting.ani_state.K_attack)
        {
            value = 5;
        }
        hp = GetHP() - value;
        G_hp_ani.SetTrigger("HP_ani");
        hpUI.value = hp;
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
