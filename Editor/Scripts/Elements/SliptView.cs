using UnityEngine;
using UnityEngine.UIElements;

namespace LDtkLevelManagerEditor
{
    public class SliptView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SliptView, TwoPaneSplitView.UxmlTraits> { }
    }
}