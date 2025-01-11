using AdminSERMAC.Core.Configuration;
using AdminSERMAC.Core.Interfaces;
using AdminSERMAC.Forms;
using AdminSERMAC.Repositories;
using AdminSERMAC.Services;
using AdminSERMAC.Services.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var services = new ServiceCollection();
        var connectionString = "Data Source=AdminSERMAC.db;Version=3;";

        ConfigureServices(services, connectionString);

        var serviceProvider = services.BuildServiceProvider();

        try
        {
            var mainLogger = serviceProvider.GetRequiredService<ILogger<MainForm>>();
            var sqliteService = serviceProvider.GetRequiredService<SQLiteService>();
            var inventarioService = serviceProvider.GetRequiredService<IInventarioDatabaseService>();
            var clienteService = serviceProvider.GetRequiredService<IClienteService>();

            // Inicializar la base de datos
            var databaseService = serviceProvider.GetRequiredService<DatabaseService>();
            EnsureDatabaseInitialized(databaseService, mainLogger);

            Application.Run(new MainForm(
                clienteService,
                sqliteService,
                mainLogger,
                serviceProvider.GetRequiredService<ILogger<SQLiteService>>(),
                serviceProvider.GetRequiredService<ILoggerFactory>(),
                inventarioService));
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ProgramLogger>>();
            logger.LogError(ex, "Error fatal al iniciar la aplicación");

            MessageBox.Show(
                $"Error al iniciar la aplicación: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        // Configurar logging
        services.AddLogging(configure =>
        {
            configure.AddConsole();
            configure.AddDebug();
            configure.SetMinimumLevel(LogLevel.Information);
        });

        // Registrar servicios
        services.AddSingleton<SQLiteService>();

        services.AddScoped<IInventarioDatabaseService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<InventarioDatabaseService>>();
            return new InventarioDatabaseService(logger, connectionString);
        });

        services.AddScoped<IClienteRepository>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ClienteRepository>>();
            return new ClienteRepository(connectionString, logger);
        });

        services.AddScoped<IClienteService>(provider =>
        {
            var repository = provider.GetRequiredService<IClienteRepository>();
            var logger = provider.GetRequiredService<ILogger<ClienteService>>();
            return new ClienteService(repository, logger, connectionString);
        });

        RegisterDatabaseServices(services, connectionString);

        services.AddSingleton<NotificationService>();
        services.AddScoped<FileDataManager>();
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<DatabaseInitializer>();

        // Registrar logger específico para Program
        services.AddSingleton<ILogger<ProgramLogger>>(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger<ProgramLogger>();
        });
    }

    private static void RegisterDatabaseServices(IServiceCollection services, string connectionString)
    {
        services.AddScoped<IInventarioDatabaseService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<InventarioDatabaseService>>();
            return new InventarioDatabaseService(logger, connectionString);
        });
    }

    private static void EnsureDatabaseInitialized(DatabaseService databaseService, ILogger logger)
    {
        try
        {
            databaseService.InitializeDatabase();
            logger.LogInformation("Base de datos inicializada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al inicializar la base de datos.");
            throw;
        }
    }
}
