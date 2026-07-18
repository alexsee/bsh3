// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.MainApp.Models;

namespace BSH.MainApp.Contracts.Services;

public interface ICompletionActionService
{
    Task ExecuteAsync(TaskCompleteAction action);
}
