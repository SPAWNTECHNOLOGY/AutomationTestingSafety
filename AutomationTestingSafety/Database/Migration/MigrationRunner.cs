using System;
using System.Data.SqlClient;

namespace AutomationTestingSafety.Database
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

                // Создание таблицы Должности, если её нет
                string createPositionsTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Должности')
                    BEGIN
                        CREATE TABLE Должности
                        (
                            ID_Должности INT PRIMARY KEY IDENTITY(1,1),
                            НазваниеДолжности NVARCHAR(200) NOT NULL
                        )
                    END";
                using (var command = new SqlCommand(createPositionsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Создание таблицы Пользователи, если её нет
                string createUsersTable = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Пользователи')
                    BEGIN
                        CREATE TABLE Пользователи
                        (
                            ID_Пользователя INT PRIMARY KEY IDENTITY(1,1),
                            ФИО NVARCHAR(200) NOT NULL,
                            Логин NVARCHAR(100) NOT NULL,
                            Пароль NVARCHAR(100) NOT NULL,
                            ДатаРегистрации DATETIME NOT NULL DEFAULT GETDATE(),
                            ID_Должности INT NOT NULL,
                            CONSTRAINT FK_Пользователи_Должности FOREIGN KEY (ID_Должности)
                                REFERENCES Должности(ID_Должности)
                        )
                    END";
                using (var command = new SqlCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Добавление должностей
                InsertPositionIfNotExists(connection, "Сотрудник");
                InsertPositionIfNotExists(connection, "Специалист");
                InsertPositionIfNotExists(connection, "Администратор");

                // Добавление пользователей
                InsertUserIfNotExists(connection, "employee", "employee123", "Иван Иванов", "Сотрудник");
                InsertUserIfNotExists(connection, "specialist", "specialist123", "Пётр Петров", "Специалист");
                InsertUserIfNotExists(connection, "admin", "admin123", "Алексей Алексеев", "Администратор");
            }
        }

        private void InsertPositionIfNotExists(SqlConnection connection, string positionName)
        {
            string query = @"
                IF NOT EXISTS (SELECT * FROM Должности WHERE НазваниеДолжности = @positionName)
                BEGIN
                    INSERT INTO Должности (НазваниеДолжности) VALUES (@positionName)
                END";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@positionName", positionName);
                command.ExecuteNonQuery();
            }
        }

        private void InsertUserIfNotExists(SqlConnection connection, string login, string password, string fio, string positionName)
        {
            string query = @"
                IF NOT EXISTS (SELECT * FROM Пользователи WHERE Логин = @login)
                BEGIN
                    DECLARE @positionID INT;
                    SELECT @positionID = ID_Должности 
                    FROM Должности 
                    WHERE НазваниеДолжности = @positionName;

                    INSERT INTO Пользователи (ФИО, Логин, Пароль, ID_Должности)
                    VALUES (@fio, @login, @password, @positionID)
                END";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@fio", fio);
                command.Parameters.AddWithValue("@positionName", positionName);
                command.ExecuteNonQuery();
            }
        }
    }
}
