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

        [SerializeField] private string _iid;
        [SerializeField] private string _displayName;
        [SerializeField] private string _world;
        [SerializeField] private string _area;
        [SerializeField] private Object _asset;
        [SerializeField] private LDtkLevelFile _levelFile;
        [SerializeField] private string _assetPath;
        [SerializeField] private string _address;
        [SerializeField] private MV_LevelScene _scene;

        #endregion

        #region Field

        private Level _ldtkLevel;

        #endregion

        #region Getters

        public string Iid => _iid;
        public string Name => !string.IsNullOrEmpty(_displayName) ? _displayName : name;
        public string Area => _area;
        public Object Asset => _asset;

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
                _ldtkLevel ??= _levelFile.FromJson;
                return _ldtkLevel;
            }
        }

        #endregion

        #region Constructors

        public void Initialize(MV_LevelProcessingData data)
        {
            _iid = data.iid;
            UpdateInfo(data);
        }

        #endregion

        #region Gathering info

        public void UpdateInfo(MV_LevelProcessingData data)
        {
            LDtkFields fields = data.ldtkComponentLevel.GetComponent<LDtkFields>();

            string displayName = fields.GetString("displayName");
            if (!string.IsNullOrEmpty(displayName))
                name = displayName;

            string area = fields.GetValueAsString("ldtkVaniaArea");
            if (!string.IsNullOrEmpty(area))
                _area = area;

            _assetPath = data.assetPath;
            _address = data.address;
            _asset = data.asset;
            _levelFile = data.ldtkFile;
        }

        #endregion

        #region Scene

        public void SetScene(MV_LevelScene levelScene)
        {
            _scene = levelScene;
        }

        #endregion
    }
}