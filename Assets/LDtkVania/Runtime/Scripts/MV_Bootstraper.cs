using UnityEngine;

namespace LDtkVania
{
    public static class MV_Bootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Bootstrap()
        {
            GameObject go = new("Metroidvania Level Manager");
            MV_LevelManager manager = go.AddComponent<MV_LevelManager>();
            // manager.Initialize(MV_Project.Instance);
        }
    }
}