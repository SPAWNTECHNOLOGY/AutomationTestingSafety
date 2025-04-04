using System;
using System.Windows;
using AutomationTestingSafety.Database;

namespace AutomationTestingSafety
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string connectionString = ConnectionString._connectionString;

            var migration = new MigrationRunner(connectionString);
            try
            {
                migration.RunMigration();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Migration failed: " + ex.Message);
            }
        }
    }
}
