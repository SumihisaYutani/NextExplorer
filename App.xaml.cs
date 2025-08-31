using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextExplorer.Services;
using NextExplorer.Repositories;
using NextExplorer.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace NextExplorer
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            ConfigureServices();
            
            var mainWindow = _serviceProvider?.GetRequiredService<MainWindow>();
            mainWindow?.Show();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
            });

            services.AddSingleton<ISessionRepository, SessionRepository>();
            services.AddSingleton<IWindowsExplorerService, WindowsExplorerService>();
            services.AddSingleton<IFolderSessionManager, FolderSessionManager>();
            
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }
    }
}