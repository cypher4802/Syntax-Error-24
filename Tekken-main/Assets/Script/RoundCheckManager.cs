using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoundCheckManager : MonoBehaviour
{ 
    static public RoundCheckManager instance = null;

    public bool rc1 = false;
    public bool rc2 = false;
    public bool rc3 = false;
    public bool rc4 = false;

    static public int Check_round = 0;    // gamemanager에서 사용
    
    // Update is called once per frame
    private void Awake()
    {
        
        // Sound Manager 인스턴스가 이미 있는지 확인, 이 상태로 설정
        if (instance == null)
            instance = this;

        // 인스턴스가 이미 있는 경우 오브젝트 제거
        else if (instance != this)
            Destroy(gameObject);

        // 이렇게 하면 다음 scene으로 넘어가도 오브젝트가 사라지지 않습니다.
        DontDestroyOnLoad(gameObject);      
    }
    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex < 2)
            gameObject.GetComponent<AudioSource>().enabled = false;
        else
            gameObject.GetComponent<AudioSource>().enabled = true;
    }

}
