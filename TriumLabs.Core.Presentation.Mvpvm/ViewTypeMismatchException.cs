using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace TriumLabs.Core.Presentation.Mvpvm
{
    /// <summary>
    /// Represents an error that occurs when a view's type is not what expected.
    /// </summary>
    [Serializable]
    public class ViewTypeMismatchException : Exception
    {

        /// <summary>
        /// Gets or sets the expected type of the view.
        /// </summary>
        /// <value>The expected type of the view.</value>
        public Type ExpectedViewType { get; private set; }

        /// <summary>
        /// Gets or sets the actual type of the view.
        /// </summary>
        /// <value>The actual type of the view.</value>
        public Type ActualViewType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTypeMismatchException"/> class.
        /// </summary>
        public ViewTypeMismatchException()
            : base("View type mismatches.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTypeMismatchException"/> class.
        /// </summary>
        /// <param name="expectedViewType">Expected type of the view.</param>
        /// <param name="actualViewType">Actual type of the view.</param>
        public ViewTypeMismatchException(Type expectedViewType, Type actualViewType)
            : base(String.Format(
                       CultureInfo.CurrentCulture,
                       "View type mismatches. Expected type {0}, actual type {1}.",
                       expectedViewType, actualViewType))
        {
            ExpectedViewType = expectedViewType;
            ActualViewType = actualViewType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTypeMismatchException"/> class
        /// with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ViewTypeMismatchException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTypeMismatchException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ViewTypeMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTypeMismatchException"/> class 
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ViewTypeMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedViewType = info.GetValue("ExpectedViewType", typeof(Type)) as Type;
            ActualViewType = info.GetValue("ActualViewType", typeof(Type)) as Type;
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic). </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ExpectedViewType", ExpectedViewType);
            info.AddValue("ActualViewType", ActualViewType);
        }
    }
}
