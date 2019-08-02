using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GridStackNET.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Fields
        private int _number;
        private ObservableCollection<UIElement> _gridElements;
        #endregion

        #region Delegates
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Properties
        public ObservableCollection<UIElement> gridElements
        {
            get { return _gridElements; }
            set { _gridElements = value; raisePropertyChanged(nameof(gridElements)); }
        }
        #endregion

        #region Commands
        public ICommand addItemCommand { get; set; }
        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            gridElements = new ObservableCollection<UIElement>();

            addItemCommand = new DelegateCommand(o => addItem());
        }
        #endregion

        #region Methods
        public void addItem()
        {
            var border = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Background = Brushes.Transparent,
                Child = new TextBlock
                {
                    Text = $"This is box {++_number}",
                    FontSize = 16,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            gridElements.Add(border);
        }

        public void raisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
            : this(execute, null) { }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
}