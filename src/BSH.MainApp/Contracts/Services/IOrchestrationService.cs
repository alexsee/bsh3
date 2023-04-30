// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace BSH.MainApp.Contracts.Services;

public interface IOrchestrationService
{
    Task InitializeAsync();
    Task StartAsync(bool turnOn = false);
    Task StopAsync(bool turnOff = false);
}