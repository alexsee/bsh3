// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Models;

public sealed record SwitchStorageFtpTarget(
    string Host,
    string Port,
    string User,
    string Password,
    string Folder,
    string Encoding,
    bool EnforceUnencrypted);
