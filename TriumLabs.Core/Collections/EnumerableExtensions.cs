using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TriumLabs.Core.Collections
{
	/// <summary>
	/// 
	/// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> enumerable, params TSource[] items)
        {
            if (enumerable == null) return items;
            if (items == null) return enumerable;
            return System.Linq.Enumerable.Concat(enumerable, items);
        }

        /// <summary>
        /// Returns distinct elements from an enumerable by using the specified comparer delegate to compare values.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to remove duplicate elements from.</param>
        /// <param name="comparer">The comparer delegate to compare values.</param>
        /// <returns>An enumerable that contains distinct elements from the source enumerable.</returns>
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> enumerable, Func<T, T, bool> comparer)
        {
            return enumerable.Distinct(new EqualityComparer<T>(comparer));
        }
        
        /// <summary>
        /// Iterates through an enumerable and executes the action for each item.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to iterate.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable != null && action != null)
            {
                foreach (var item in enumerable)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Iterates through an enumerable and executes the action for each item.
        /// The index of the current item is also supplied.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to iterate.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            if (enumerable != null && action != null)
            {
                var idx = 0;
                foreach (var item in enumerable)
                {
                    action(item, idx++);
                }
            }
        }

        /// <summary>
        /// Iterates through an enumerable and executes the action for each item.
        /// </summary>
        /// <param name="enumerable">The enumerable to iterate.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEach(this IEnumerable enumerable, Action<object> action)
        {
            if (enumerable != null && action != null)
            {
                foreach (var item in enumerable)
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Iterates backward through an enumerable and executes the action for each item.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to iterate.</param>
        /// <param name="action">The action to execute.</param>
        public static void ForEachBackward<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable != null && action != null)
            {
                foreach (var item in enumerable.Reverse())
                {
                    action(item);
                }
            }
        }

        /// <summary>
        /// Iterates through an enumerable and executed the action for each item paralelly.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to iterate.</param>
        /// <param name="action">The action to execute.</param>
        /// <param name="threadCount">The number of <see cref="T:Thread"/> to use.</param>
        public static void ForEachMultiThreaded<T>(this IEnumerable<T> enumerable, Action<T> action, int threadCount)
        {
            var qItem = Queue.Synchronized(new Queue());
            var qError = Queue.Synchronized(new Queue());
            //var dtStart = DateTime.Now;

            enumerable.ForEach(item => qItem.Enqueue(item));

            var threads = new Thread[threadCount];
            for (var idx = 0; idx < threads.Length; idx++)
            {
                threads[idx] = new Thread(() =>
                {
                    //Console.WriteLine("Thread started: {0}", Thread.CurrentThread.ManagedThreadId);
                    while (qItem.Count > 0)
                    {
                        try
                        {
                            var item = (T)qItem.Dequeue();
                            //Console.WriteLine("Thread picked a task: {0}", Thread.CurrentThread.ManagedThreadId);
                            action(item);
                        }
                        catch (InvalidOperationException)
                        {
                            return;
                        }
                        catch (Exception ex)
                        {
                            qItem.Clear();
                            qError.Enqueue(ex);
                        }
                    }
                    //Console.WriteLine("Thread ended: {0}", Thread.CurrentThread.ManagedThreadId);
                });
                threads[idx].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            //Console.WriteLine("ForEachParallel duration: {0}", DateTime.Now - dtStart);

            if (qError.Count > 0)
                throw (Exception)qError.Dequeue();
        }

        /// <summary>
        /// Indicates whether the specified enumerable is neither null nor empty.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to test.</param>
        /// <returns><c>true</c> if enumerable is neither null nor empty; otherwise <c>false</c>.</returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> enumerable)
        {
            return !IsNullOrEmpty(enumerable);
        }

        /// <summary>
        /// Indicates whether the specified enumerable is neither null nor empty.
        /// </summary>
        /// <param name="enumerable">The enumerable to test.</param>
        /// <returns><c>true</c> if enumerable is neither null nor empty; otherwise <c>false</c>.</returns>
        public static bool IsNotEmpty(this IEnumerable enumerable)
        {
            return !IsNullOrEmpty(enumerable);
        }

        /// <summary>
        /// Indicates whether the specified enumerable is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of items in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable to test.</param>
        /// <returns><c>true</c> if enumerable is null or empty; otherwise <c>false</c>.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return true;

            var collectionT = enumerable as ICollection<T>;
            if (collectionT != null) return collectionT.Count == 0;

            var collection = enumerable as ICollection;
            if (collection != null) return collection.Count == 0;

            using (var enumerator = enumerable.GetEnumerator())
            {
                return !enumerator.MoveNext();
            }
        }

        /// <summary>
        /// Indicates whether the specified enumerable is null or empty.
        /// </summary>
        /// <param name="enumerable">The enumerable to test.</param>
        /// <returns><c>true</c> if enumerable is null or empty; otherwise <c>false</c>.</returns>
        public static bool IsNullOrEmpty(this IEnumerable enumerable)
        {
            if (enumerable == null) return true;

            var collection = enumerable as ICollection;
            if (collection != null) return collection.Count == 0;

            var enumerator = enumerable.GetEnumerator();
            return !enumerator.MoveNext();
        }

        /// <summary>
        /// Selects an item from the enumerable randomly.
        /// </summary>
        /// <param name="enumerable">The enumerable to select from.</param>
        /// <returns>The random item.</returns>
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            return Random(enumerable, new Random());
        }

        /// <summary>
        /// Selects an item from the enumerable randomly using the given random number generator.
        /// </summary>
        /// <param name="enumerable">The enumerable to select from.</param>
        /// <param name="numberGenerator">The random number generator to use.</param>
        /// <returns>The random item.</returns>
        public static T Random<T>(this IEnumerable<T> enumerable, Random numberGenerator)
        {
            var max = enumerable.Count();
            var itemIndex = numberGenerator.Next(0, max - 1);
            return enumerable.ElementAt(itemIndex);
        }

    }
}