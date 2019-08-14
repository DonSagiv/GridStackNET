using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GridStackNET.Utilities;

namespace GridStackNET
{
    public partial class GridStack : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty NumColumnsProperty = DependencyProperty.Register("NumColumns",
            typeof(int),
            typeof(UserControl),
            new FrameworkPropertyMetadata(5));

        public static readonly DependencyProperty MinRowsProperty = DependencyProperty.Register("MinRows",
            typeof(int),
            typeof(UserControl),
            new FrameworkPropertyMetadata(5));

        public static readonly DependencyProperty MinRowHeightProperty = DependencyProperty.Register("MinRowHeight",
            typeof(double),
            typeof(UserControl),
            new FrameworkPropertyMetadata(80d));

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register("ItemMargin",
            typeof(Thickness),
            typeof(UserControl),
            new FrameworkPropertyMetadata(new Thickness(5)));

        public static readonly DependencyProperty DefaultColumnSpanProperty = DependencyProperty.Register("DefaultColumnSpan",
            typeof(int),
            typeof(UserControl),
            new FrameworkPropertyMetadata(2));

        public static readonly DependencyProperty DefaultRowSpanProperty = DependencyProperty.Register("DefaultRowSpan",
            typeof(int),
            typeof(UserControl),
            new FrameworkPropertyMetadata(2));

        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register("Children",
            typeof(ObservableCollection<UIElement>),
            typeof(UserControl),
            new FrameworkPropertyMetadata(new ObservableCollection<UIElement>()));

        public static readonly DependencyProperty AutoAssignGridSpaceProperty = DependencyProperty.Register("AutoAssignGridSpace",
            typeof(bool),
            typeof(UserControl),
            new FrameworkPropertyMetadata(true));
        #endregion

        #region Fields
        private bool _isDragging;
        private bool _isInitialized;
        #endregion

        #region Properties
        /// <summary>
        /// Children items in the grid stack.
        /// </summary>
        public ObservableCollection<UIElement> Children
        {
            get { return (ObservableCollection<UIElement>)GetValue(ChildrenProperty); }
            set { SetValue(ChildrenProperty, value); }
        }

        /// <summary>
        /// Number of columns in the grid stack.
        /// </summary>
        public int NumColumns
        {
            get { return (int)GetValue(NumColumnsProperty); }
            set { SetValue(NumColumnsProperty, value); }
        }

        /// <summary>
        /// Minimum number of rows in the grid stack.
        /// </summary>
        public int MinRows
        {
            get { return (int)GetValue(MinRowsProperty); }
            set { SetValue(MinRowsProperty, value); }
        }

        /// <summary>
        /// Minimum row height of each element in the grid stack.
        /// </summary>
        public double MinRowHeight
        {
            get { return (double)GetValue(MinRowHeightProperty); }
            set { SetValue(MinRowHeightProperty, value); }
        }

        /// <summary>
        /// Column span of a new item to be added
        /// </summary>
        public int DefaultColumnSpan
        {
            get { return (int)GetValue(DefaultColumnSpanProperty); }
            set { SetValue(DefaultColumnSpanProperty, value); }
        }

        /// <summary>
        /// Row span of a new item to be added
        /// </summary>
        public int DefaultRowSpan
        {
            get { return (int)GetValue(DefaultRowSpanProperty); }
            set { SetValue(DefaultRowSpanProperty, value); }
        }

        /// <summary>
        /// Margin of each child item the in the grid stack.
        /// </summary>
        public Thickness ItemMargin
        {
            get { return (Thickness)GetValue(ItemMarginProperty); }
            set { SetValue(ItemMarginProperty, value); }
        }

        /// <summary>
        /// If true, GridStack will use default column and row spans when adding a new item,
        /// and automatically assign its row and column.
        ///
        /// If false, the element's Grid attached properties must be pre-defined.
        /// </summary>
        public bool AutoAssignGridSpace
        {
            get { return (bool)GetValue(AutoAssignGridSpaceProperty); }
            set { SetValue(AutoAssignGridSpaceProperty, value); }
        }
        #endregion

