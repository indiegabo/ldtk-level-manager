using UnityEngine;

namespace LDtkLevelManager
{
    [System.Serializable]
    public class LanesSettings
    {
        [SerializeField] private float _cameraNegativeOffset;

        [SerializeField] private LaneInfo _universeLane;
        [SerializeField] private LaneInfo _mapRenderingLane;

        /// <summary>
        /// The negative offset to apply to the camera.
        /// </summary>
        public float CameraNegativeOffset
        {
            get { return _cameraNegativeOffset; }
            set { _cameraNegativeOffset = value; }
        }

        /// <summary>
        /// The lane info for the universe.
        /// </summary>
        public LaneInfo UniverseLane => _universeLane;

        /// <summary>
        /// The lane info for rendering the map.
        /// </summary>
        public LaneInfo MapRenderingLane => _mapRenderingLane;

    }

    [System.Serializable]
    public class LaneInfo
    {
        [SerializeField] private float _startingZ;
        [SerializeField] private float _depth;

        /// <summary>
        /// The starting Z position of the lane.
        /// </summary>
        public float StartingZ
        {
            get { return _startingZ; }
            set { _startingZ = value; }
        }

        /// <summary>
        /// The depth of the lane.
        /// </summary>
        public float Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }
    }
}