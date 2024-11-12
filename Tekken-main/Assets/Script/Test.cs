using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Animator ani;
    public GameObject hit_hand;
    public GameObject guard;
    public GameObject hit_kick;
    public Knockback G_knockback;
    public Knockback M_knockback;

    // test 가드도 추가해야됌
    IEnumerator specialAttackdelay()
    {
        yield return new WaitForSeconds(1f);
        Maskman_ani_Setting.M_S_T = Maskman_ani_Setting.special_state.idle;
    }


    // hit_possibe이 켜져있으면 hit 아니면 guard
    void Hit_Guard(Transform t, bool hand)
    {
        // 손
        if (hand)
        {
            // particle
            if (ani.GetBool("Hit_possible"))    // hit_possible이 아니고 guard로 바꿔야 함
                Instantiate(hit_hand, t.position, t.rotation);
            else
            {
                Instantiate(guard, t.position, t.rotation);
                // 넉백 
                //Debug.Log(gameObject.name);
                if (Maskman_ani_Setting.M_A_T == Maskman_ani_Setting.ani_state.backstep)
                {
                    // 가드 special attack
                    if (Guard_ani_Setting.G_S_T == Guard_ani_Setting.special_state.specialattack)
                    {
                        M_knockback.ApplyKnockback(-transform.forward, 5);
                        // special_state = specialattack  - > special_state = idle 딜레이주기                       
                    }
                    else
                    {
                        M_knockback.ApplyKnockback(-transform.forward, 1);
                    }
                }
                if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.backstep)
                {
                    // 마스크맨 special attack
                    if (Maskman_ani_Setting.M_S_T == Maskman_ani_Setting.special_state.specialattack)
                    {
                        G_knockback.ApplyKnockback(-transform.forward, 5);
                        // special_state = specialattack  - > special_state = idle 딜레이주기                       
                    }
                    else
                    {
                       G_knockback.ApplyKnockback(-transform.forward, 1);
                    }
                }
            }

            // collider가 maskman인지 Guard인지 체크
            if (gameObject.name.Contains("M_Wall_hit") && Maskman_ani_Setting.M_A_T != Maskman_ani_Setting.ani_state.backstep)
            {
                // 콤보 도중에 막을 수 있는 상황 방지하기 위해 코루틴 넣어야되나
                Maskman_ani_Setting.M_A_T = Maskman_ani_Setting.ani_state.Hit_hand;

            }
            else if (gameObject.name.Contains("G_Wall_hit") && Guard_ani_Setting.G_A_T != Guard_ani_Setting.ani_state.backstep)
            {
                Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.Hit_hand;
            }
        }
        // 발
        if (!hand)
        {
            // particle
            if (ani.GetBool("Hit_possible"))
                Instantiate(hit_kick, t.position, t.rotation);
            else
            {
                Instantiate(guard, t.position, t.rotation);
                if (Maskman_ani_Setting.M_A_T == Maskman_ani_Setting.ani_state.backstep)
                {
                    // 가드 special attack
                    if (Guard_ani_Setting.G_S_T == Guard_ani_Setting.special_state.specialattack)
                    {
                        M_knockback.ApplyKnockback(-transform.forward, 5);
                        // special_state = specialattack  - > special_state = idle 딜레이주기                       
                    }
                    else
                    {
                        M_knockback.ApplyKnockback(-transform.forward, 1.5f);
                    }
                }
                if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.backstep)
                {
                    // 마스크맨 special attack
                    if (Maskman_ani_Setting.M_S_T == Maskman_ani_Setting.special_state.specialattack)
                    {
                        G_knockback.ApplyKnockback(-transform.forward, 5);
                        // special_state = specialattack  - > special_state = idle 딜레이주기                       
                    }
                    else
                    {
                        G_knockback.ApplyKnockback(-transform.forward, 1);
                    }
                }
            }

            if (gameObject.name.Contains("M_Wall_hit") && Maskman_ani_Setting.M_A_T != Maskman_ani_Setting.ani_state.backstep)
                Maskman_ani_Setting.M_A_T = Maskman_ani_Setting.ani_state.Hit_kick;

            else if (gameObject.name.Contains("G_Wall_hit") && Guard_ani_Setting.G_A_T != Guard_ani_Setting.ani_state.backstep)
                Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.Hit_kick;
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        //Transform t = collision.transform;
        Transform t = other.transform;
        // 왼손
        if (other.CompareTag("Hand_L"))
        {
            if (t.position.y > 1.2f)
            {
                ani.SetTrigger("hit_head_L");
                Hit_Guard(t, true);
            }
            else if (t.position.y > 0.61f && t.position.y < 1.2f)
            {
                ani.SetTrigger("hit_mid_L");
                Hit_Guard(t, true);
            }

            else if (t.position.y < 0.6f)
            {
                ani.SetTrigger("hit_low_L");
                Hit_Guard(t, true);
            }
        }

        // 오른손
        if (other.CompareTag("Hand_R"))
        {
            if (t.position.y > 1.2f)
            {
                // 마스크맨도 추가해야됌
                if (Guard_ani_Setting.G_S_T == Guard_ani_Setting.special_state.specialattack)
                {
                    if (ani.GetBool("Hit_possible"))
                        ani.SetTrigger("special_hit");
                    else
                        ani.SetTrigger("hit_head_R");
                }
                else
                    ani.SetTrigger("hit_head_R");
                Hit_Guard(t, true);
            }
            else if (t.position.y > 0.61f && t.position.y < 1.2f)
            {
                if (Guard_ani_Setting.G_S_T == Guard_ani_Setting.special_state.specialattack)
                {
                    if (ani.GetBool("Hit_possible"))
                        ani.SetTrigger("special_hit");
                    else
                        ani.SetTrigger("hit_head_R");
                }
                else
                    ani.SetTrigger("hit_mid_R");

                Hit_Guard(t, true);
            }
            else if (t.position.y < 0.6f)
            {
                ani.SetTrigger("hit_low_R");
                Hit_Guard(t, true);
            }
        }

        //왼발
        if (other.CompareTag("Kick_L"))
        {
            if (t.position.y > 1.2f)
            {
                if (Maskman_ani_Setting.M_S_T == Maskman_ani_Setting.special_state.specialattack)
                {
                    if (ani.GetBool("Hit_possible"))
                        ani.SetTrigger("special_hit");
                    else
                        ani.SetTrigger("hit_head_L");
                }
                else
                    ani.SetTrigger("hit_head_L");
                Hit_Guard(t, false);
            }
            else if (t.position.y > 0.0f && t.position.y < 1.2f)
            {
                if (Maskman_ani_Setting.M_S_T == Maskman_ani_Setting.special_state.specialattack)
                {
                    if (ani.GetBool("Hit_possible"))
                        ani.SetTrigger("special_hit");
                    else
                        ani.SetTrigger("hit_head_L");
                }
                else
                    ani.SetTrigger("hit_mid_L");
                Hit_Guard(t, false);
            }

        }

        // 오른발
        if (other.CompareTag("Kick_R"))
        {
            if (t.position.y > 1.2f)
            {
                if (Maskman_ani_Setting.M_S_T == Maskman_ani_Setting.special_state.specialattack)
                {
                    if (ani.GetBool("Hit_possible"))
                        ani.SetTrigger("special_hit");
                    else
                        ani.SetTrigger("hit_head_R");
                }
                else
                ani.SetTrigger("hit_head_R");
                Hit_Guard(t, false);
            }
            else if (t.position.y > 0f && t.position.y < 1.2f)
            {
                if (Maskman_ani_Setting.M_S_T == Maskman_ani_Setting.special_state.specialattack)
                {
                    if (ani.GetBool("Hit_possible"))
                        ani.SetTrigger("special_hit");
                    else
                        ani.SetTrigger("hit_head_R");
                }
                else
                    ani.SetTrigger("hit_mid_R");
                Hit_Guard(t, false);
            }

            // 하단 일시 중단
            //else if (t.position.y < 0.6f)
            //{
            //    ani.SetTrigger("hit_low_L");
            //    Hit_Guard(collision, false);
            //}
        }

        //    private void OnCollisionEnter(Collision collision)
        //{
        //    print(collision.transform.name + ", " + collision.collider.name);
        //    //Transform t = collision.transform;
        //    Transform t = collision.collider.transform;
        //    // 왼손
        //    if (collision.collider.CompareTag("Hand_L"))
        //    {
        //        if (t.position.y > 1.2f)
        //        {
        //            ani.SetTrigger("hit_head_L");
        //            Hit_Guard(collision, true);
        //        }
        //        else if (t.position.y > 0.61f && t.position.y < 1.2f)
        //        {
        //            ani.SetTrigger("hit_mid_L");
        //            Hit_Guard(collision, true);
        //        }

        //        else if (t.position.y < 0.6f)
        //        {
        //            ani.SetTrigger("hit_low_L");
        //            Hit_Guard(collision, true);
        //        }
        //    }

        //    // 오른손
        //    if (collision.collider.CompareTag("Hand_R"))
        //    {
        //        if (t.position.y > 1.2f)
        //        {
        //            ani.SetTrigger("hit_head_R");
        //            Hit_Guard(collision, true);
        //        }
        //        else if (t.position.y > 0.61f && t.position.y < 1.2f)
        //        {
        //            ani.SetTrigger("hit_mid_R");
        //            Hit_Guard(collision, true);
        //        }
        //        else if (t.position.y < 0.6f)
        //        {
        //            ani.SetTrigger("hit_low_R");
        //            Hit_Guard(collision, true);
        //        }
        //    }

        //    //왼발
        //    if (collision.collider.CompareTag("Kick_L"))
        //    {
        //        if (t.position.y > 1.2f)
        //        {
        //            ani.SetTrigger("hit_head_L");
        //            Hit_Guard(collision, false);
        //        }
        //        else if (t.position.y > 0.0f && t.position.y < 1.2f)
        //        {
        //            ani.SetTrigger("hit_mid_L");
        //            Hit_Guard(collision, false);
        //        }
        //        // 하단 일시 중단
        //        //else if (t.position.y < 0.6f)
        //        //{
        //        //    ani.SetTrigger("hit_low_L");
        //        //    Hit_Guard(collision, false);
        //        //}
        //    }

        //    // 오른발
        //    if (collision.collider.CompareTag("Kick_R"))
        //    {
        //        if (t.position.y > 1.2f)
        //        {
        //            ani.SetTrigger("hit_head_L");
        //            Hit_Guard(collision, false);
        //        }
        //        else if (t.position.y > 0f && t.position.y < 1.2f)
        //        {
        //            ani.SetTrigger("hit_mid_L");
        //            Hit_Guard(collision, false);
        //        }

        //        // 하단 일시 중단
        //        //else if (t.position.y < 0.6f)
        //        //{
        //        //    ani.SetTrigger("hit_low_L");
        //        //    Hit_Guard(collision, false);
        //        //}
        //    }

    }
}
