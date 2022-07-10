using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMotion : StateMachineBehaviour
{
    Enemy.AnimatorParameters parameters;
    public bool haveParameterController;
    public string param;
    public bool how_in;
    public bool how_out;


    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(!haveParameterController)
            animator.SetBool(param, how_in);
        else
            parameters.SetBool(param, how_in);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(!haveParameterController)
            animator.SetBool(param, how_out);
        else
            parameters.SetBool(param, how_out);
    }
    public void SetParameters(Enemy.AnimatorParameters parameters)
    {
        this.parameters = parameters;
        haveParameterController = true;
    }
}
