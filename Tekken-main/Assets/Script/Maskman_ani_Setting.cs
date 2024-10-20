using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Maskman_ani_Setting : MonoBehaviour
{
    public enum ani_state
    {
        idle,
        forwardstep,
        backstep,
        sit,
        jump,
        lay,
        guard,
        H_attack,
        K_attack,
        Hit_hand,
        Hit_kick,
        Upper
        
    }

    public enum special_state
    {
        idle,
        upper,
        Floating,
        specialattack
    }

    public lookat La;

    Animator ani;
    Rigidbody rb;

    static public ani_state M_A_T;
    static public special_state M_S_T;

    // 사운드
    public AudioClip[] Audioclip;
    AudioSource soundSource;

    public GameObject Hand_R;
    public GameObject Hand_L;
    public GameObject kick_R;
    public GameObject kick_L;

    // 공중에 떠있는지 체크
    bool isfloating;

    // dash,backstep,sit,jump 플래그
    bool D_flag;
    bool B_flag;
    bool S_flag;
    bool J_flag;

    IEnumerator delay(string S)
    {
        yield return new WaitForSeconds(0.2f);
        ani.SetBool(S, false);

    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.2f);
        D_flag = false;
        B_flag = false;
        J_flag = false;
        S_flag = false;
    }

    // Guard animation events 
    IEnumerator FloatingColliderdelay()
    {
        yield return new WaitForSeconds(0.4f);
        //콜라이더 각도와 포지션 수정
        transform.Find("M_Wall_hit").transform.localEulerAngles = new Vector3(-90, 0, 0);
        transform.Find("M_Wall_hit").transform.localPosition = new Vector3(0, 0, 0.3f);
    }

    // floating 상태일 때 hit
    void floatingHit ()
    {
        rb.velocity = Vector3.zero;
        rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);
        
        ani.SetTrigger("air hit");
    }

    // hand_hit sound
    // 머리 맞는 애니메이션에 넣음
    void Head_hit()
    {        // 손과 발
        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.H_attack)
        {
            soundSource.clip = Audioclip[1];
            soundSource.PlayOneShot(Audioclip[1]);
            if (M_S_T == Maskman_ani_Setting.special_state.Floating)
            {
                floatingHit();
            }
            // 어퍼컷!         
            if (Guard_ani_Setting.G_S_T == Guard_ani_Setting.special_state.upper && 
                M_S_T != Maskman_ani_Setting.special_state.Floating)
            {
                M_S_T = Maskman_ani_Setting.special_state.Floating;
                ani.SetTrigger("Upperhit");
                isfloating = true;
                rb.useGravity = true;
                rb.AddForce(Vector3.up * 7, ForceMode.Impulse);

                // 공중 콤보를 위한 마스크맨의 hitcollider을 회전시키기 위해 가져옴
                // floating collider 90도 회전
                StartCoroutine(FloatingColliderdelay());
                 //Debug.Log(transform.Find("M_Wall_hit").transform.rotation);
                ani.SetBool("Floating", true);
            }
           

        }
        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.K_attack)
        {
         
            if (M_S_T == Maskman_ani_Setting.special_state.Floating)
            {
                
                floatingHit();
            }
            soundSource.clip = Audioclip[4];
            soundSource.PlayOneShot(Audioclip[4]);
        }
    }

    // 중단 맞는 애니메이션
    void Middle_hit()
    {
        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.H_attack)
        {
            soundSource.clip = Audioclip[2];
            soundSource.PlayOneShot(Audioclip[2]);

            // 공중에 떠있을때
            if (M_S_T == Maskman_ani_Setting.special_state.Floating)
            {
              
                floatingHit();
            }

            // 어퍼컷! 
            if (Guard_ani_Setting.G_S_T == Guard_ani_Setting.special_state.upper &&
                M_S_T != Maskman_ani_Setting.special_state.Floating)
            {
                //Debug.Log("실행");
                M_S_T = Maskman_ani_Setting.special_state.Floating;

                ani.SetTrigger("Upperhit");
                isfloating = true;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
                

                // floating collider 90도 회전
                StartCoroutine(FloatingColliderdelay());
                //gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward * 10, ForceMode.Impulse);
                ani.SetBool("Floating", true);
            }

            // 공중에 떠 있을때
           

        }
        if (Guard_ani_Setting.G_A_T == Guard_ani_Setting.ani_state.K_attack)
        {
            Debug.Log(M_A_T);
            if (M_S_T == Maskman_ani_Setting.special_state.Floating)
            {
                Debug.Log("중단");
                floatingHit();
            }
            soundSource.clip = Audioclip[5];
            soundSource.PlayOneShot(Audioclip[5]);
        }
    }

    // 하단 맞는 애니메이션
    void Low_hit()
    {
        Debug.Log("머리");
    }

    void Start()
    {
        M_A_T = ani_state.idle;
        ani = gameObject.GetComponent<Animator>();
        soundSource = gameObject.GetComponent<AudioSource>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // 보류
    public IEnumerator Knockback(float dur, float power)
    {
        float timer = 0f;
        int writeCall = 0;

        /*넉백을 드디어 해결했다*/
        /*어디에도 이걸 안알려주네...*/
        while (timer <= dur)
        {
            timer += Time.deltaTime;
            writeCall += 1;
            rb.AddRelativeForce(new Vector3(0f, 0, 1.2f * power));
        }
        //Debug.Log(writeCall);
        yield return 0;
    }

    void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.P)) 
        {
            rb.velocity = new Vector3(0,0,0);
            StartCoroutine(Knockback(1f, 2f));
        }

        if (GameManager.Gs == GameManager.Gamesetting.GameStart)
        {
            // idle 일때만 lookat 컴포넌트 켜짐
            if (M_A_T == ani_state.idle ||
            M_A_T == ani_state.sit ||
            M_A_T == ani_state.forwardstep ||
            M_A_T == ani_state.backstep)
                La.enabled = true;
            else if(M_A_T == ani_state.H_attack || M_A_T == ani_state.K_attack)
                La.enabled = false;


            // idle 상황일때 ani_state => idle
            if (ani.GetCurrentAnimatorStateInfo(0).IsName("idle"))
            {
                M_A_T = ani_state.idle;
                transform.transform.Find("M_Wall_hit").GetComponent<BoxCollider>().enabled = true;
            }
            // ani.state => sit
            if (ani.GetCurrentAnimatorStateInfo(0).IsName("sit"))
            {
                M_A_T = ani_state.sit;
            }
            // ani.state => fwd
            if (ani.GetCurrentAnimatorStateInfo(0).IsName("fwd"))
            {
                M_A_T = ani_state.forwardstep;
            }
            // ani.state => fwd
            if (ani.GetCurrentAnimatorStateInfo(0).IsName("bwd"))
            {
                M_A_T = ani_state.backstep;
            }
            if (ani.GetCurrentAnimatorStateInfo(0).IsName("Lay"))
            {
                M_A_T = ani_state.lay;
                transform.transform.Find("M_Wall_hit").GetComponent<BoxCollider>().enabled = false;

            }

            // 마스크맨 히트박스
            if (M_A_T == ani_state.lay || M_S_T==special_state.Floating)
            {                
                transform.Find("M_Wall_hit").transform.localEulerAngles = new Vector3(-90, 0, 0);
                transform.Find("M_Wall_hit").transform.localPosition = new Vector3(0, 0, 0.3f);
            }
            else
            {
                transform.Find("M_Wall_hit").transform.localEulerAngles = new Vector3(0, 0, 0);
                transform.Find("M_Wall_hit").transform.localPosition = new Vector3(0, 0, 0);
            }

            // 공격 모션일때 콜라이더가 들어있는 게임 오브젝트가 켜짐.
            if (M_A_T == ani_state.H_attack && !ani.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
            {
                Hand_R.SetActive(true);
                Hand_L.SetActive(true);
            }
            // 대쉬하고 발공격할때 콜라이더 켜지는거 수정
            else if (M_A_T == ani_state.K_attack && !ani.GetCurrentAnimatorStateInfo(0).IsName("Dash"))
            {
                kick_L.SetActive(true);
                kick_R.SetActive(true);
            }
            else
            {
                Hand_R.SetActive(false);
                Hand_L.SetActive(false);
                kick_L.SetActive(false);
                kick_R.SetActive(false);

            }


            // 방어 모션
            if (M_A_T == ani_state.backstep)
                ani.SetBool("Hit_possible", false);

            else
                ani.SetBool("Hit_possible", true);

            // 이동 공격 모션
            WalkFwd();
            WalkBwd();
            sit();
            jump();
            R_jab(); L_jab(); R_kick(); L_kick();
        }
        else
            M_A_T = ani_state.idle;

        // ko 상황일떄 콜라이더 켜져있는 버그 수정
        if (GameManager.Gs == GameManager.Gamesetting.KO)
        {
            Hand_R.SetActive(false);
            Hand_L.SetActive(false);
            kick_L.SetActive(false);
            kick_R.SetActive(false);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // 콜라이더 각도 설정한거 초기화
            
            isfloating = false;
            ani.SetBool("Floating", false);
            // 콜라이더 포지션 수정한것 초기화
          
            
            M_S_T = Maskman_ani_Setting.special_state.idle;

        }
    }


    // 애니메이션 events
    void False_dash()
    {
        ani.SetBool("Dash", false);
    }
    void False_backstep()
    {
        ani.SetBool("BackStep", false);
    }

    // 이동
    void WalkFwd()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (D_flag)
                ani.SetBool("Dash", true);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            M_A_T = ani_state.forwardstep;
            ani.SetBool("walkfwd", true);
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            D_flag = true;
            StartCoroutine(Delay());
            ani.SetBool("walkfwd", false);
        }
    }
    void WalkBwd()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (B_flag)
            {
                ani.SetBool("BackStep", true);
            }
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {

            ani.SetBool("walkbwd", true);
        }
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            B_flag = true;

            StartCoroutine(Delay());
            ani.SetBool("walkbwd", false);
        }
    }
    void sit()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (S_flag)
                ani.SetBool("siderwd", true);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            ani.SetBool("sit", true);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            S_flag = true;

            StartCoroutine(Delay());
            ani.SetBool("sit", false);
        }
    }
    void jump()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ani.SetBool("jump", true);
            if (J_flag)
                ani.SetBool("sidelwd", true);
            J_flag = true;
            StartCoroutine(Delay());
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            ani.SetBool("jump", false);
        }


    }

    IEnumerator ExecuteForDuration(float duration, ani_state state)
    {
        float timer = 0f;

        while (timer < duration)
        {
            // 일정 시간 동안 실행될 코드

            M_A_T = state;
            // 시간 업데이트
            timer += Time.deltaTime;

            yield return null;
        }
    }
        // 공격
    void R_jab()
    {
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            ani.SetBool("Jab_R", true);
            StartCoroutine(ExecuteForDuration(0.2f, ani_state.H_attack));
            StartCoroutine(delay("Jab_R"));
        }
    }
    void L_jab()
    {
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            StartCoroutine(ExecuteForDuration(0.2f, ani_state.H_attack));
            ani.SetBool("Jab_L", true);
            StartCoroutine(delay("Jab_L"));
        }
    }
    void R_kick()
    {
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            ani.SetBool("Kick_R", true);
            StartCoroutine(ExecuteForDuration(0.2f, ani_state.K_attack));
            StartCoroutine(delay("Kick_R"));
        }
    }
    void L_kick()
    {
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            ani.SetBool("Kick_L", true);
            StartCoroutine(ExecuteForDuration(0.2f, ani_state.K_attack));
            StartCoroutine(delay("Kick_L"));
        }
    }

}
