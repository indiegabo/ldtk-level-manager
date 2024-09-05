using UnityEngine;

namespace LDtkLevelManager.Implementations.Basic
{
    public class WaitOnAnimation : CustomYieldInstruction
    {
        private Animator _animator;

        public override bool keepWaiting
        {
            get
            {
                return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
            }
        }

        public WaitOnAnimation(Animator animator)
        {
            _animator = animator;
        }
    }
}