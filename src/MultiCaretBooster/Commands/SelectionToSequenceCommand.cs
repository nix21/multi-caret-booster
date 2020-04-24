﻿using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Task = System.Threading.Tasks.Task;

namespace MultiCaretBooster
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SelectionToSequenceCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("ca6901e6-688f-4a8c-823a-e88c8685b7c0");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionToSequenceCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SelectionToSequenceCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SelectionToSequenceCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SelectionToSequenceCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SelectionToSequenceCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ExecuteAsync();
        }

        private async Task ExecuteAsync()
        {
            IWpfTextView textView = await ServiceProvider.GetWpfTextViewAsync();
            if (textView == null)
                return;

            IMultiSelectionBroker multiSelectioBroker = textView.GetMultiSelectionBroker();

            if (multiSelectioBroker.HasMultipleSelections)
            {
                string firstSelectionValue = multiSelectioBroker.AllSelections[0].Extent.GetText();

                if (int.TryParse(firstSelectionValue, out int currentValue))
                {
                    using (ITextEdit edit = textView.TextBuffer.CreateEdit())
                    {
                        for (int i = 0; i < multiSelectioBroker.AllSelections.Count; i++)
                        {
                            multiSelectioBroker.AllSelections[i].ReplaceWith(edit, currentValue.ToString());
                            currentValue++;
                        }

                        edit.Apply();
                    }
                }
                else if (firstSelectionValue.Length == 1)
                {
                    char currentChar = firstSelectionValue[0];

                    using (ITextEdit edit = textView.TextBuffer.CreateEdit())
                    {
                        for (int i = 0; i < multiSelectioBroker.AllSelections.Count; i++)
                        {
                            multiSelectioBroker.AllSelections[i].ReplaceWith(edit, currentChar.ToString());
                            currentChar++;
                        }

                        edit.Apply();
                    }
                }
            }
        }
    }
}
