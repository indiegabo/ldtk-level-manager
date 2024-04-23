using System.Threading.Tasks;

namespace LDtkVania
{
    public interface ITransition
    {
        Task TransitionInto();
        void SnapInto();
    }
}