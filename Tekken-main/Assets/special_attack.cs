using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class special_attack : StateMachineBehaviour
{
    public bool Guard;
    public bool Maskman;

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(Guard)
        {
            Guard_ani_Setting.G_S_T = Guard_ani_Setting.special_state.specialattack;
        }
        if(Maskman)
        {
            Maskman_ani_Setting.M_S_T = Maskman_ani_Setting.special_state.specialattack;
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Maskman_ani_Setting.M_S_T = Maskman_ani_Setting.special_state.idle;
        Guard_ani_Setting.G_S_T = Guard_ani_Setting.special_state.idle;
    }
}
