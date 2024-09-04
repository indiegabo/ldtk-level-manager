using Cysharp.Threading.Tasks;

namespace LDtkLevelManager.PlayerNavigation
{
    public interface ITransition
    {
        UniTask TransitionInto();
        void SnapInto();
    }
}