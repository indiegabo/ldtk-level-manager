using System;
using HandyFSM;
using Sirenix.OdinInspector;
using UnityEngine;

public class Child : GenericHandyMachine<ChildState, ChildStateIdle>
{
    [BoxGroup("Test")]
    [SerializeField]
    private float _testFloat;
}
