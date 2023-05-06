// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
