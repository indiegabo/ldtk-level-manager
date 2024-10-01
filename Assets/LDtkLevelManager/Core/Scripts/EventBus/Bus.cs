using System.Collections.Generic;

namespace LDtkLevelManager.EventBus
{
    /// <summary>
    /// A bus that can be used to dispatch events of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of event that can be dispatched on this bus.</typeparam>
    public static class Bus<T> where T : ILDtkLevelManagerEvent
    {
        /// <summary>
        /// A set of all the bindings registered on this bus.
        /// </summary>
        private static readonly HashSet<IEventBinding<T>> _bindings = new();

        /// <summary>
        /// Registers a binding on this bus.
        /// </summary>
        /// <param name="binding">The binding to register.</param>
        public static void Register(IEventBinding<T> binding) => _bindings.Add(binding);

        /// <summary>
        /// Deregisters a binding from this bus.
        /// </summary>
        /// <param name="binding">The binding to deregister.</param>
        public static void Deregister(IEventBinding<T> binding) => _bindings.Remove(binding);

        /// <summary>
        /// Raises an event on this bus.
        /// </summary>
        /// <param name="ev">The event to raise.</param>
        public static void Raise(T ev)
        {
            foreach (var binding in _bindings)
            {
                binding.OnEvent(ev);
                binding.OnEventNoArgs();
            }
        }

        /// <summary>
        /// Clears all the bindings from this bus.
        /// </summary>
        /// <remarks>
        /// This method is used internally by the <see cref="EventBusUtil"/> class.
        /// </remarks>
        internal static void Clear()
        {
            _bindings.Clear();
        }
    }
}
