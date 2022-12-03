using BeanFramework.core;
using DesktopTools;
using System.Windows;
using System.Windows.Threading;

namespace DesktopToolsTest
{
    [SetUpFixture, Apartment(ApartmentState.STA)]
    internal class SetUpTest
    {
        public static Context context = new Context();
        [OneTimeSetUp]
        public void Setup()
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
            var mw = new Window()
            {
                Height = 30,
                Width = 30,
                Title = "Test",
                ShowActivated = false,
                ShowInTaskbar = false,
                Top = 0,
                Left = 0,
            };
            _ = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown,
                MainWindow = mw
            };
            mw.Show();
            context.Start(typeof(App));
        }
        [OneTimeTearDown]
        public void TearDown()
        {
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }
}
