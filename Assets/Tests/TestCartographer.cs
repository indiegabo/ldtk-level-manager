using LDtkLevelManager;
using LDtkLevelManager.Cartography;
using UnityEngine;

public class TestCartographer : MonoBehaviour
{
    public Project project;

    private void Awake()
    {
        Cartographer cartographer = Cartographer.ForProject(project);
        Debug.Log(cartographer.PixelsPerUnit);
        Debug.Log(cartographer.ScaleFactor);
        cartographer.LogWorlds();
    }
}