// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System;

namespace Brightbits.BSH.Engine.Exceptions;

public class DeviceContainsWrongStateException : Exception
{
    public DeviceContainsWrongStateException() : base() { }

    public override string Message => "Das Sicherungsmedium steht nicht zur Verfügung, da es einen anderen Sicherungsstand enthält als dieser Computer. Um die Sicherung nicht zu beschädigen wird BSH auf dieses Sicherungsmedium nicht schreiben.\r\n\r\n";
}
