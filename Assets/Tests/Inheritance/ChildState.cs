using System.Collections;
using System.Collections.Generic;
using HandyFSM;
using UnityEngine;

public class ChildState : State
{
    protected Child ChildMachine => Machine.As<Child>();
}
