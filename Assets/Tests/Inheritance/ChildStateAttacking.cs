using System.Collections;
using System.Collections.Generic;
using HandyFSM;
using UnityEngine;

[CreateAssetMenu(fileName = "Attacking State", menuName = "FSM/Attacking State")]
public class ChildStateAttacking : ScriptableState
{

    public void OnEnter()
    {
        Machine.StartCoroutine(WaitAndIdle());
    }

    public void OnExit()
    {
    }

    private IEnumerator WaitAndIdle()
    {
        yield return new WaitForSeconds(0.25f);
        Machine.EndState(Machine.GetState<ChildStateIdle>());
    }
}
