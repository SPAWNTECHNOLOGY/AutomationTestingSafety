using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutomationTestingSafety.Database.Migration;

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

            // Замените строку подключения на свою
            string connectionString = @"Data Source=DESKTOP-L61C90H;Initial Catalog=AutomationTestingSafety;Integrated Security=True;";

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
