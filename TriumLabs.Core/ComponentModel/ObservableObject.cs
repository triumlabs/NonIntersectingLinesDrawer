using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TriumLabs.Core.Reflection;
using TriumLabs.Core.Threading;

namespace TriumLabs.Core.ComponentModel
{
    /// <summary>
    /// Represents an observable object.
    /// </summary>
    [Observable]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region Inner Types

        /// <summary>
        /// Represents the dependency information class.
        /// </summary>
        private class DependencyInfo
        {
            /// <summary>
            /// Gets or sets the property information of the owner, where the ObservationDependOnAttribute is declared.
            /// </summary>
            public PropertyInfo OwnerProperty { get; set; }

            /// <summary>
            /// Gets or sets the list of ObservationDependOnAttributes.
            /// </summary>
            public ObservationDependOnAttribute[] DependOnAttributes { get; set; }
        }

        #endregion

        #region Fields

        private static readonly IDictionary<Type, DependencyInfo[]> dicDependency = new ConcurrentDictionary<Type, DependencyInfo[]>();
        private readonly Dispatcher dispatcher;

        #endregion
    
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableObject"/> class.
        /// </summary>
        protected ObservableObject()
        {
            var type = GetType();

            if (!dicDependency.ContainsKey(type))
            {
                lock (dicDependency)
                {
                    if (!dicDependency.ContainsKey(type))
                    {
                        var dependencies = type.GetProperties()
                            .Select(pi => new DependencyInfo 
                                { 
                                    OwnerProperty = pi, 
                                    DependOnAttributes = pi.GetCustomAttributes<ObservationDependOnAttribute>() 
                                })
                            .Where(item => item.DependOnAttributes.Length > 0)
                            .ToArray();
                        dicDependency[type] = dependencies;
                    }
                }
            }

            if (dicDependency[type].Length > 0)
            {
                PropertyChanged += (sender, e) =>
                {
                    var dependency = dicDependency[type]
                        .FirstOrDefault(dep => dep.DependOnAttributes.Count(attr => attr.PropertyName == e.PropertyName) > 0);
                    if (dependency == null) return;

                    RaiseEventPropertyChanged(dependency.OwnerProperty.Name);
                };
            }

            dispatcher = new Dispatcher();
        }

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        public virtual void RaiseEventPropertyChanged(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            RaiseEventPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected virtual void RaiseEventPropertyChanged(PropertyChangedEventArgs args)
        {
            dispatcher.Invoke(() => PropertyChanged(this, args));
        }

        /// <summary>
        /// Resets the property changed event.
        /// </summary>
        protected virtual void ResetEventPropertyChanged()
        {
            PropertyChanged = delegate { };
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion
    }
}
