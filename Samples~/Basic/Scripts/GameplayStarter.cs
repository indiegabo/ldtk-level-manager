using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    public class GameplayStarter : MonoBehaviour
    {
        #region Inspector

        [SerializeField] private LevelInfo _initialLevel;
        [SerializeField] private Player _player;
        [SerializeField] private LevelTransitioner _levelTransitioner;

        #endregion

        #region Fields

        private LevelLoader _levelLoader;

        #endregion

        #region Behaviour

        private void Awake()
        {
            _levelLoader = LevelLoader.Instance;
        }

        /// <summary>
        /// Instantiates the player and loads a level.
        /// </summary>
        private void Start()
        {
            Player player = Instantiate(_player);
            player.gameObject.name = "Player";

            _ = LoadStandAloneLevel();
        }

        #endregion

        #region Loading

        /// <summary>
        /// Loads a level, prepares it, opens the curtains and enters it.
        /// </summary>
        /// <returns>A UniTask that completes when the level is entered.</returns>
        private async UniTask LoadStandAloneLevel()
        {
            // Load the level and its neighbours. At the end of this task
            // the level will be fully loaded and can rely on its neighbours
            // being loaded as well
            await _levelLoader.LoadUniverseLevel(_initialLevel.Iid);

            // Prepare the level. This sets the player in the loading spot. 
            // Notice that at this time the curtains are not yet open.
            _levelLoader.Prepare(_initialLevel.Iid);

            // Open the curtains
            await _levelTransitioner.OpenCurtains();

            // Enter the level. Meaning the player is now in control
            // of the player character
            _levelLoader.Enter();
        }

        #endregion
    }
}
