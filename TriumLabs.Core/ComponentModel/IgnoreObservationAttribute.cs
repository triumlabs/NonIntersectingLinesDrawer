using System;

namespace TriumLabs.Core.ComponentModel
{
    /// <summary>
    /// Auto property marked with <see cref="IgnoreObservationAttribute"/> attribute does not raise PropertyChanged event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class IgnoreObservationAttribute : Attribute { }
}
