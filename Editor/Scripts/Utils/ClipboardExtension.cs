using LDtkLevelManager;
using UnityEngine;

namespace LDtkLevelManagerEditor
{
    public static class ClipboardExtension
    {
        /// <summary>
        /// Puts the string into the Clipboard.
        /// </summary>
        public static void CopyToClipboard(this string str)
        {
            GUIUtility.systemCopyBuffer = str;
            LDtkLevelManager.Logger.Message($"Copied: {str} to clipboard.");
        }
    }
}