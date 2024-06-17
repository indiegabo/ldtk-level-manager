using LDtkUnity;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LDtkVania
{
    public class MV_Level : ScriptableObject
    {
        #region Static

        public static readonly string AdressableAddressPrexix = "LDtkVaniaLevel";
        public static readonly string AddressableGroupName = "LDtkVaniaLevels";
        public static readonly string AddressableLabel = "LDtkVaniaLevel";

        #endregion

        #region Inspector

        [SerializeField] private MV_Project _project;
        [SerializeField] private string _iid;
        [SerializeField] private string _displayName;
        [SerializeField] private string _worldName;
        [SerializeField] private string _areaName;
        [SerializeField] private Object _asset;
        [SerializeField] private LDtkLevelFile _levelFile;
        [SerializeField] private string _assetPath;
        [SerializeField] private string _address;
        [SerializeField] private MV_LevelScene _scene;
        [SerializeField] private bool _leftBehind;

        #endregion

        #region Field

        private Level _ldtkLevel;

        #endregion

        #region Getters

        public MV_Project Project => _project;
        public string Iid => _iid;
        public string Name => !string.IsNullOrEmpty(_displayName) ? _displayName : name;
        public string WorldName => _worldName;
        public string AreaName => _areaName;
        public Object Asset => _asset;
        public bool LeftBehind => _leftBehind;

        public bool HasScene => _scene != null && !string.IsNullOrEmpty(_scene.AddressableKey);
        public MV_LevelScene Scene => _scene;

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

        public void Initialize(MV_LevelProcessingData data)
        {
            _project = data.project;
            _iid = data.iid;
            UpdateInfo(data);
        }

        public void UpdateInfo(MV_LevelProcessingData data)
        {
            LDtkFields fields = data.ldtkComponentLevel.GetComponent<LDtkFields>();

            string displayName = fields.GetString("displayName");

            if (!string.IsNullOrEmpty(displayName))
            {
                name = displayName;
            }
            else
            {
                name = data.ldtkFile.name;
            }

            string area = fields.GetValueAsString("ldtkVaniaArea");
            if (!string.IsNullOrEmpty(area))
                _areaName = area;

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

        public void SetScene(MV_LevelScene levelScene)
        {
            _scene = levelScene;
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