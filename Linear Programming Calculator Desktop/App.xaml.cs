using Linear_Programming_Calculator_Desktop.DTOs;
using Linear_Programming_Calculator_Desktop.Services;
using Linear_Programming_Calculator_Desktop.Stores;
using Linear_Programming_Calculator_Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Linear_Programming_Calculator_Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        public App()
        {
            var services = new ServiceCollection();

            services.AddSingleton<NavigationStore>();
            services.AddSingleton<IProblemFormatterService, ProblemFormatterService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton((s) => new MainWindow()
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });

            services.AddScoped<EquationInputViewModel>();
            services.AddScoped<StartViewModel>();
            services.AddScoped<ResultsViewModel>();

            services.AddSingleton<INavigator, NavigationService<StartViewModel>>(sp =>
                new NavigationService<StartViewModel>(
                    sp.GetRequiredService<NavigationStore>(),
                    () => sp.GetRequiredService<StartViewModel>()
                )
            );

            services.AddScoped<INavigator<(int variables, int constraints)>>(sp =>
                new NavigationService<(int variables, int constraints), EquationInputViewModel>(
                    sp.GetRequiredService<NavigationStore>(),
                    parameters => ActivatorUtilities.CreateInstance<EquationInputViewModel>(sp, parameters)
                )
            );

            services.AddScoped<INavigator<LinearProgramResultDto>>(sp =>
               new NavigationService<LinearProgramResultDto, ResultsViewModel>(
                   sp.GetRequiredService<NavigationStore>(),
                   parameters => ActivatorUtilities.CreateInstance<ResultsViewModel>(sp, parameters)
               )
           );


            _serviceProvider = services.BuildServiceProvider();
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            var navStore = _serviceProvider.GetRequiredService<NavigationStore>();
            var starter = _serviceProvider.GetRequiredService<INavigator>();
            starter.Navigate();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }

}
