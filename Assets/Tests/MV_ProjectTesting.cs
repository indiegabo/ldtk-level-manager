using LDtkVania;
using UnityEditor;
using UnityEngine;

public class MV_ProjectTesting
{
    [MenuItem("LDtkVania/Project Test")]
    static void DoSomething()
    {
        if (MV_Project.Instance == null) return;
    }
}