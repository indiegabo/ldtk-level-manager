using System;
using UnityEngine;

namespace LDtkVania
{
        public static class MV_Logger
        {
                private static string Prefix => $"<color=#FFFFFF>[LDtkVania]</color>";

                /// <summary>
                /// Logs a message.
                /// </summary>
                /// <param name="message">The message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Message(string message, UnityEngine.Object sender = null)
                {
#if UNITY_EDITOR
                        Debug.Log($"{Prefix} {message}", sender);
#endif
                }

                /// <summary>
                /// Logs an warning message.
                /// </summary>
                /// <param name="message">The warning message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Warning(string message, UnityEngine.Object sender = null)
                {
#if UNITY_EDITOR
                        Debug.LogWarning($"{Prefix} {message}", sender);
#endif
                }

                /// <summary>
                /// Logs an error message.
                /// </summary>
                /// <param name="message">The error message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Error(string message, UnityEngine.Object sender = null)
                {
#if UNITY_EDITOR
                        Debug.LogError($"{Prefix} {message}", sender);
#endif
                }

                /// <summary>
                /// Logs an error message.
                /// </summary>
                /// <param name="message">The error message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Exception(System.Exception exception, UnityEngine.Object sender = null)
                {
#if UNITY_EDITOR
                        Message($"The following exception was thrown:", sender);
                        Debug.LogException(exception, sender);
#endif
                }
        }
}