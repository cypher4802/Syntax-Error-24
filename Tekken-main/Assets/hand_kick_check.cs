using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hand_kick_check : StateMachineBehaviour
{

    public bool Guard;
    public bool maskman;
    public bool hand;
    public bool kick;

    

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Guard && hand)    Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.H_attack;      
        else if (Guard && kick)  Guard_ani_Setting.G_A_T = Guard_ani_Setting.ani_state.K_attack;

        if (maskman && hand) Maskman_ani_Setting.M_A_T = Maskman_ani_Setting.ani_state.H_attack;
        else if (maskman && kick) Maskman_ani_Setting.M_A_T = Maskman_ani_Setting.ani_state.K_attack;
    }
}
