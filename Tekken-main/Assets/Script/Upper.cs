using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upper : MonoBehaviour
{
    IEnumerator ExecuteForDuration(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            // 일정 시간 동안 실행될 코드

           // 이름에 가드가 포함되면 가드의 상태를 어퍼컷으로 바꾼다.
            if (gameObject.name.Contains("Guard"))
            {
                Guard_ani_Setting.G_S_T = Guard_ani_Setting.special_state.upper;              
            }

            if (gameObject.name.Contains("Maskman"))
            {
                Maskman_ani_Setting.M_S_T = Maskman_ani_Setting.special_state.upper;
            //    Debug.Log("maskman");
            }

            // 시간 업데이트
            timer += Time.deltaTime;
            
            yield return null;           
        }
        
        // 어퍼컷 이후에 hand attack 할 때 어퍼컷되는 것 방지.
        yield return new WaitForSeconds(0.02f);
        Guard_ani_Setting.G_S_T = Guard_ani_Setting.special_state.idle;
        Maskman_ani_Setting.M_S_T = Maskman_ani_Setting.special_state.idle;
    }

    // 어퍼컷에 있는 애니메이션 이벤트
    void Upper_check()
    {
        StartCoroutine(ExecuteForDuration(0.5f));
    }
}
