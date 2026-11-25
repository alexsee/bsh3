using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using C4F.DevKit.PreviewHandler.PreviewHandlerFramework;

namespace C4F.DevKit.PreviewHandler.PreviewHandlerHost
{
    [ToolboxItem(true), ToolboxBitmap(typeof(PreviewHandlerHostControl))]
    public partial class PreviewHandlerHostControl : UserControl
    {
        private string _filePath;
        private object _comInstance = null;
        private Stream _previewStream;
        private Boolean _hasHandler = true;

        public PreviewHandlerHostControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Full path to file to be previewed
        /// 
        /// Whenever a new path is set, the preview is generated
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
                if (!IsDesignTime())
                {
                    if (value == null)
                    {
                        Unload();
                    }
                    else
                    {
                        GeneratePreview();
                    }
                }
            }
        }

        public Boolean HasHandler
        {
            get
            {
                return _hasHandler;
            }
        }

        private bool IsDesignTime()
        {
            return (this.Site != null && this.Site.DesignMode);
        }

        public void Unload()
        {
            var comInstance = _comInstance;
            var stream = _previewStream;
            _comInstance = null;
            _previewStream = null;
            _hasHandler = false;

            try
            {
                if (comInstance != null)
                {
                    ((IPreviewHandler)comInstance).Unload();
                }
            }
            catch
            {
                // Unload can fail if the COM preview handler is already torn down.
            }
            finally
            {
                try
                {
                    if (comInstance != null)
                    {
                        Marshal.ReleaseComObject(comInstance);
                    }
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unload();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void GeneratePreview()
        {
            lblMessage.Visible = false;
            Unload();

            RECT r;
            r.top = 0;
            r.bottom = this.Height;
            r.left = 0;
            r.right = this.Width;

            string handlerId = PreviewHandlerRegistryAccessor.FindHandlerId(_filePath);
            if (handlerId == null)
            {
                lblMessage.Visible = true;
                lblMessage.Text = "Keine Vorschau verfügbar";
                _hasHandler = false;
                return;
            }

            Type comType = Type.GetTypeFromCLSID(new Guid(handlerId));
            try
            {
                _comInstance = Activator.CreateInstance(comType);

                if (_comInstance is IInitializeWithFile fileInitializer)
                {
                    fileInitializer.Initialize(_filePath, 0);
                }
                else if (_comInstance is IInitializeWithStream streamInitializer)
                {
                    if (File.Exists(_filePath))
                    {
                        _previewStream = File.Open(_filePath, FileMode.Open);
                        streamInitializer.Initialize(new StreamWrapper(_previewStream), 0);
                    }
                    else
                    {
                        throw new FileNotFoundException("File not found", _filePath);
                    }
                }
                ((IPreviewHandler)_comInstance).SetWindow(this.Handle, ref r);
                ((IPreviewHandler)_comInstance).DoPreview();
                _hasHandler = true;
            }
            catch (Exception ex)
            {
                Unload();
                lblMessage.Visible = true;
                lblMessage.Text = "Keine Vorschau verfügbar - " + ex.Message;
                _hasHandler = false;
            }

        }


        private void Control_Resize(object sender, EventArgs e)
        {
            if (_comInstance != null)
            {
                RECT r;
                r.top = 0;
                r.bottom = this.Height;
                r.left = 0;
                r.right = this.Width;
                ((IPreviewHandler)_comInstance).SetRect(ref r);
            }

        }
    }
}