using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCaretBooster
{
    internal static class ServiceProviderExtensions
    {
        public static async Task<IWpfTextView> GetWpfTextViewAsync(this Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider)
        {
            IVsTextManager2 textManager = (IVsTextManager2)await serviceProvider.GetServiceAsync(typeof(SVsTextManager));

            if (textManager == null)
                return null;

            int result = textManager.GetActiveView2(1, null, (uint)_VIEWFRAMETYPE.vftCodeWindow, out IVsTextView view);
            if (result != VSConstants.S_OK)
                return null;

            IComponentModel componentModel = (IComponentModel)await serviceProvider.GetServiceAsync(typeof(SComponentModel));

            IVsEditorAdaptersFactoryService adapterService = componentModel?.GetService<IVsEditorAdaptersFactoryService>();

            if (adapterService == null)
                return null;

            return adapterService.GetWpfTextView(view);
        }
    }
}
