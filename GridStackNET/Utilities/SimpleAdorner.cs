using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GridStackNET.Utilities
{
    public class SimpleAdorner : Adorner
    {
        #region Fields
        private readonly VisualCollection _children;
        private readonly Thumb _center, _bottomRight;
        #endregion

        #region Delegates
        public event EventHandler<DragStartedEventArgs> dragStarted;
        public event EventHandler<DragDeltaEventArgs> centerDragging;
        public event EventHandler<DragDeltaEventArgs> edgeDragging;
        public event EventHandler<DragCompletedEventArgs> dragCompleted;
        #endregion

        #region Properties
        protected override int VisualChildrenCount => _children.Count;
        #endregion

        #region Constructor
        public SimpleAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _children = new VisualCollection(this);

            buildAdornerCenter(ref _center);
            buildAdornerEdge(ref _bottomRight, Cursors.SizeNWSE);

            _center.DragStarted += (s, e) => dragStarted?.Invoke(this, e);
            _bottomRight.DragStarted += (s, e) => dragStarted?.Invoke(this, e);

            _center.DragDelta += (s, e) => centerDragging?.Invoke(this, e);
            _bottomRight.DragDelta += (s, e) => edgeDragging?.Invoke(this, e);

            _center.DragCompleted += (s, e) => dragCompleted?.Invoke(this, e);
            _bottomRight.DragCompleted += (s, e) => dragCompleted?.Invoke(this, e);
        }

        #endregion

        #region Methods
        protected override Size ArrangeOverride(Size finalSize)
        {
            base.ArrangeOverride(finalSize);

            var renderWidth = finalSize.Width;
            var renderHeight = finalSize.Height;

            var centerRectangle = new Rect(0, 0, renderWidth, renderHeight);
            _center.Arrange(centerRectangle);

            _bottomRight.Arrange(new Rect(renderWidth - _bottomRight.Width, renderHeight - _bottomRight.Height, _bottomRight.Width, _bottomRight.Height));

            return finalSize;
        }

        private void buildAdornerCenter(ref Thumb centerThumb)
        {
            if (centerThumb != null) return;

            centerThumb = new Thumb
            {
                Cursor = Cursors.SizeAll,
                Opacity = 0.4,
                Background = new SolidColorBrush(Colors.Gray)
            };

            _children.Add(centerThumb);
        }

        private void buildAdornerEdge(ref Thumb edgeThumb, Cursor customizedCursor)
        {
            if (edgeThumb != null) return;

            edgeThumb = new Thumb
            {
                Cursor = customizedCursor,
                Height = 20,
                Width = 20,
                Opacity = 0.4,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Colors.DodgerBlue),
            };

            _children.Add(edgeThumb);
        }

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }
        #endregion
    }
}