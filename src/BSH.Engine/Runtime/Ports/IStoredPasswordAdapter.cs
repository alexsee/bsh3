// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

namespace Brightbits.BSH.Engine.Runtime.Ports;

/// <summary>
/// Provides access to durable password storage for a specific product.
/// </summary>
public interface IStoredPasswordAdapter
{
    /// <summary>
    /// Gets a stored password if one is available.
    /// </summary>
    string GetPassword();

    /// <summary>
    /// Persists the provided password for later use.
    /// </summary>
    void StorePassword(string password);
}
