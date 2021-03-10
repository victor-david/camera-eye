using Restless.App.Camera.Core;
using Restless.App.Database.Core;
using Restless.Toolkit.Controls;
using Restless.Toolkit.Core.Database.SQLite;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;

namespace Restless.App.Camera
{
    /// <summary>
    /// Extends <see cref="ApplicationViewModel"/> to provide common functionality for views that use DataGrid to display table rows. This class must be interited.
    /// </summary>
    /// <typeparam name="T">The table type derived from <see cref="TableBase"/></typeparam>
    public abstract class DataGridViewModel<T> : ApplicationViewModel where T : TableBase
    {
        #region Private
        private object selectedItem;
        private IList selectedDataGridItems;
        #endregion

        /************************************************************************/

        #region Public properties
        /// <summary>
        /// Gets the table object associated with this instance
        /// </summary>
        public T Table
        {
            get => DatabaseController.Instance.GetTable<T>();
        }

        /// <summary>
        /// Gets the main data view, the one associated with the <see cref="TableBase"/> type used to create this class.
        /// </summary>
        public DataView MainView
        {
            get;
        }

        /// <summary>
        /// Gets the list view. The UI binds to this property
        /// </summary>
        public ListCollectionView ListView
        {
            get;
        }

        /// <summary>
        /// Gets the collection of columns. The view binds to this property so that columns can be manipulated from the VM
        /// </summary>
        public DataGridColumnCollection Columns
        {
            get;
        }

        /// <summary>
        /// Gets the collection of menu items. The view binds to this collection so that VMs can manipulate menu items programatically
        /// </summary>
        public MenuItemCollection MenuItems
        {
            get;
        }

        /// <summary>
        /// Gets a boolean value that indicates if an item is selected,
        /// </summary>
        public bool IsItemSelected
        {
            get => SelectedItem != null;
        }

        /// <summary>
        /// Gets a boolean value that indicates if a single item is selected.
        /// </summary>
        public bool SingleItemSelected
        {
            get => SelectedItem != null && selectedDataGridItems?.Count == 1; // (selectedDataGridItems != null && SelectedItem != null) && selectedDataGridItems.Count == 1;
        }

        /// <summary>
        /// Gets a boolean value that indicates if multiple items are selected.
        /// </summary>
        public bool MultipleItemsSelected
        {
            get => selectedDataGridItems?.Count > 1;
        }

        /// <summary>
        /// Gets or sets the selected item of the DataGrid.
        /// </summary>
        public object SelectedItem
        {
            get => selectedItem;
            set
            {
                if (SetProperty(ref selectedItem, value))
                {
                    OnPropertyChanged(nameof(IsItemSelected));
                    OnPropertyChanged(nameof(SelectedDataRow));
                    OnSelectedItemChanged();
                }
            }
        }

        /// <summary>
        /// Gets the selected data row
        /// </summary>
        public virtual DataRow SelectedDataRow
        {
            get
            {
                if (SelectedItem is DataRowView drv)
                {
                    return drv.Row;
                }
                return null;
            }
        }

        /// <summary>
        /// Sets the selected data grid items. This property is bound to the view.
        /// </summary>
        public IList SelectedDataGridItems
        {
            get => selectedDataGridItems;
            set
            {
                SetProperty(ref selectedDataGridItems, value);
                OnPropertyChanged(nameof(SingleItemSelected));
                OnPropertyChanged(nameof(MultipleItemsSelected));
                OnSelectedItemsChanged();
            }
        }

        /// <summary>
        /// Gets the primary key value of the selected row, or null if none
        /// (no selected row or no primary key column on the table)
        /// </summary>
        public object SelectedPrimaryKey
        {
            get
            {
                if (SelectedDataRow != null && Table.PrimaryKeyName != null)
                {
                    return SelectedDataRow[Table.PrimaryKeyName];
                }
                return null;
            }
        }
        #endregion

