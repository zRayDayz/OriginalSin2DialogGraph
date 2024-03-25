using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GraphShape.Controls;
using GraphShape.Utils;

namespace DialogGraph;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private static MainWindow Instance;
    private const string HeaderPanelControlNameInTabControl = "HeaderPanel";
    private const double WindowMaximizeHeight = 700;
    private const double WindowMinimizeHeight = 28;

    private GraphCanvas graph;

    public MainWindow(NotifierObject mainWindowViewModel)
    {
        InitializeComponent();
        
        DataContext = mainWindowViewModel;
        IsWindowAlwaysOnTop = true;
        
        Instance = this;
        Loaded += ProcessAdditionalControlElements;

        previousHeight = Height;
        SizeChanged += WindowSizeChanged;
    }

    // FindName() does not find the TabPanel if this method is executed in the MainWindow constructor
    private void ProcessAdditionalControlElements(object sender, RoutedEventArgs args)
    {
        var mainTabPanel = this.MainTabControl.Template.FindName(HeaderPanelControlNameInTabControl, this.MainTabControl) as TabPanel;
        mainTabPanel.Background = new SolidColorBrush(Colors.White);
        mainTabPanel.MouseLeftButtonDown += TabPanel_MouseLeftButtonDown;

        graph = (GraphCanvas)this.FindName("GraphLayout");
    }
    
    private void TabPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        this.DragMove();
    }
    
    private void ScrollToEnd(object sender, TextChangedEventArgs args) => (sender as TextBox).ScrollToEnd();
    private void ToogleLogAutoScrollToEnd(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        if (checkBox.IsChecked == true) MainWindow.Instance.LogTextBox.TextChanged += ScrollToEnd;
        else MainWindow.Instance.LogTextBox.TextChanged -= ScrollToEnd;
    }

    private bool windowMinimized;
    private void ToggleWindowMinimization(object sender, RoutedEventArgs e)
    {
        if (windowMinimized) Height = WindowMaximizeHeight;
        else Height = WindowMinimizeHeight;
        windowMinimized = !windowMinimized;
    }
    
    private void ToggleDialogInfoVisibility(object sender, RoutedEventArgs e)
    {
        if (DialogInfo.Visibility == Visibility.Collapsed) DialogInfo.Visibility = Visibility.Visible;
        else DialogInfo.Visibility = Visibility.Collapsed;
    }
    
    private double previousHeight;
    private void WindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        double currentHeight = e.NewSize.Height;

        double delta = previousHeight - currentHeight;
        this.Top += delta;
        
        previousHeight = currentHeight;
    }
    
    private void RelayoutGraph(object sender, RoutedEventArgs e)
    {
        graph.Relayout();
    }
    
    private bool isWindowAlwaysOnTop;
    public bool IsWindowAlwaysOnTop
    {
        get => isWindowAlwaysOnTop;
        set
        {
            Topmost = value;
            isWindowAlwaysOnTop = value;
            OnPropertyChanged(nameof(IsWindowAlwaysOnTop));
        }
    }

    private void ExitProgram(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Do you really want to exit the program?", "Exit the program", MessageBoxButton.YesNo);
        if (result == MessageBoxResult.Yes) this.Close();
    }
    
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // This class allows to resolve Circular Dependencies for MainWindow, and encapsulates all methods for interacting with MainWindow Controls, additionally having access to all private members of the MainWindow class
    // There is no check of Singleton instance of MainWindow class for Null, because if instance is Null, why and how then ViewModel logic tries to call methods of the Controls in the first place, because in this case the Window is not even shown
    public class MainWindowActions : IMainWindowActions
    {
        public void AppendTextToLog(string text)
        {
            MainWindow.Instance.LogTextBox.AppendText(text);
        }
    }
}

