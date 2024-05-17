using System.Collections.Generic;
using LDtkVania.Utils;

namespace LDtkVania
{
    public class MV_PaginatedResponse<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
    }

    public struct MV_PaginationInfo
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    [System.Serializable]
    public class MV_WorldAreas
    {
        public string worldIid;
        public string worldName;
        public List<string> areas;
    }


    [System.Serializable]
    public class MV_LevelsDictionary : SerializedDictionary<string, MV_Level> { }

    [System.Serializable]
    public class MV_WorldAreasDictionary : SerializedDictionary<string, MV_WorldAreas> { }
}