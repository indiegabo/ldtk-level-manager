using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildStateIdle : ChildStateGrounded
{
    protected void OnInit()
    {
        SetInterruptible(true);
        SetName("Idle");
    }

    public void OnEnter()
    {
        Machine.StartCoroutine(WaitAndRun());
    }

    public void OnExit()
    {

    }

    public void OnTick()
    {
    }

    public void OnFixedTick()
    {
    }

    public void OnLateTick()
    {
    }

    private IEnumerator WaitAndRun()
    {
        yield return new WaitForSeconds(0.25f);
        Machine.EndState(Machine.GetState<ChildStateRunnning>());
    }
}
