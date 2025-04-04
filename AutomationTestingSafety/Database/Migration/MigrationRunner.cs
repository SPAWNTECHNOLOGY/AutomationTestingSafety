using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTestingSafety.Database.Migration
{
    public class MigrationRunner
    {
        private readonly string _connectionString;

        public MigrationRunner(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void RunMigration()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Создание таблицы Positions, если она не существует
                var createPositionsTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Positions')
                    BEGIN
                        CREATE TABLE Positions
                        (
                            PositionID INT PRIMARY KEY IDENTITY(1,1),
                            PositionName NVARCHAR(200) NOT NULL
                        )
                    END";
                using (var command = new SqlCommand(createPositionsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Создание таблицы Users, если она не существует
                var createUsersTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                    BEGIN
                        CREATE TABLE Users
                        (
                            UserID INT PRIMARY KEY IDENTITY(1,1),
                            FullName NVARCHAR(200) NOT NULL,
                            Login NVARCHAR(100) NOT NULL,
                            Password NVARCHAR(100) NOT NULL,
                            RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
                            PositionID INT NOT NULL,
                            CONSTRAINT FK_Users_Positions FOREIGN KEY (PositionID)
                                REFERENCES Positions(PositionID)
                        )
                    END";
                using (var command = new SqlCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
