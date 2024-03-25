using System.Windows;

namespace DialogGraph
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }
        
        
        protected override void OnStartup(StartupEventArgs e)
        {
            AppInitializer.Instance.InitializeApp();

            base.OnStartup(e);
        }
    }
}