using Cysharp.Threading.Tasks;
using LDtkLevelManager.Cartography;
using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    public class WorldMap : MonoBehaviour
    {
        [SerializeField] private Project _project;

        private Cartographer _cartographer;
        private WorldInfo _currentWorld;

        private LdtkJson _projectJson;

        private void Awake()
        {
            _cartographer = Cartographer.ForProject(_project);
            if (!ProjectService.Instance.TryGetLdtkJson(_project, out _projectJson))
            {
                Logger.Error($"Failed to load LDtkJson for project {_project.name}.", this);
                return;
            }

            World world = _projectJson.Worlds[0];
            SetWorld(world);
        }

        public void SetWorld(string worldName)
        {

        }

        public void SetWorld(World world)
        {
            _cartographer.TryWorldCartography(world, out WorldCartography worldCartography);


        }
    }
}