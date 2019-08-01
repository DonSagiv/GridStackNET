using System.Windows;
using GridStackNET.ViewModels;

namespace GridStackNET
{
    public partial class MainWindow : Window
    {
        #region ViewModels
        private MainWindowViewModel _viewModel;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
        }
    }
}
