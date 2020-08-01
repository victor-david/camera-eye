using System;
using System.Collections;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides a generic implementation for a generic comparer
    /// </summary>
    /// <typeparam name="T">The type to compare.</typeparam>
    public class GenericComparer<T> : IComparer
    {
        #region Private
        private readonly Comparison<T> comparer;
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericComparer{T}"/> class.
        /// </summary>
        /// <param name="comparer">The method that the comparison is delegated to.</param>
        public GenericComparer(Comparison<T> comparer)
        {
            this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }
        #endregion

        /************************************************************************/

        #region IComparer
        /// <summary>
        /// Compares two objects and returns a value that indicates whether one is less than,
        /// equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Less than zero if x is less than y, zero if x and y are equal, or greater
        /// than zero if x is greater than y.
        /// </returns>
        public int Compare(object x, object y)
        {
            if (x is T item1 && y is T item2)
            {
                return comparer(item1, item2);
            }
            return 0;
        }
        #endregion
    }
}