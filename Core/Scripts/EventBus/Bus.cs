using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LDtkLevelManager.EventBus
{
    public static class Bus<T> where T : ILDtkLevelManagerEvent
    {
        static readonly HashSet<IEventBinding<T>> _bindings = new();

        public static void Register(IEventBinding<T> binding) => _bindings.Add(binding);
        public static void Deregister(IEventBinding<T> binding) => _bindings.Remove(binding);

        public static void Raise(T @event)
        {
            foreach (var binding in _bindings)
            {
                binding.OnEvent(@event);
                binding.OnEventNoArgs();
            }
        }

        static void Clear()
        {
            _bindings.Clear();
        }
    }
}
