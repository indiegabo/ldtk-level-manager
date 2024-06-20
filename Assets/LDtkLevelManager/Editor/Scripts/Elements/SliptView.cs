using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkVaniaEditor
{
    public class SliptView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SliptView, TwoPaneSplitView.UxmlTraits> { }
    }
}