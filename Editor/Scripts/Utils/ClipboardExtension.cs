using LDtkVania;
using UnityEngine;

namespace LDtkVaniaEditor
{
    public static class ClipboardExtension
    {
        /// <summary>
        /// Puts the string into the Clipboard.
        /// </summary>
        public static void CopyToClipboard(this string str)
        {
            GUIUtility.systemCopyBuffer = str;
            LDtkVania.Logger.Message($"Copied: {str} to clipboard.");
        }
    }
}