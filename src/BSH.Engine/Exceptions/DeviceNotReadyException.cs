// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Exceptions;

public class DeviceNotReadyException : Exception
{
    public DeviceNotReadyException() : base() { }

    public override string Message => "Das Sicherungsmedium steht aktuell nicht zur Verfügung.";
}
