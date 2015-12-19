using System;

namespace TriumLabs.Core.ComponentModel
{
    /// <summary>
    /// Auto property marked with <see cref="ObservableAttribute"/> attribute raises PropertyChanged event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ObservableAttribute : Attribute { }
}
