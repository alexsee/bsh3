// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Specialized;

namespace BSH.MainApp.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
}
