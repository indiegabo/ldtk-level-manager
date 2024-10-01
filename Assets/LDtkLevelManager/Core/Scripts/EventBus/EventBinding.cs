using System;

namespace LDtkLevelManager.EventBus
{
    /// <summary>
    /// Represents an event binding to a specified event.
    /// </summary>
    public interface IEventBinding<T>
    {
        /// <summary>
        /// Gets or sets the event handler for the event.
        /// </summary>
        Action<T> OnEvent { get; set; }

        /// <summary>
        /// Gets or sets the event handler for the event without arguments.
        /// </summary>
        Action OnEventNoArgs { get; set; }
    }

    /// <summary>
    /// Represents an event binding to a specified event. After creating a new <see cref="EventBinding{T}"/>,
    /// use <see cref="Bus.Register(IEventBinding{T})"/> to register and <see cref="Bus.Deregister(IEventBinding{T})"/> to deregister event bindings.
    /// </summary>
    /// <typeparam name="T">The type of the event.</typeparam>
    public class EventBinding<T> : IEventBinding<T> where T : ILDtkLevelManagerEvent
    {
        private Action<T> _onEvent = _ => { };
        private Action _onEventNoArgs = () => { };

        /// <summary>
        /// Gets or sets the event handler for the event.
        /// </summary>
        Action<T> IEventBinding<T>.OnEvent { get => _onEvent; set => _onEvent = value; }

        /// <summary>
        /// Gets or sets the event handler for the event without arguments.
        /// </summary>
        Action IEventBinding<T>.OnEventNoArgs { get => _onEventNoArgs; set => _onEventNoArgs = value; }

        /// <summary>
        /// Creates a new <see cref="EventBinding{T}"/> with the specified event handler.
        /// </summary>
        /// <param name="onEvent">The event handler for the event.</param>
        public EventBinding(Action<T> onEvent) => _onEvent = onEvent;

        /// <summary>
        /// Creates a new <see cref="EventBinding{T}"/> with the specified event handler without arguments.
        /// </summary>
        /// <param name="onEventNoArgs">The event handler for the event without arguments.</param>
        public EventBinding(Action onEventNoArgs) => _onEventNoArgs = onEventNoArgs;

        /// <summary>
        /// Adds an event handler for the event.
        /// </summary>
        /// <param name="onEvent">The event handler for the event.</param>
        public void Add(Action<T> onEvent) => _onEvent += onEvent;

        /// <summary>
        /// Removes an event handler for the event.
        /// </summary>
        /// <param name="onEvent">The event handler for the event.</param>
        public void Remove(Action<T> onEvent) => _onEvent -= onEvent;

        /// <summary>
        /// Adds an event handler for the event without arguments.
        /// </summary>
        /// <param name="onEventNoArgs">The event handler for the event without arguments.</param>
        public void Add(Action onEventNoArgs) => _onEventNoArgs += onEventNoArgs;

        /// <summary>
        /// Removes an event handler for the event without arguments.
        /// </summary>
        /// <param name="onEventNoArgs">The event handler for the event without arguments.</param>
        public void Remove(Action onEventNoArgs) => _onEventNoArgs -= onEventNoArgs;
    }
}