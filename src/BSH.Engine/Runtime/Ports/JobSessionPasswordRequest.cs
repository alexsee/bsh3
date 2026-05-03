// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Runtime.Ports;

/// <summary>
/// Represents a password entered during a live job-session prompt.
/// </summary>
public readonly record struct JobSessionPasswordRequest(string Password, bool Persist);