        #region Constructor
        public GridStack()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }
        #endregion

        #region Misc Methods
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isInitialized) return;

            // children = new ObservableCollection<UIElement>();
            Children.CollectionChanged += onChildrenChanged;

            addColumnDefinitions(NumColumns);
            addRowDefinition(MinRows);

            traceBorder.Visibility = Visibility.Hidden;

            // Ensures trace border is visible even if margin is set to 0
            var borderThickness = new Thickness(ItemMargin.Left < 3 ? 3 : ItemMargin.Left,
                ItemMargin.Top < 3 ? 3 : ItemMargin.Top,
                ItemMargin.Right < 3 ? 3 : ItemMargin.Right,
                ItemMargin.Bottom < 3 ? 3 : ItemMargin.Bottom);

            traceBorder.BorderThickness = borderThickness;

            parentGrid.SizeChanged += onGridSizeChanged;

            _isInitialized = true;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Used to calculate the new Column/row of a cell item when dragging the cell.
        /// </summary>
        /// <param name="cellValueBeforeDrag">Column/row before dragging.</param>
        /// <param name="cellSpan">Column/row span of cell element</param>
        /// <param name="numberOfDefinitions">Number of column/row definitions</param>
        /// <param name="dragDistance">Distance of dragged element from its original position (horizontal or vertical)</param>
        /// <param name="getCellSize">Delegate that provides the cell's width/height</param>
        /// <returns></returns>
        public static int getAdjustedCellValue(int cellValueBeforeDrag,
            int cellSpan,
            int numberOfDefinitions,
            double dragDistance,
            Func<int, double> getCellSize)
        {
            // Based on the cell's width/height an column/row before drag, calculate the position before dragging.
            var cellPositionBeforeDrag = 0d;

            for (var i = 0; i < cellValueBeforeDrag; i++)
            {
                var currentCellDimensionValue = getCellSize?.Invoke(cellValueBeforeDrag);

                if (!currentCellDimensionValue.HasValue)
                    throw new Exception("Could not get cell dimension width/height");

                cellPositionBeforeDrag += currentCellDimensionValue.Value;
            }

            // Calculate modified cell column/row after dragging.
            var adjustedCellPosition = cellPositionBeforeDrag + dragDistance;

            var remainingValue = adjustedCellPosition;
            var newCellDimensionValue = 0;

            // Determine new cell column/row based on new position
            // "Snaps" cells to column/row edge it's closest to (midpoint of column/row used as divider)
            while (remainingValue > getCellSize?.Invoke(cellValueBeforeDrag) / 2)
            {
                var currentCellDimensionValue = getCellSize?.Invoke(cellValueBeforeDrag);

                remainingValue -= currentCellDimensionValue.Value;
                newCellDimensionValue++;
            }

            // Limits the cell movement to the available column/row definition in the grids.
            // Prevents cells from moving out of bounds of the grid.
            return newCellDimensionValue > numberOfDefinitions - cellSpan ?
                numberOfDefinitions - cellSpan
                : newCellDimensionValue;
        }

        /// <summary>
        /// Used to calculate the new column/row spans of a cell item when re-sizing the cell.
        /// </summary>
        /// <param name="originalCellSize">Width/height of cell before resizing</param>
        /// <param name="cellSpan">Column/row span of </param>
        /// <param name="numberOfDefinitions">Number of column/row definitions</param>
        /// <param name="dragDistance">Distance of dragged element from its original position (horizontal or vertical)</param>
        /// <param name="getCellSize">Delegate that provides the cell's width/height</param>
        /// <returns></returns>
        private static int getAdjustedSpan(double originalCellSize,
            int cellSpan,
            int numberOfDefinitions,
            double dragDistance,
            Func<int, double> getCellSize)
        {
            // Gets the new cell size
            var adjustedCellSize = originalCellSize + dragDistance;

            var remainingValue = adjustedCellSize;
            var currentCell = cellSpan;
            var newSpanValue = 0;

            // Determine the new column/row span of the cell based on its adjusted size
            // "Snaps cells to cloumn/row edge it's closest to (midpoint of column/row used as divider)
            while (remainingValue > getCellSize?.Invoke(cellSpan) / 2)
            {
                if (currentCell >= numberOfDefinitions) break;

                var currentCellDimensionValue = getCellSize?.Invoke(cellSpan);

                remainingValue -= currentCellDimensionValue.Value;

                newSpanValue++;
            }

            // Prevents column/row span from exceeding limits of provided column/row definitions
            if (newSpanValue > numberOfDefinitions - cellSpan)
                newSpanValue = numberOfDefinitions - cellSpan;
            else if (newSpanValue == 0)
                newSpanValue = 1;

            return newSpanValue;
        }
        #endregion

        #region Column, Row Management
        private void addColumnDefinitions(int numColumns)
        {
            for (var i = 0; i < numColumns; i++)
            {
                parentGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }
        }

        public void addRowDefinition(int numRows = 1)
        {
            if (numRows < 1) return;

            for (var i = 0; i < numRows; i++)
            {
                var rowDefinition = new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star),
                    MinHeight = MinRowHeight,
                };

                parentGrid.RowDefinitions.Add(rowDefinition);
            }

            Dispatcher?.Invoke(() => { }, DispatcherPriority.Render);
        }

        private void removeRowDefinitions(int rowsToRemove)
        {
            if (rowsToRemove < 1) return;

            for (var i = 0; i < rowsToRemove; i++)
            {
                if (parentGrid.RowDefinitions.Count <= MinRows) break;
                parentGrid.RowDefinitions.Remove(parentGrid.RowDefinitions.LastOrDefault());
            }
        }

        private void autoAddRowDefinitions()
        {
            var maxRow = getPanelGridSpaces().Max(x => x.bottomRow);

            var rowsToAdd = maxRow - parentGrid.RowDefinitions.Count + 1;

            if (rowsToAdd > 0)
                addRowDefinition(rowsToAdd);
        }

        /// <summary>
        /// Clears empty rows on bottom of grid (up to _minrows)
        /// </summary>
        private void clearEmptyRowDefinitions()
        {
            if (!getPanelGridSpaces().Any()) return;

            var maxRow = getPanelGridSpaces().Max(x => x.bottomRow);
            var totalRowDefinitions = parentGrid.RowDefinitions.Count;
            var rowsToRemove = totalRowDefinitions - maxRow - 1;

            removeRowDefinitions(rowsToRemove);
        }

        private void onGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            adjustGridStackItemHeight();
        }
        #endregion

        #region Grid Item Management
        private void onChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    var elementToAdd = e.NewItems[0] as UIElement;
                    autoAddGridItem(elementToAdd, DefaultColumnSpan, DefaultRowSpan);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    var elementToRemove = e.OldItems[0] as UIElement;
                    removeGridItem(elementToRemove);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    removeAllGridItems();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void addGridItem(UIElement content, int column, int row, int colSpan, int rowSpan)
        {
            var borderToAdd = new Border
            {
                Margin = ItemMargin,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Child = content
            };

            borderToAdd.MouseEnter += activateAdorners;

            Grid.SetColumn(borderToAdd, column);
            Grid.SetRow(borderToAdd, row);
            Grid.SetColumnSpan(borderToAdd, colSpan);
            Grid.SetRowSpan(borderToAdd, rowSpan);

            Grid.SetColumn(borderToAdd.Child, column);
            Grid.SetRow(borderToAdd.Child, row);
            Grid.SetColumnSpan(borderToAdd.Child, colSpan);
            Grid.SetRowSpan(borderToAdd.Child, rowSpan);

            parentGrid.Children.Add(borderToAdd);

            autoAddRowDefinitions();
            adjustGridStackItemHeight();
        }

        private void removeGridItem(UIElement elementToRemove)
        {
            var borders = parentGrid.Children
                .OfType<Border>()
                .Where(x => x.Child == elementToRemove)
                .ToList();

            foreach (var border in borders)
                parentGrid.Children.Remove(border);

            clearEmptyRowDefinitions();
            adjustGridStackItemHeight();
        }

        private void removeAllGridItems()
        {
            var bordersToRemove = getPanelGridSpaces()
                .Where(x => x.element != traceBorder)
                .ToList();

            foreach (var borderGridSpace in bordersToRemove)
                parentGrid.Children.Remove(borderGridSpace.element);

            clearEmptyRowDefinitions();
            adjustGridStackItemHeight();
        }

        /// <summary>
        /// Finds an empty spot on the grid to place new grid item.
        /// </summary>
        /// <param name="defaultColSpan">Column span of new item</param>
        /// <param name="defaultRowSpan">Row span of new item</param>
        private void autoAddGridItem(UIElement content, int defaultColSpan, int defaultRowSpan)
        {
            int column, row, columnSpan, rowSpan;

            if (AutoAssignGridSpace)
            {
                column = 0;
                row = 0;
                columnSpan = defaultColSpan;
                rowSpan = defaultRowSpan;
            }
            else
            {
                column = Grid.GetColumn(content);
                row = Grid.GetRow(content);
                columnSpan = Grid.GetColumnSpan(content);
                rowSpan = Grid.GetRowSpan(content);
            }

            // Ensures the column span does not exceed the number of columns
            if (columnSpan > parentGrid.ColumnDefinitions.Count)
                columnSpan = parentGrid.ColumnDefinitions.Count;

            var j = row;

            while (true)
            {
                for (var i = 0; i < parentGrid.ColumnDefinitions.Count; i++)
                {
                    if (j == row && i < column) i = column;

                    var element = new ElementGridSpace(i, j, columnSpan, rowSpan);

                    if (findOverlayingElements(element).Any())
                        continue;

                    if (element.rightColumn >= parentGrid.ColumnDefinitions.Count)
                        continue;

                    addGridItem(content, i, j, columnSpan, rowSpan);

                    return;
                }

                j++;
            }
        }

        /// <summary>
        /// Creates an object with the cell dimensions and occupied grid spaces of each cell item.
        /// </summary>
        /// <returns>Descriptive dimensional information of each item in the grid.</returns>
        private IEnumerable<ElementGridSpace> getPanelGridSpaces()
        {
            return parentGrid.Children
                .OfType<Border>()
                .Where(x => x != traceBorder)
                .Select(x => new ElementGridSpace(x));
        }

        private void adjustGridStackItemHeight()
        {
            var gridHeight = parentGrid.ActualHeight;
            var rowDefinitions = parentGrid.RowDefinitions.Count;

            var borders = getPanelGridSpaces()
                .Select(x => x.element)
                .OfType<Border>()
                .ToList();

            foreach (var border in borders)
            {
                var rowSpan = Grid.GetRowSpan(border);
                border.MaxHeight = gridHeight / rowDefinitions * rowSpan - ItemMargin.Top - ItemMargin.Bottom;
            }
        }
        #endregion

        #region Adorner Management
        /// <summary>
        /// Shows adorners over the selected element
        /// </summary>
        private void activateAdorners(object sender, MouseEventArgs e)
        {
            if (_isDragging) return;

            clearAdorners();

            var border = sender as Border;

            if (border == null) return;

            var adornerLayers = AdornerLayer.GetAdornerLayer(border);
            var adorner = new SimpleAdorner(border);

            adorner.dragStarted += onDragStarted;
            adorner.centerDragging += onCenterAdornerDragging;
            adorner.edgeDragging += onEdgeAdornerDragging;
            adorner.dragCompleted += onDragCompleted;

            adornerLayers?.Add(adorner);
        }

        /// <summary>
        /// Clears all adorners
        /// </summary>
        private void clearAdorners()
        {
            var borders = parentGrid.Children
                .OfType<Border>()
                .Where(x =>
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(x);

                    if (adornerLayer?.GetAdorners(x) == null)
                        return false;

                    return adornerLayer.GetAdorners(x).OfType<SimpleAdorner>().Any();
                })
                .ToList();

            foreach (var borderWithAdorner in borders)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(borderWithAdorner);
                var simpleAdorners = adornerLayer.GetAdorners(borderWithAdorner).OfType<SimpleAdorner>();

                foreach (var simpleAdorner in simpleAdorners)
                    adornerLayer.Remove(simpleAdorner);
            }
        }

        /// <summary>
        /// Gets the adorner of the parent UI object
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>Parent element of adorner</returns>
        private Border getAdornerParentBorder(object sender)
        {
            var adorner = sender as SimpleAdorner;

            if (adorner == null) return null;

            var border = parentGrid.Children
                .OfType<Border>()
                .FirstOrDefault(x =>
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(x);
                    var adornerArray = adornerLayer?.GetAdorners(x);
                    return adornerArray != null && adornerArray.Contains(adorner);
                });

            return border;
        }
        #endregion

        #region Drag Event Handlers
        /// <summary>
        /// Occurs when adorner dragging starts
        /// </summary>
        private void onDragStarted(object sender, DragStartedEventArgs e)
        {
            var adorner = sender as Adorner;

            if (adorner == null) return;

            var border = getAdornerParentBorder(adorner);

            if (border == null) return;

            _isDragging = true;

            // Makes the "trace border" item visible and sets its grid parameters to the item to drag.
            if (traceBorder.Visibility == Visibility.Hidden)
                traceBorder.Visibility = Visibility.Visible;

            var column = Grid.GetColumn(border);
            var row = Grid.GetRow(border);
            var columnSpan = Grid.GetColumnSpan(border);
            var rowSpan = Grid.GetRowSpan(border);

            Grid.SetColumn(traceBorder, column);
            Grid.SetRow(traceBorder, row);
            Grid.SetColumnSpan(traceBorder, columnSpan);
            Grid.SetRowSpan(traceBorder, rowSpan);
        }

        /// <summary>
        /// Occurs while the mouse is moving when dragging an adorner
        /// </summary>
        private void onCenterAdornerDragging(object sender, DragDeltaEventArgs e)
        {
            var adorner = sender as Adorner;

            if (adorner == null) return;

            var border = getAdornerParentBorder(adorner);

            if (border == null) return;

            // Calculates the new trace border location based on the current mouse location when dragging
            var column = Grid.GetColumn(border);
            var row = Grid.GetRow(border);
            var columnSpan = Grid.GetColumnSpan(border);
            var rowSpan = Grid.GetRowSpan(border);

            var newColumn = getAdjustedCellValue(column,
                columnSpan,
                parentGrid.ColumnDefinitions.Count,
                e.HorizontalChange,
                i => parentGrid.ColumnDefinitions[i].ActualWidth);

            Grid.SetColumn(traceBorder, newColumn);

            // Original row definition is increased in case the item is dragged below the last row in the grid.
            // Upon dropping, the new rows will automatically be added
            var newRow = getAdjustedCellValue(row,
                rowSpan,
                parentGrid.RowDefinitions.Count + rowSpan,
                e.VerticalChange,
                i => parentGrid.RowDefinitions[i].ActualHeight);

            Grid.SetRow(traceBorder, newRow);
        }

        private void onEdgeAdornerDragging(object sender, DragDeltaEventArgs e)
        {
            var adorner = sender as Adorner;

            if (adorner == null) return;

            var border = getAdornerParentBorder(adorner);

            if (border == null) return;

            _isDragging = true;

            // Calculates the new trace border size based on the current mouse location when dragging
            var column = Grid.GetColumn(border);
            var row = Grid.GetRow(border);
            var columnSpan = Grid.GetColumnSpan(border);
            var rowSpan = Grid.GetRowSpan(border);

            Grid.SetColumn(traceBorder, column);
            Grid.SetRow(traceBorder, row);
            Grid.SetColumnSpan(traceBorder, columnSpan);
            Grid.SetRowSpan(traceBorder, rowSpan);

            var newColumnSpan = getAdjustedSpan(border.ActualWidth,
                column,
                parentGrid.ColumnDefinitions.Count,
                e.HorizontalChange,
                i => parentGrid.ColumnDefinitions[i].ActualWidth);

            Grid.SetColumnSpan(traceBorder, newColumnSpan);

            // Original row definition is increased in case the item is dragged below the last row in the grid.
            // Upon dropping, the new rows will automatically be added
            var newRowSpan = getAdjustedSpan(border.ActualHeight,
                row,
                parentGrid.RowDefinitions.Count,
                e.VerticalChange,
                i => parentGrid.RowDefinitions[i].ActualHeight);

            Grid.SetRowSpan(traceBorder, newRowSpan);
        }

        private void onDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDragging = false;

            if (traceBorder.Visibility == Visibility.Visible)
                traceBorder.Visibility = Visibility.Hidden;

            var border = getAdornerParentBorder(sender as Adorner);

            if (border == null) return;

            // Gets the grid parameters of the trace border
            var traceBorderColumn = Grid.GetColumn(traceBorder);
            var traceBorderRow = Grid.GetRow(traceBorder);
            var traceBorderColumnSpan = Grid.GetColumnSpan(traceBorder);
            var traceBorderRowSpan = Grid.GetRowSpan(traceBorder);

            moveTraceBorderOverlaidElements(border);

            // Sets the grid parameters of the parent item to that of the trace border
            Grid.SetColumn(border, traceBorderColumn);
            Grid.SetRow(border, traceBorderRow);
            Grid.SetColumnSpan(border, traceBorderColumnSpan);
            Grid.SetRowSpan(border, traceBorderRowSpan);

            Grid.SetColumn(border.Child, traceBorderColumn);
            Grid.SetRow(border.Child, traceBorderRow);
            Grid.SetColumnSpan(border.Child, traceBorderColumnSpan);
            Grid.SetRowSpan(border.Child, traceBorderRowSpan);

            clearAdorners();

            autoAddRowDefinitions();
            clearEmptyRowDefinitions();
            adjustGridStackItemHeight();
        }
        #endregion

        #region Overlaying Elements Management
        public void moveTraceBorderOverlaidElements(UIElement originalElement)
        {
            var traceBorderOverlayElements = findOverlayingElements(traceBorder, originalElement);

            if (!traceBorderOverlayElements.Any()) return;

            // Calculates new row of overlaid element
            var newRow = new ElementGridSpace(traceBorder).bottomRow + 1;

            foreach (var overlayingElement in traceBorderOverlayElements)
                moveOverlaidElementsDown(overlayingElement.element, newRow, originalElement);
        }

        public void moveOverlaidElementsDown(UIElement overlayElementInput, int newRow, params UIElement[] dontMoveElements)
        {
            // Sets the new row of the overlayElementInput
            Grid.SetRow(overlayElementInput, newRow);

            var inputOverlayBottomRow = new ElementGridSpace(overlayElementInput).bottomRow;

            // Recursively moves down any elements that overlayElementInput overlays.
            foreach (var overlayingElement in findOverlayingElements(overlayElementInput, dontMoveElements))
            {
                // Calculates new row of overlaid element
                var overlayElementNewRow = inputOverlayBottomRow + 1;
                moveOverlaidElementsDown(overlayingElement.element, overlayElementNewRow);
            }
        }

        /// <summary>
        /// Finds any elements that overlay this UI element
        /// </summary>
        /// <param name="element">Element that has (or doesn't have) overlaid elements</param>
        /// <param name="excludeElements">Elements to ignore even if they're overlaid with this element</param>
        /// <returns>List of element grid spaces that overlay the input "element" parameter</returns>
        private IList<ElementGridSpace> findOverlayingElements(UIElement element, params UIElement[] excludeElements)
        {
            var elementGridSpace = new ElementGridSpace(element);

            return getPanelGridSpaces()
                .Where(x => x.element != element)
                .Where(x => !excludeElements.Contains(x.element))
                .Where(x => elementGridSpace.intersects(x))
                .ToList();
        }

        /// <summary>
        /// Finds overlaying element of an element grid space.
        /// </summary>
        /// <param name="inputGridSpace">Element grid space to compare.</param>
        /// <returns></returns>
        private IList<ElementGridSpace> findOverlayingElements(ElementGridSpace inputGridSpace)
        {
            return getPanelGridSpaces()
                .Where(inputGridSpace.intersects)
                .ToList();
        }
        #endregion
    }
}