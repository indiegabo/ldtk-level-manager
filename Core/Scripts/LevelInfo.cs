using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    /// <summary>
    /// Represents all the information the <see cref="LevelLoader"/> needs to know about a level, including
    /// its IID, name, world, area, and addressable address. The project will use this
    /// information to generate the level's addressable address and to keep track of the
    /// level's name and world. The project will also use this information to set the
    /// level as addressable and to enforce the addressable address.
    /// </summary>
    public class LevelInfo : ScriptableObject
    {
        #region Static

        public static readonly string AdressableAddressPrexix = "LM_";
        public static readonly string AddressableGroupName = "LM_Levels";
        public static readonly string AddressableLabel = "LM_Level";

        #endregion

        #region Inspector

        [SerializeField] private Project _project;
        [SerializeField] private string _iid;
        [SerializeField] private string _displayName;
        [SerializeField] private string _worldIid;
        [SerializeField] private string _worldName;
        [SerializeField] private string _areaName;
        [SerializeField] private bool _standAlone;
        [SerializeField] private Object _asset;
        [SerializeField] private LDtkLevelFile _levelFile;
        [SerializeField] private string _assetPath;
        [SerializeField] private string _address;
        [SerializeField] private LevelScene _scene;
        [SerializeField] private bool _leftBehind;

        #endregion

        #region Fields

        #endregion

        #region Getters

        /// <summary>
        /// The project this level belongs to.
        /// </summary>
        public Project Project => _project;

        /// <summary>
        /// The IID of the level in the LDtk project.
        /// </summary>
        public string Iid => _iid;

        /// <summary>
        /// The display name of the level if it has one, otherwise the level's name.
        /// </summary>
        public string Name => !string.IsNullOrEmpty(_displayName) ? _displayName : name;

        /// <summary>
        /// The world of the level.
        /// </summary>
        public string WorldIid => _worldIid;

        /// <summary>
        /// The world of the level.
        /// </summary>
        public string WorldName => _worldName;

        /// <summary>
        /// The area of the level.
        /// </summary>
        public string AreaName => _areaName;

        /// <summary>
        /// Whether the level is standalone.
        /// </summary>
        public bool StandAlone => _standAlone;

        /// <summary>
        /// The asset that represents the level.
        /// </summary>
        public Object Asset => _asset;

        /// <summary>
        ///  Whether the level is left behind. Meaning that its LDtk level file was not found in the project.
        /// </summary>
        public bool LeftBehind => _leftBehind;

        /// <summary>
        /// Whether the level is wrapped in a scene.
        /// </summary>
        public bool WrappedInScene => _scene != null && !string.IsNullOrEmpty(_scene.AddressableKey);

        /// <summary>
        /// The scene information of the level if it has one.
        /// </summary>
        public LevelScene SceneInfo => _scene;

        /// <summary>
        /// The path of the asset in the Unity project.
        /// </summary>
        public string AssetPath => _assetPath;

        /// <summary>
        /// The address of the asset in the Unity project.
        /// </summary>
        public string Address => _address;

        /// <summary>
        /// The LDtk level file.
        /// </summary>
        public LDtkLevelFile LevelFile => _levelFile;

        /// <summary>
        /// Gets the LDtk level.
        /// </summary>
        public Level LDtkLevel
        {
            get
            {
#if UNITY_EDITOR
                return _levelFile.FromJson;
#else
                if (_ldtkLevel == null)
                {
                    _ldtkLevel = _levelFile.FromJson;
                }                
                return _ldtkLevel;
#endif
            }
        }

        #endregion

#if UNITY_EDITOR

        #region Information

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Initializes the <see cref="LevelInfo"/> from the given <see cref="LevelProcessingData"/>.
        /// This method is called automatically when the <see cref="LDtkLevelManager.Project"/> is initialized.
        /// </summary>
        /// <param name="data">The <see cref="LevelProcessingData"/> to initialize the level from.</param>
        public void Initialize(LevelProcessingData data)
        {
            _project = data.project;
            _iid = data.iid;
            UpdateInfo(data);
        }

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Updates the <see cref="LevelInfo"/> from the given <see cref="LevelProcessingData"/>.
        /// This method is called automatically when the <see cref="LDtkLevelManager.Project"/> is reinitialized.
        /// </summary>
        /// <param name="data">The <see cref="LevelProcessingData"/> to update the level from.</param>
        public void UpdateInfo(LevelProcessingData data)
        {
            name = data.ldtkFile.name;
            _areaName = null;

            if (data.ldtkComponentLevel.TryGetComponent(out LDtkFields fields))
            {
                string displayName = fields.GetString("displayName");
                if (!string.IsNullOrEmpty(displayName))
                {
                    name = displayName;
                }

                string area = fields.GetValueAsString("area");
                if (!string.IsNullOrEmpty(area))
                    _areaName = area;

                _standAlone = fields.GetBool("standAlone");
            }

            _assetPath = data.assetPath;
            _address = data.address;
            _asset = data.asset;
            _levelFile = data.ldtkFile;

            if (data.world != null)
            {
                _worldIid = data.world.Iid;
                _worldName = data.world.Identifier;
            }
        }

        #endregion

        #region Scene

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Sets the scene for this level to the given <see cref="LevelScene"/>.
        /// </summary>
        /// <param name="levelScene">The <see cref="LevelScene"/> to set.</param>
        public void SetScene(LevelScene levelScene)
        {
            _scene = levelScene;
        }

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Clears the scene for this level.
        /// </summary>
        public void ClearScene()
        {
            _scene = null;
        }

        #endregion

        #region Leaving Behind

        /// <summary>
        /// [Editor Only] <br/><br/>
        /// Sets a flag indicating whether the level was left behind by the user.
        /// A level is considered "left behind" if its LDtk level file was not found in the project.
        /// </summary>
        /// <param name="leftBehind">true if the level was left behind; false otherwise</param>
        public void SetLeftBehind(bool leftBehind)
        {
            _leftBehind = leftBehind;
        }

        #endregion
#endif
    }
}