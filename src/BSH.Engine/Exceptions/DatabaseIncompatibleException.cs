// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;
using Brightbits.BSH.Engine.Properties;

namespace Brightbits.BSH.Engine.Exceptions;

public class DatabaseIncompatibleException : Exception
{
    public DatabaseIncompatibleException() : base(Resources.EXCEPTION_NEWER_DATABASE) { }
}
