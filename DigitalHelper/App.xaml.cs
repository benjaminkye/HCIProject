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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            LLMService.Instance.SetApiKey("PUT-API-KEY-HERE");

            var mainWindow = new MainWindow();
            mainWindow.Hide();
            this.MainWindow = mainWindow;

            HelperWindowInstance = new HelperWindow();
            HelperWindowInstance.Show();
        }
    }

}
