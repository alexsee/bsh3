// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Windows.Forms;

namespace Brightbits.BSH.Main;

public interface IMainTabs
{
    void OpenTab();
    void CloseTab();

    string Title
    {
        get;
    }
    UserControl UserControlInstance
    {
        get;
    }
    frmMain Super
    {
        set;
    }
}