        /************************************************************************/

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridViewModel{T}"/> class.
        /// </summary>
        protected DataGridViewModel()
        {
            MainView = new DataView(Table);
            ListView = new ListCollectionView(MainView);
            using (ListView.DeferRefresh())
            {
                ListView.CustomSort = new GenericComparer<DataRowView>((x,y) => OnDataRowCompare(x.Row, y.Row));
                ListView.Filter = (item) => item is DataRowView view && OnDataRowFilter(view.Row);
            }
            Columns = new DataGridColumnCollection();
            MenuItems = new MenuItemCollection();
            MainView.ListChanged += (s, e) =>
            {
                OnDataViewListChanged(e);
            };
        }
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Refreshes the <see cref="ListView"/> property.
        /// Override in a derived class to perform other refresh operations.
        /// Always call the base method.
        /// </summary>
        public virtual void Refresh()
        {
            //if (Dispatcher.CheckAccess())
            {
                ListView.Refresh();
                OnPropertyChanged(nameof(SingleItemSelected));
                OnPropertyChanged(nameof(MultipleItemsSelected));
            }
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Override in a derived class to perform actions when the <see cref="SelectedItem"/> property changes.
        /// The base implementation does nothing.
        /// </summary>
        protected virtual void OnSelectedItemChanged()
        {
        }

        /// <summary>
        /// Override in a derived class to perform actions when the <see cref="SelectedDataGridItems"/> property changes.
        /// The base implementation does nothing.
        /// </summary>
        protected virtual void OnSelectedItemsChanged()
        {
        }

        /// <summary>
        /// Override in a derived class to receive notification when <see cref="MainView"/> changes.
        /// The base implementation does nothing.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnDataViewListChanged(ListChangedEventArgs e)
        {
        }

        /// <summary>
        /// Override in a derived class to filter <see cref="ListView"/>. The base implementation returns true.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true if <paramref name="item"/> is included; otherwise, false.</returns>
        protected virtual bool OnDataRowFilter(DataRow item)
        {
            return true;
        }

        /// <summary>
        /// Override in a derived class to compares two specified <see cref="DataRow"/> objects.
        /// The base method returns zero.
        /// </summary>
        /// <param name="item1">The first data row</param>
        /// <param name="item2">The second data row</param>
        /// <returns>An integer value 0, 1, or -1</returns>
        protected virtual int OnDataRowCompare(DataRow item1, DataRow item2)
        {
            return 0;
        }

        /// <summary>
        /// Shortcut method. Compares two data rows in a stringwise fashion.
        /// </summary>
        /// <param name="item1">The first data row.</param>
        /// <param name="item2">The second data row.</param>
        /// <param name="columnName">The name of the column to compare</param>
        /// <returns>A string comparison result (zero, 1, or -1)</returns>
        protected int DataRowCompareString(DataRow item1, DataRow item2, string columnName)
        {
            return string.Compare(item1[columnName].ToString(), item2[columnName].ToString());
        }

        /// <summary>
        /// Shortcut method. Compares two data rows in a boolean fashion.
        /// </summary>
        /// <param name="item1">The first data row.</param>
        /// <param name="item2">The second data row.</param>
        /// <param name="columnName">The name of the column to compare</param>
        /// <returns>A string comparison result (zero, 1, or -1)</returns>
        protected int DataRowCompareBoolean(DataRow item1, DataRow item2, string columnName)
        {
            return ((bool)item1[columnName]).CompareTo((bool)item2[columnName]);
        }

        /// <summary>
        /// Shortcut method. Compares two data rows in a long integer fashion.
        /// </summary>
        /// <param name="item1">The first data row.</param>
        /// <param name="item2">The second data row.</param>
        /// <param name="columnName">The name of the column to compare</param>
        /// <returns>A string comparison result (zero, 1, or -1)</returns>
        protected int DataRowCompareLong(DataRow item1, DataRow item2, string columnName)
        {
            return ((long)item1[columnName]).CompareTo((long)item2[columnName]);
        }

        /// <summary>
        /// Shortcut method. Compares two data rows in a date/time fashion.
        /// </summary>
        /// <param name="item1">The first data row.</param>
        /// <param name="item2">The second data row.</param>
        /// <param name="columnName">The name of the column to compare</param>
        /// <returns>A date/time comparison result (zero, 1, or -1)</returns>
        protected int DataRowCompareDateTime(DataRow item1, DataRow item2, string columnName)
        {
            return DateTime.Compare((DateTime)item1[columnName], (DateTime)item2[columnName]);
        }

        /// <summary>
        /// Gets the primary id from the selected row of this controller's owner.
        /// </summary>
        /// <returns>The string primary id from the selected row of this controller's owner, or null if none.</returns>
        protected string GetOwnerSelectedPrimaryIdString()
        {
            DataGridViewModel<T> owner =  GetOwner<DataGridViewModel<T>>();
            if (owner != null && owner.SelectedDataRow != null)
            {
                if (owner.SelectedPrimaryKey is string str)
                {
                    return str;
                }
            }
            return null;
        }


        /// <summary>
        /// A helper method for derived classes when setting up commands that must have an item selected.
        /// </summary>
        /// <param name="parm">Not used. Satisfies ICommand</param>
        /// <returns>true if <see cref="IsItemSelected"/> is true; otherwise, false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Signature required")]
        protected bool CanRunIfIsItemSelected(object parm)
        {
            return IsItemSelected;
        }

        /// <summary>
        /// A helper method for derived classes when setting up commands that must have a single (not multiple) item selected.
        /// </summary>
        /// <param name="parm">Not used. Satisfies ICommand</param>
        /// <returns>true if <see cref="SingleItemSelected"/> is true; otherwise, false.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Signature required")]
        protected bool CanRunIfSingleItemSelected(object parm)
        {
            return SingleItemSelected;
        }

        /// <summary>
        /// Called when the view model is closing.
        /// </summary>
        protected override void OnClosing()
        {
            Columns.Clear();
            MenuItems.Clear();
            base.OnClosing();
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private void SelectedItems2BatchEnded(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SingleItemSelected));
            OnPropertyChanged(nameof(MultipleItemsSelected));
            OnSelectedItemsChanged();
        }
        #endregion
    }
}