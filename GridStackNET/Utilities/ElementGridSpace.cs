using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GridStackNET.Utilities
{
    /// <summary>
    /// A class that describes the current state of a UI element in a Grid control in WPF
    /// </summary>
    public class ElementGridSpace
    {
        #region Properties
        /// <summary>
        /// UI Element this ElementGridSpace describes
        /// </summary>
        public UIElement element { get; }

        /// <summary>
        /// Left-most column this element occupies
        /// </summary>
        public int leftColumn { get; }

        /// <summary>
        /// Top-most row this element occupies
        /// </summary>
        public int topRow { get; }

        /// <summary>
        /// Right-most column this element occupies
        /// </summary>
        public int rightColumn { get; }

        /// <summary>
        /// Bottom-most row this element occupies
        /// </summary>
        public int bottomRow { get; }

        /// <summary>
        /// List of (column, row) coordinates on the grid that this element occupies in the parent grid.
        /// </summary>
        public IList<int[]> occupiedGridSpaces { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an ElementGridSpace from an object within a Grid control.
        /// </summary>
        /// <param name="element">Element to get Grid parameters from.</param>
        public ElementGridSpace(UIElement element) : this(Grid.GetColumn(element),
            Grid.GetRow(element),
            Grid.GetColumnSpan(element),
            Grid.GetRowSpan(element))
        {
            this.element = element;
        }

        /// <summary>
        /// WPF Grid control uses spans, so this converts the spans into right/bottom-most column/row this element occupies in the Grid control.
        /// </summary>
        /// <param name="column">Left-most column of element</param>
        /// <param name="row">Top-most row of element</param>
        /// <param name="columnSpan">Element's column span</param>
        /// <param name="rowSpan">Element's row span</param>
        public ElementGridSpace(int column, int row, int columnSpan, int rowSpan)
        {
            leftColumn = column;
            topRow = row;
            rightColumn = leftColumn + columnSpan - 1;
            bottomRow = topRow + rowSpan - 1;

            // Creation of list of coordinates this UI element
            occupiedGridSpaces = new List<int[]>();

            for (var i = leftColumn; i <= rightColumn; i++)
                for (var j = topRow; j <= bottomRow; j++)
                    occupiedGridSpaces.Add(new[] { i, j });
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if the elementGridSpace occupies any of the same grid-space as this element.
        /// </summary>
        /// <param name="elementGridSpace">Element grid space to compare</param>
        /// <returns>True if this and the input elements overlay each other</returns>
        public bool intersects(ElementGridSpace elementGridSpace)
        {
            // For efficiency:
            // This is an O(N^2) complexity operation
            // Reduce complexity by iterating over smaller grid space
            return occupiedGridSpaces.Count >= elementGridSpace.occupiedGridSpaces.Count 
                ? isOverlaid(elementGridSpace, this) 
                : isOverlaid(this, elementGridSpace);
        }

        /// <summary>
        /// Determines if this grid space occupies the input coordinates.
        /// </summary>
        /// <param name="column">Input column coordinate</param>
        /// <param name="row">Input row coordinate</param>
        /// <returns>True if this element occupies the input grid coordinates</returns>
        private bool hasCoordinates(int column, int row)
        {
            return occupiedGridSpaces
                .Where(x => x[0] == column)
                .Any(x => x[1] == row);
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Determines if the two input elements occupy any of the same grid-space as each other.
        /// For efficiency purposes, since this is an O(N^2) complexity operation,
        /// The smaller element should be input first.
        /// </summary>
        /// <param name="smallerTestElement">Smaller grid element</param>
        /// <param name="largerTestElement">Larger grid element</param>
        /// <returns>True of the two elements overlay each other on the gird.</returns>
        private static bool isOverlaid(ElementGridSpace smallerTestElement, ElementGridSpace largerTestElement)
        {
            // This will iterate through the smaller grid element's coordinates, and determines if
            // it has any of the larger element's grid coordinates match them.
            for(var i = smallerTestElement.leftColumn; i <= smallerTestElement.rightColumn; i++)
                for (var j = smallerTestElement.topRow; j <= smallerTestElement.bottomRow; j++)
                    if (largerTestElement.hasCoordinates(i, j)) return true;

            return false;
        }
        #endregion
    }
}