using System;
using UnityEngine;

namespace LDtkLevelManager
{
        public static class Logger
        {
                private static string Prefix => $"<color=#FFFFFF><b>[LDtkLevelManager]</b></color>";

                /// <summary>
                /// Logs a message.
                /// </summary>
                /// <param name="message">The message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Message(string message, UnityEngine.Object sender = null)
                {
                        Debug.Log($"{Prefix} {message}", sender);
                }

                /// <summary>
                /// Logs an warning message.
                /// </summary>
                /// <param name="message">The warning message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Warning(string message, UnityEngine.Object sender = null)
                {
                        Debug.LogWarning($"{Prefix} {message}", sender);
                }

                /// <summary>
                /// Logs an error message.
                /// </summary>
                /// <param name="message">The error message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Error(string message, UnityEngine.Object sender = null)
                {
                        Debug.LogError($"{Prefix} {message}", sender);
                }

                /// <summary>
                /// Logs an error message.
                /// </summary>
                /// <param name="message">The error message.</param>
                /// <param name="sender">The object that triggered the error (optional).</param>
                public static void Exception(System.Exception exception, UnityEngine.Object sender = null)
                {
                        Message($"{Prefix} The following exception was thrown:", sender);
                        Debug.LogException(exception, sender);
                }
        }
}