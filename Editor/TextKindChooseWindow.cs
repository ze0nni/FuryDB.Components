using System;
using System.Collections.Generic;

namespace FDB.Components.Editor
{
    public class TextKindChooseWindow : BaseChoseWindow
    {
        public TextKindChooseWindow(
            string value,
            IEnumerable<string> kinds, 
            float width,
            Action<string> callback) 
            : base(value, kinds, width, callback)
        {
        }
    }
}