using LDtkUnity;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LDtkVania
{
    public class MV_Level : ScriptableObject
    {
        #region Inspector

        [SerializeField] private string _iid;
        [SerializeField] private string _displayName;
        [SerializeField] private string _world;
        [SerializeField] private string _area;
        [SerializeField] private Object _asset;
        [SerializeField] private LDtkLevelFile _levelFile;
        [SerializeField] private string _assetPath;
        [SerializeField] private string _addressableKey;
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
        public string AddressableKey => _addressableKey;

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

        public void Initialize(LdtkJson projectJSON, LDtkComponentLevel ldtkComponentLevel, IResourceLocation location, Object asset, LDtkLevelFile ldtkFile)
        {
            LDtkIid lDtkIid = ldtkComponentLevel.GetComponent<LDtkIid>();
            _iid = lDtkIid.Iid;

            UpdateInfo(projectJSON, ldtkComponentLevel, location, asset, ldtkFile);
        }

        #endregion

        #region Gathering info

        public void UpdateInfo(LdtkJson projectJSON, LDtkComponentLevel ldtkComponentLevel, IResourceLocation location = null, Object asset = null, LDtkLevelFile ldtkFile = null)
        {
            LDtkFields fields = ldtkComponentLevel.GetComponent<LDtkFields>();

            string displayName = fields.GetString("displayName");
            if (!string.IsNullOrEmpty(displayName))
                name = displayName;

            string area = fields.GetValueAsString("ldtkVaniaArea");
            if (!string.IsNullOrEmpty(area))
                _area = area;

            if (location != null)
            {
                _assetPath = location.InternalId;
                _addressableKey = location.PrimaryKey;
            }

            if (asset != null)
            {
                _asset = asset;
            }

            if (ldtkFile != null)
            {
                _levelFile = ldtkFile;
            }

            // Find the world name of this level
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