using System.Threading.Tasks;

namespace LDtkVania.Transitioning
{
    public interface ITransition
    {
        Task TransitionInto();
        void SnapInto();
    }
}