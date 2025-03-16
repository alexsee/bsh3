// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace MRG.Controls.UI
{
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.All)]
    public class LoadingCircleToolStripMenuItem : ToolStripControlHost
    {
        /// <summary>
        /// Gets the loading circle control.
        /// </summary>
        /// <value>The loading circle control.</value>
        [RefreshProperties(RefreshProperties.All),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LoadingCircle LoadingCircleControl
        {
            get
            {
                return Control as LoadingCircle;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadingCircleToolStripMenuItem"/> class.
        /// </summary>
        public LoadingCircleToolStripMenuItem()
            : base(new LoadingCircle())
        {
        }

        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <param name="constrainingSize">The custom-sized area for a control.</param>
        /// <returns>
        /// An ordered pair of type <see cref="T:System.Drawing.Size"></see> representing the width and height of a rectangle.
        /// </returns>
        /// <PermissionSet><IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/><IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/><IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/></PermissionSet>
        public override Size GetPreferredSize(Size constrainingSize)
        {
            return this.LoadingCircleControl.GetPreferredSize(constrainingSize);
        }
    }
}