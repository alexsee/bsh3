// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Models;

public class BrowserFavoriteItem
{
    public string Name
    {
        get; set;
    } = string.Empty;

    public string Path
    {
        get; set;
    } = string.Empty;

    public bool IsUserFavorite
    {
        get; set;
    }
}
