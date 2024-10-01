using Cysharp.Threading.Tasks;

namespace LDtkLevelManager
{
    public class UnrelatedLevelLoader : LevelLoader
    {
        /// <summary>
        /// Loads a level by its LDtk Iid. If the level is not present in the project, <br />
        /// an error will be logged and no action will be taken. <br />
        /// </summary>
        /// <param name="iid">The LDtk Iid of the level to load.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is loaded.</returns>
        public override async UniTask LoadLevel(string iid)
        {
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                Logger.Error($"Level under LDtk Iid {iid} not present in project {_project.name}", this);
                return;
            }

            await LoadLevel(level);
        }

        /// <summary>
        /// Loads a level by its <see cref="LevelInfo"/>. If the level is not present in the project, <br />
        /// an error will be logged and no action will be taken. <br />
        /// </summary>
        /// <param name="level">The <see cref="LevelInfo"/> of the level to load.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the level is loaded.</returns>
        public override async UniTask LoadLevel(LevelInfo level)
        {
            if (!level.WrappedInScene)
            {
                await LoadLevelObjectAsync(level);
            }
            else
            {
                await LoadLevelSceneAsync(level);
            }
        }

        /// <summary>
        /// Unloads a level by its LDtk Iid. If the level is not present in the project, <br />
        /// an error will be logged and no action will be taken. <br />
        /// </summary>
        /// <param name="iid">The LDtk Iid of the level to unload.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the unload operation is complete.</returns>
        public async UniTask UnloadLevel(string iid)
        {
            if (!TryGetLevel(iid, out LevelInfo level))
            {
                Logger.Error($"Level under LDtk Iid {iid} not present in project {_project.name}", this);
                return;
            }

            await UnloadAsync(level);
        }

        /// <summary>
        /// Unloads a level by its <see cref="LevelInfo"/>. If the level is not present in the project, <br />
        /// an error will be logged and no action will be taken. <br />
        /// </summary>
        /// <param name="level">The <see cref="LevelInfo"/> of the level to unload.</param>
        /// <returns>A <see cref="UniTask"/> that completes when the unload operation is complete.</returns>
        public async UniTask UnloadLevel(LevelInfo level)
        {
            await UnloadAsync(level);
        }
    }
}