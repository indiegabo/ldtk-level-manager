using Cysharp.Threading.Tasks;

namespace LDtkLevelManager
{
    public class UnrelatedLevelLoader : LevelLoader
    {
        public override UniTask LoadLevel(string iid)
        {
            throw new System.NotImplementedException();
        }

        public override UniTask LoadLevel(LevelInfo level)
        {
            throw new System.NotImplementedException();
        }
    }
}