namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides an enumeration that describes the grid layout of the camera wall.
    /// </summary>
    public enum WallGridLayout
    {
        /// <summary>
        /// None. Layout has not been initialized.
        /// </summary>
        None = 0,
        /// <summary>
        /// One row, one column. One slot total, one camera has the whole window
        /// </summary>
        OneByOne = 1,
        /// <summary>
        /// Two rows, one column. Two slots total, one camera top, one camera bottom.
        /// </summary>
        TwoByOne = 2,
        /// <summary>
        /// Two rows, two columns. Four slots total.
        /// </summary>
        TwoByTwo = 4,
        /// <summary>
        /// Three rows, two columns. Six slots total.
        /// </summary>
        ThreeByTwo = 6,
        /// <summary>
        /// Three rows, three columns. Nine slots total.
        /// </summary>
        ThreeByThree = 9,
    }

    /// <summary>
    /// Provides static extension methods for <see cref="WallGridLayout"/>
    /// </summary>
    public static class WallGridLayoutHelper
    {
        /// <summary>
        /// Gets the number of rows associated with the specified <see cref="WallGridLayout"/>
        /// </summary>
        /// <param name="layout">The layout value.</param>
        /// <returns>The number of rows for <paramref name="layout"/>.</returns>
        public static int ToRow(this WallGridLayout layout)
        {
            return layout switch
            {
                WallGridLayout.OneByOne => 1,
                WallGridLayout.TwoByOne => 2,
                WallGridLayout.TwoByTwo => 2,
                WallGridLayout.ThreeByTwo => 3,
                WallGridLayout.ThreeByThree => 3,
                _ => 0,
            };
        }

        /// <summary>
        /// Gets the number of columns associated with the specified <see cref="WallGridLayout"/>
        /// </summary>
        /// <param name="layout">The layout value.</param>
        /// <returns>The number of columns for <paramref name="layout"/>.</returns>
        public static int ToColumn(this WallGridLayout layout)
        {
            return layout switch
            {
                WallGridLayout.OneByOne => 1,
                WallGridLayout.TwoByOne => 1,
                WallGridLayout.TwoByTwo => 2,
                WallGridLayout.ThreeByTwo => 2,
                WallGridLayout.ThreeByThree => 3,
                _ => 0,
            };
        }
    }
}