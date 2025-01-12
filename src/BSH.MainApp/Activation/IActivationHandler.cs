// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Activation;

public interface IActivationHandler
{
    bool CanHandle(object? args);

    Task HandleAsync(object? args);
}
