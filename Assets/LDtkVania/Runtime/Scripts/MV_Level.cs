using System.Collections.Generic;
using LDtkUnity;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace LDtkVania
{
    [System.Serializable]
    public class MV_Level : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        private string _name;

        [SerializeField]
        private string _iid;

        [SerializeField]
        private Object _asset;

        [SerializeField]
        private LDtkLevelFile _levelFile;

        [SerializeField]
        private SceneField _scene;

        [SerializeField]
        private string _sceneAssetGuid;

        [SerializeField]
        private string _sceneAddressableKey;

        [SerializeField]
        private string _assetPath;

        [SerializeField]
        private string _assetKey;

        #endregion

        #region Field

        private Level _ldtkLevel;

        #endregion

        #region Getters

        public string Iid => _iid;
        public Object Asset => _asset;

        public SceneField Scene { get => _scene; set => _scene = value; }
        public string SceneAssetGuid { get => _sceneAssetGuid; set => _sceneAssetGuid = value; }

        public string AssetPath => _assetPath;
        public string AddressableKey => _assetKey;
        public string SceneAddressableKey => _sceneAddressableKey;

        public string Name => _name;

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

        public void Initialize(LDtkComponentLevel ldtkComponentLevel, IResourceLocation location, Object asset, LDtkLevelFile ldtkFile)
        {
            LDtkIid lDtkIid = ldtkComponentLevel.GetComponent<LDtkIid>();
            _iid = lDtkIid.Iid;

            _assetPath = location.InternalId;
            _assetKey = location.PrimaryKey;

            _asset = asset;
            _levelFile = ldtkFile;

            _name = asset.name;
        }

        #endregion

        #region Gathering info

        public void UpdateInfo(LDtkComponentLevel ldtkComponentLevel, IResourceLocation location = null, Object asset = null, LDtkLevelFile ldtkFile = null)
        {
            LDtkIid lDtkIid = ldtkComponentLevel.GetComponent<LDtkIid>();
            _iid = lDtkIid.Iid;

            if (location != null)
            {
                _assetPath = location.InternalId;
                _assetKey = location.PrimaryKey;
            }

            if (asset != null)
            {
                _asset = asset;
                _name = asset.name;
            }

            if (ldtkFile != null)
            {
                _levelFile = ldtkFile;
            }
        }

        #endregion
    }
}