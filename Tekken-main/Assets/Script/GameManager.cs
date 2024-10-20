using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using DG.Tweening;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum Gamesetting
    {
        GameStart,
        Ready,
        Loding,
        GameOver,
        KO
    }
    static public Gamesetting Gs;
    static public int a = 2;
    public TextMeshProUGUI time_text;    // 시간 체크
    public UnityEngine.UI.Text ready_text;
    public UnityEngine.UI.Text fight_text;
    public TextMeshProUGUI TimeUp_text;
    public TextMeshProUGUI ko_text;
    public TextMeshProUGUI round1;
    public TextMeshProUGUI[] Round_text;
    public float time = 50;


    public int startFontSize ;
    public int endFontSize;
    public float scaleDuration = 1f;
    public float fadeOutTime = 1f;
    // 사운드
    public AudioClip[] Audioclip;
    AudioSource soundSource;

    public void ready_dotween()
    {
        ready_text.enabled = true;

        // 초기 크기와 알파 값 설정
        ready_text.fontSize = (int)startFontSize;
        ready_text.color = Color.red;

        // 텍스트 확대
        ready_text.rectTransform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutBack);

        // 텍스트 크기 보간
        DOTween.To(() => ready_text.fontSize, value => ready_text.fontSize = (int)value, endFontSize, scaleDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 페이드 아웃
                ready_text.DOFade(0f, fadeOutTime).OnComplete(() => ready_text.enabled = false);
            });
    }


    public void fight_dowteen()
    {

        fight_text.enabled = true;

        // 초기 폰트 사이즈와 알파 값 설정

        fight_text.fontSize = (int)startFontSize;
        fight_text.color = Color.red;

        // 텍스트 확대
        fight_text.rectTransform.DOScale(Vector3.one, scaleDuration).SetEase(Ease.OutQuint);

        // 텍스트 크기 보간
        DOTween.To(() => fight_text.fontSize, value => fight_text.fontSize = (int)value, endFontSize, scaleDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 페이드 아웃
                fight_text.DOFade(0f, fadeOutTime).OnComplete(() => fight_text.enabled = false);
            });
    }



    IEnumerator Fight()
    {
        yield return new WaitForSeconds(2f);
        Round_text[RoundCheckManager.Check_round].enabled = false; // 라운드 체크함수 가져옴
        ready_dotween();
        yield return new WaitForSeconds(0.5f);

        fight_dowteen();
        soundSource.clip = Audioclip[3];    // fight 오디오 클립
        soundSource.PlayOneShot(Audioclip[3]);
        yield return new WaitForSeconds(0.5f);
       
        RoundCheckManager.Check_round++;
        Gs = Gamesetting.GameStart;
    }

    IEnumerator round()
    {
        yield return new WaitForSeconds(0.5f);
        // round sound
        Round_text[RoundCheckManager.Check_round].enabled = true;  // 라운드 체크함수 가져옴      
        soundSource.PlayOneShot(Audioclip[RoundCheckManager.Check_round]);
        StartCoroutine(Fight());
    }

    IEnumerator delay(TextMeshProUGUI TMP)
    {
        TMP.enabled = true;
        yield return new WaitForSeconds(2f);
        Time.timeScale = 1f;
        TMP.enabled = false;

        SceneManager.LoadScene(2);
    }

    private void Awake()
    {
        // 라운드 체크
        // guard win 승리모션
        if (RoundCheckManager.instance.rc1 && RoundCheckManager.instance.rc2)
        {
            SceneManager.LoadScene(3);
        }
        // maskman win 승리모션
        if (RoundCheckManager.instance.rc3 && RoundCheckManager.instance.rc4)
        {
            SceneManager.LoadScene(4);
        }
    }

    private void Start()
    {
        flag = true;
        soundSource = GetComponent<AudioSource>();
        Gs = Gamesetting.Loding;
        StartCoroutine(round());
    }
    bool flag;
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.KeypadEnter))    // 임의로
        {
            SceneManager.LoadScene(2);
        }
        if (Gs == Gamesetting.GameStart)
        {

            time -= Time.deltaTime;

            int b = (int)time;  //  명시적 형변환
            time_text.text = b.ToString();   // 문자열로 형변환 int -> string
        }

        // time out 상황시
        if (time <= 0)
        {
            time = 0;
            StartCoroutine(delay(TimeUp_text));
        }
        // Ko 상태일때
        if (M_HP.m_hp.hp <= 0 || G_HP.g_hp.hp <= 0)
        {
            Gs = Gamesetting.KO;

            if (flag)
            {
                Time.timeScale = 0.2f;
                soundSource.PlayOneShot(Audioclip[4]);
                flag = false;
            }
            StartCoroutine(delay(ko_text));
        }
    }
}
