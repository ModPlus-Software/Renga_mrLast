namespace mrLast;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        Title = ModPlusAPI.Language.GetPluginLocalName(ModPlusConnector.Instance);
    }
}