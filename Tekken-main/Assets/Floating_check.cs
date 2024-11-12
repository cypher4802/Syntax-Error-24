using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating_check : StateMachineBehaviour
{
    public bool guard;
    public bool maskman;
    public bool floatingState;
    public GameObject Maskman;
    float po_X;
    float po_Z;
    Vector3 currentPosition;
    float minX = 1;
    float minZ = 1;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    { 
        po_X = animator.transform.position.x;
        po_Z = animator.transform.position.z;

    }
    // floating 상태일 때 lookat 컴포넌트 비활성화 하기 위한 설정
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        Vector3 Po = animator.transform.position;
        Po.x = po_X;
        Po.z = po_Z;
        animator.transform.position = Po;
      
        if (guard)
        {
            floatingState = true;
            Guard_ani_Setting.G_S_T = Guard_ani_Setting.special_state.Floating;
        }
        if (maskman)
        {
            floatingState = true;
            Maskman_ani_Setting.M_S_T = Maskman_ani_Setting.special_state.Floating;
        }
    }

    // 애니메이션 종료 후 끝내기
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (guard)
        {
            floatingState = false;
            Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.idle;
        }
        if (maskman) floatingState = false;
        {
            floatingState = false;
            Maskman_ani_Setting.M_A_T = Maskman_ani_Setting.ani_state.idle;
        }
    }
}
