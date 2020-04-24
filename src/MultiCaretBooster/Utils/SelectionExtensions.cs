using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCaretBooster
{
    internal static class SelectionExtensions
    {
        public static void ReplaceWith(this Selection selection, ITextEdit edit, string newText)
        {
            int selectionLength = selection.End.Position.Position - selection.Start.Position.Position;
            if (selectionLength > 0)
                edit.Replace(selection.Start.Position.Position, selectionLength, newText);
            else
                edit.Insert(selection.InsertionPoint.Position.Position, newText);
        }
    }
}
