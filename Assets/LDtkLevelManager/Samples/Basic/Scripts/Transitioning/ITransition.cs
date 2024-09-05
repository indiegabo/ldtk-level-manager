using Cysharp.Threading.Tasks;

namespace LDtkLevelManager.Implementations.Basic
{
    public interface ITransition
    {
        UniTask TransitionInto();
        void SnapInto();
    }
}