using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerHost
{
    /// <summary>
    /// Looks up the registered preview handler for a file.
    /// The majority of this code and logic comes from the Preview Handler Association Editor that Stephen Toub wrote
    /// and posted about on his blog.  http://blogs.msdn.com/toub/archive/2006/12/14/preview-handler-association-editor.aspx
    /// We made a few minor tweaks for our purposes, but the core of the logic is his.  Thanks to Stephen for sharing this code.
    /// </summary>
    internal static class PreviewHandlerRegistryAccessor
    {
        private const string BaseRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\PreviewHandlers";
        private const string BaseClsIDKey = @"HKEY_CLASSES_ROOT\{0}\shellex\{{8895b1c6-b41f-4c1c-a562-0d564250836f}}";
        private const string BaseClsIdKey2 = @"HKEY_CLASSES_ROOT\SystemFileAssociations\{0}\shellex\{{8895b1c6-b41f-4c1c-a562-0d564250836f}}";

        internal static string FindHandlerId(string filePath)
        {
            HashSet<string> registeredHandlerIds;
            using (RegistryKey handlersKey = Registry.LocalMachine.OpenSubKey(BaseRegistryKey))
            {
                registeredHandlerIds = new HashSet<string>(handlersKey.GetValueNames(), StringComparer.Ordinal);
            }

            foreach (string extension in Registry.ClassesRoot.GetSubKeyNames())
            {
                if (!extension.StartsWith(".") || !filePath.ToUpper().EndsWith(extension.ToUpper()))
                {
                    continue;
                }

                string id = Registry.GetValue(string.Format(BaseClsIDKey, extension), null, null) as string;
                if (id == null)
                {
                    id = Registry.GetValue(string.Format(BaseClsIdKey2, extension), null, null) as string;
                }

                if (id != null && registeredHandlerIds.Contains(id))
                {
                    return id;
                }
            }

            return null;
        }
    }
}
