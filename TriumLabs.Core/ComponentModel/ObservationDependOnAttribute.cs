using System;

namespace TriumLabs.Core.ComponentModel
{
    /// <summary>
    /// Property marked with <see cref="ObservationDependOnAttribute"/> attribute raises PropertyChanged event, 
    /// if specified dependent property also raised PropertyChanged event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ObservationDependOnAttribute : Attribute
    {
        /// <summary>
        /// The name of the other property the marked property is dependent on.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservationDependOnAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the other property the marked property is dependent on.</param>
        public ObservationDependOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
