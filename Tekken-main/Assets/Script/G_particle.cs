using UnityEngine;

public class G_particle : MonoBehaviour
{
    public Animator ani;
    public GameObject hit_hand;
    public GameObject guard;
    public GameObject hit_kick;


    // hit_possibe이 켜져있으면 hit 아니면 guard
    void Hit_Guard(Collision collision, bool hand)
    {
        // 손
        if (hand)
        {
            if (ani.GetBool("Hit_possible"))
                Instantiate(hit_hand, collision.transform.position, collision.transform.rotation);
            else
                Instantiate(guard, collision.transform.position, collision.transform.rotation);
        }
        // 발
        if (!hand)
        {
            if (ani.GetBool("Hit_possible"))
                Instantiate(hit_kick, collision.transform.position, collision.transform.rotation);
            else
                Instantiate(guard, collision.transform.position, collision.transform.rotation);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // 왼손
        if (collision.collider.CompareTag("Hand_L"))
        {
            if (collision.transform.position.y > 1.2f)
            {
                ani.SetTrigger("hit_head_L");
                Hit_Guard(collision, true);
            }
            else if (collision.transform.position.y > 0.61f && collision.transform.position.y < 1.2f)
            {
                ani.SetTrigger("hit_mid_L");
                Hit_Guard(collision, true);
            }

            else if (collision.transform.position.y < 0.6f)
            {
                ani.SetTrigger("hit_low_L");
                Hit_Guard(collision, true);
            }
        }

        // 오른손
        if (collision.collider.CompareTag("Hand_R"))
        {
            if (collision.transform.position.y > 1.2f)
            {
                ani.SetTrigger("hit_head_R");
                Hit_Guard(collision, true);
            }
            else if (collision.transform.position.y > 0.61f && collision.transform.position.y < 1.2f)
            {
                ani.SetTrigger("hit_mid_R");
                Hit_Guard(collision, true);
            }
            else if (collision.transform.position.y < 0.6f)
            {
                ani.SetTrigger("hit_low_R");
                Hit_Guard(collision, true);
            }
        }

        //왼발
        if (collision.collider.CompareTag("Kick_L"))
        {
            if (collision.transform.position.y > 1.2f)
            {
                ani.SetTrigger("hit_head_L");
                Hit_Guard(collision, false);
            }
            else if (collision.transform.position.y > 0.61f && collision.transform.position.y < 1.2f)
            {
                ani.SetTrigger("hit_mid_L");
                Hit_Guard(collision, false);
            }
            else if (collision.transform.position.y < 0.6f)
            {
                ani.SetTrigger("hit_low_L");
                Hit_Guard(collision, false);
            }
        }

        // 오른발
        if (collision.collider.CompareTag("Kick_R"))
        {
            if (collision.transform.position.y > 1.2f)
            {
                ani.SetTrigger("hit_head_L");
                Hit_Guard(collision, false);
            }
            else if (collision.transform.position.y > 0.61f && collision.transform.position.y < 1.2f)
            {
                ani.SetTrigger("hit_mid_L");
                Hit_Guard(collision, false);
            }
            else if (collision.transform.position.y < 0.6f)
            {
                ani.SetTrigger("hit_low_L");
                Hit_Guard(collision, false);
            }
        }

    }
}
