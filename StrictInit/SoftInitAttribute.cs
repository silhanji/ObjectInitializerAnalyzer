using System;

namespace StrictInit
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class SoftInitAttribute : Attribute
    {
        
    }
}