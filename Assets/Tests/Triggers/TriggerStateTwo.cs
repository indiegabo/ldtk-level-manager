using System.Collections;
using System.Collections.Generic;
using HandyFSM;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TriggerStateTwo", menuName = "FSM/Triggers/StateTwo")]
public class TriggerStateTwo : ScriptableState
{
    public void OnInit()
    {

    }

    public void OnEnter()
    {
        Machine.RegisterOnTrigger("tap", GoToOne);
    }

    public void OnExit()
    {
        Machine.UnregisterOnTrigger("tap", GoToOne);
    }

    private void GoToOne()
    {
        Debug.Log($"GoToOne - {Random.Range(0, 100)}");
        Machine.EndState(Machine.GetState<TriggerStateOne>());
    }
}