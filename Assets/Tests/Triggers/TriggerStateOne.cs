using System.Collections;
using System.Collections.Generic;
using HandyFSM;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TriggerStateOne", menuName = "FSM/Triggers/StateOne")]
public class TriggerStateOne : ScriptableState
{
    public void OnInit()
    {

    }

    public void OnEnter()
    {
        Machine.RegisterOnTrigger("tap", GoToTwo);
    }

    public void OnExit()
    {
        Machine.UnregisterOnTrigger("tap", GoToTwo);
    }

    private void GoToTwo()
    {
        Debug.Log($"GoToTwo - {Random.Range(0, 100)}");
        Machine.EndState(Machine.GetState<TriggerStateTwo>());
    }
}