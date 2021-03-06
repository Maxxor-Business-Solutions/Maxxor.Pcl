﻿namespace Maxxor.PCL.ValueObject.Base
{
    public abstract class MxEnum : MxValueObject<MxEnum>
    {
        protected readonly string Name;

        protected MxEnum(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}