using DotNetBrowser.Browser;
using DotNetBrowser.Engine;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp.TestDotNetBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IBrowser browser;
        private IEngine engine;
        private bool isInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InitBrowser();
                isInitialized = true;
                LoadUrl();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(Application.Current.MainWindow, $"Failed to init: {ex.Message}");
                isInitialized = false;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUrl();
        }

        private void LoadUrl()
        {
            if (!isInitialized)
            {
                MessageBox.Show(Application.Current.MainWindow, $"Enter license and click Init Engine first");
                return;
            }
            btnRefresh.IsEnabled = false;
            browser.Navigation.LoadUrl(txtUrl.Text);
        }

        private void InitBrowser()
        {
            string licenseKey = txtLicenseKey.Text;
            Task.Run(() =>
            {
                engine = EngineFactory.Create(new EngineOptions.Builder
                {

                    RenderingMode = RenderingMode.HardwareAccelerated,
                    LicenseKey = licenseKey
                }
                .Build()); ;
                browser = engine.CreateBrowser();
            }).Wait();

            engine.Network.HttpAuthPreferences.ServerWhiteList = "*";
            browser.Navigation.LoadFinished += Navigation_LoadFinished;
            webView.InitializeFrom(browser);
        }

        private void Navigation_LoadFinished(object sender, DotNetBrowser.Navigation.Events.LoadFinishedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                btnRefresh.IsEnabled = true;
            });

            Debug.WriteLine("Navigation_LoadFinished");
        }

        private void App_Closing(object sender, CancelEventArgs e)
        {
            if (isInitialized)
            {
                engine.CookieStore.Flush(); // Is it needed if not making any custom changes?!?
                engine.Dispose();
            }
        }
    }
}
