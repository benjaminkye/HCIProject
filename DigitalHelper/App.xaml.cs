using System.Configuration;
using System.Data;
using System.Windows;
using DigitalHelper.Services;
namespace DigitalHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static HelperWindow? HelperWindowInstance { get; private set; }
        public static MainWindow? MainWindowInstance { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LLMService.Instance.SetApiKey("PUT_API_KEY_HERE");
            
            await BrowserBridgeService.Instance.StartAsync();
            
            MainWindowInstance = new MainWindow();
            MainWindowInstance.Hide();
            this.MainWindow = MainWindowInstance;

            HelperWindowInstance = new HelperWindow();
            HelperWindowInstance.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await BrowserBridgeService.Instance.StopAsync();
            base.OnExit(e);
        }
    }

}
