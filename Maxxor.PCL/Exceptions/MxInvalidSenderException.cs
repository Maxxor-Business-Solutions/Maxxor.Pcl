﻿using System;

namespace Maxxor.PCL.Exceptions
{
    public class MxInvalidSenderException : Exception
    {
        public override string Message => "An instance of MxResult cannot be used as the sender";
    }
}