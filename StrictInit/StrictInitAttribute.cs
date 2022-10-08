using System;

namespace StrictInit
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property, AllowMultiple = false)]
    public class StrictInitAttribute : Attribute
    {
    }
}