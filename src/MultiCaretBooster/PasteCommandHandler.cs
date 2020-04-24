using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Windows.Forms;

namespace MultiCaretBooster
{
    internal class PasteCommandHandler : IOleCommandTarget
    {
        private readonly Guid _guid = VSConstants.GUID_VSStandardCommandSet97; // The VSConstants.VSStd97CmdID enumeration
        private readonly uint _commandId = (uint)VSConstants.VSStd97CmdID.Paste; // The paste command in the above enumeration

        private readonly ITextView _textView;
        private readonly IOleCommandTarget _nextCommandTarget;

        public PasteCommandHandler(IVsTextView adapter, ITextView textView)
        {
            _textView = textView;
            adapter.AddCommandFilter(this, out _nextCommandTarget);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == _guid && nCmdID == _commandId && Clipboard.ContainsText())
            {
                IMultiSelectionBroker selection = _textView.GetMultiSelectionBroker();
                if (selection.HasMultipleSelections)
                {
                    string clipText = Clipboard.GetText(TextDataFormat.Text);

                    if (!string.IsNullOrEmpty(clipText))
                    {
                        int clipTextLinesCount = 0;
                        for (int i = 0; i < clipText.Length; i++)
                        {
                            if (clipText[i] == '\n')
                                clipTextLinesCount++;
                        }
                        if (clipText[clipText.Length - 1] != '\n')
                            clipTextLinesCount++;

                        if (clipTextLinesCount == selection.AllSelections.Count)
                        {
                            using (ITextEdit edit = _textView.TextBuffer.CreateEdit())
                            {
                                int currentClipTextPosition = 0;

                                for (int i = 0; i < selection.AllSelections.Count; i++)
                                {
                                    int clipNextTextPosition = clipText.IndexOf('\n', currentClipTextPosition);
                                    if (clipNextTextPosition < 0)
                                        clipNextTextPosition = clipText.Length;

                                    int clipLength = clipNextTextPosition - currentClipTextPosition - 1;
                                    if (clipLength > 0 && clipText[clipNextTextPosition - 2] == '\r')
                                        clipLength--;

                                    string newText = clipText.Substring(currentClipTextPosition, clipLength);

                                    selection.AllSelections[i].ReplaceWith(edit, newText);

                                    currentClipTextPosition = clipNextTextPosition + 1;
                                }
                                edit.Apply();
                            }

                            return VSConstants.S_OK;
                        }
                    }
                }
            }

            return _nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        //this worked, but given behavior is sometimes unwanted
        //if (clipTextLinesCount % selection.AllSelections.Count == 0)  
        //{
        //    int newLinesPerSelection = clipTextLinesCount / selection.AllSelections.Count;

        //    using (ITextEdit edit = _textView.TextBuffer.CreateEdit())
        //    {
        //        int currentClipTextPosition = 0;

        //        for (int i = 0; i < selection.AllSelections.Count; i++)
        //        {
        //            int clipNextTextPosition = currentClipTextPosition;
        //            for (int newLines = 0; newLines < newLinesPerSelection; clipNextTextPosition++)
        //            {
        //                if (clipNextTextPosition == clipText.Length || clipText[clipNextTextPosition] == '\n')
        //                    newLines++;
        //            }
        //            int clipLength = clipNextTextPosition - currentClipTextPosition - 1;
        //            if (clipText[clipNextTextPosition - 2] == '\r')
        //                clipLength--;
        //            string newText = clipText.Substring(currentClipTextPosition, clipLength);

        //            selection.AllSelections[i].ReplaceWith(edit, newText);

        //            currentClipTextPosition = clipNextTextPosition;
        //        }
        //        edit.Apply();
        //    }

        //    return VSConstants.S_OK;
        //}

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == _guid)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    if (prgCmds[i].cmdID == _commandId)
                    {
                        prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_ENABLED | OLECMDF.OLECMDF_SUPPORTED);
                        return VSConstants.S_OK;
                    }
                }
            }

            return _nextCommandTarget.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}