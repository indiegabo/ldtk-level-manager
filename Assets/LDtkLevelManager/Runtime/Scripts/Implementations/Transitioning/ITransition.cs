using System.Threading.Tasks;

namespace LDtkLevelManager.Transitioning
{
    public interface ITransition
    {
        Task TransitionInto();
        void SnapInto();
    }
}