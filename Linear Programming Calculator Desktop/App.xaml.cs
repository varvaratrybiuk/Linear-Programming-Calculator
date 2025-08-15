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
            services.AddSingleton<LinearProgramInputStore>();
            services.AddSingleton<IProblemFormatterService, ProblemFormatterService>();
            services.AddSingleton<IOptimalResultSummaryService, OptimalResultSummaryService>();
            services.AddSingleton<IGomoryCutFormatterService, GomoryCutFormatterService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton((s) => new MainWindow()
            {
                DataContext = s.GetRequiredService<MainViewModel>()
            });

            services.AddScoped<EquationInputViewModel>();
            services.AddScoped<StartViewModel>();
            services.AddScoped<ResultsViewModel>();

            services.AddSingleton<INavigator<StartViewModel>, NavigationService<StartViewModel>>(sp =>
                new NavigationService<StartViewModel>(
                    sp.GetRequiredService<NavigationStore>(),
                    () => sp.GetRequiredService<StartViewModel>()
                )
            );

            services.AddScoped<INavigator<EquationInputViewModel>>(sp =>
               new NavigationService<EquationInputViewModel>(
                   sp.GetRequiredService<NavigationStore>(),
                   () => ActivatorUtilities.CreateInstance<EquationInputViewModel>(sp)
               )
           );

            services.AddScoped<INavigator<EquationInputViewModel, (int variables, int constraints)>>(sp =>
                new NavigationService<EquationInputViewModel, (int variables, int constraints)>(
                    sp.GetRequiredService<NavigationStore>(),
                    parameters => ActivatorUtilities.CreateInstance<EquationInputViewModel>(sp, parameters)
                )
            );

            services.AddScoped<INavigator<ResultsViewModel, LinearProgramResultDto>>(sp =>
               new NavigationService<ResultsViewModel, LinearProgramResultDto>(
                   sp.GetRequiredService<NavigationStore>(),
                   parameters => ActivatorUtilities.CreateInstance<ResultsViewModel>(sp, parameters)
               )
           );


            _serviceProvider = services.BuildServiceProvider();
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            _ = _serviceProvider.GetRequiredService<NavigationStore>();
            var starter = _serviceProvider.GetRequiredService<INavigator<StartViewModel>>();
            starter.Navigate();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }

}
