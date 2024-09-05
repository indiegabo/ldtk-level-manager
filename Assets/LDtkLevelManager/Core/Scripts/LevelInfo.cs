using LDtkUnity;
using UnityEngine;

namespace LDtkLevelManager
{
    /// <summary>
    /// Represents all the information the project needs to know about a level.
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
        [SerializeField] private string _worldName;
        [SerializeField] private string _areaName;
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

        public Project Project => _project;
        public string Iid => _iid;
        public string Name => !string.IsNullOrEmpty(_displayName) ? _displayName : name;
        public string WorldName => _worldName;
        public string AreaName => _areaName;
        public Object Asset => _asset;
        public bool LeftBehind => _leftBehind;

        public bool HasScene => _scene != null && !string.IsNullOrEmpty(_scene.AddressableKey);
        public LevelScene Scene => _scene;

        public string AssetPath => _assetPath;
        public string Address => _address;

        // LDtk
        public LDtkLevelFile LevelFile => _levelFile;
        public Level LDtkLevel
        {
            get
            {
#if UNITY_EDITOR
                return _levelFile.FromJson;
#else
                return _ldtkLevel ??= _levelFile.FromJson;
#endif
            }
        }

        #endregion

        #region Constructors

        public void Initialize(LevelProcessingData data)
        {
            _project = data.project;
            _iid = data.iid;
            UpdateInfo(data);
        }

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
            }

            _assetPath = data.assetPath;
            _address = data.address;
            _asset = data.asset;
            _levelFile = data.ldtkFile;

            if (data.world != null)
            {
                _worldName = data.world.Identifier;
            }
        }

        #endregion

        #region Scene

        public void SetScene(LevelScene levelScene)
        {
            _scene = levelScene;
        }

        public void ClearScene()
        {
            _scene = null;
        }

        #endregion

        #region Leaving Behind

        public void SetLeftBehind(bool leftBehind)
        {
            _leftBehind = leftBehind;
        }

        #endregion
    }
}