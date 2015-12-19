using System;
using System.Threading;
using System.Threading.Tasks;

namespace TriumLabs.Core.Threading
{
    /// <summary>
    /// Represents a dispatcher to invoke actions in a synchronization context.
    /// </summary>
    public class Dispatcher
    {
        private readonly SynchronizationContext ctxSynchronization = null;

        /// <summary>
        /// Gets a value indicating whether call to invoke method is required.
        /// </summary>
        /// <value><c>true</c> if call to invoke method is required; otherwise <c>false</c>.</value>
        public bool InvokeRequired
        {
            get { return ctxSynchronization != null && ctxSynchronization != SynchronizationContext.Current; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Dispatcher"/> class.
        /// </summary>
        public Dispatcher()
        {
            ctxSynchronization = SynchronizationContext.Current;
        }

        /// <summary>
        /// Invokes the given action synchronous in a synchronization context if required.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public void Invoke(Action action)
        {
            if (action == null) return;

            if (InvokeRequired) ctxSynchronization.Send(delegate { action(); }, null);
            else action();
        }

        /// <summary>
        /// Invokes the given action asynchronous in a synchronization context if required.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource<object>();

            if (action != null)
            {
                if (InvokeRequired)
                {
                    ctxSynchronization.Post(
                        delegate
                        {
                            try
                            {
                                action();
                                tcs.SetResult(null);
                            }
                            catch (Exception ex)
                            {
                                tcs.SetException(ex);
                            }
                        },
                        null);
                }
                else
                {
                    action.BeginInvoke(
                        delegate(IAsyncResult ar)
                        {
                            try
                            {
                                action.EndInvoke(ar);
                                tcs.SetResult(null);
                            }
                            catch (Exception ex)
                            {
                                tcs.SetException(ex);
                            }
                        },
                        null);
                }
            }
            else
            {
                tcs.SetResult(null);
            }

            return tcs.Task;
        }
    }
}
