﻿using System;
using System.Data.SqlClient;
using AutomationTestingSafety.Database.Migration;

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
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // ===============================
                        // 1) Таблица Должности
                        // ===============================
                        string createPositionsTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Должности')
BEGIN
    CREATE TABLE Должности
    (
        ID_Должности INT PRIMARY KEY IDENTITY(1,1),
        НазваниеДолжности NVARCHAR(200) NOT NULL
    )
END";
                        using (SqlCommand command = new SqlCommand(createPositionsTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // ===============================
                        // 2) Таблица Пользователи
                        // ===============================
                        string createUsersTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Пользователи')
BEGIN
    CREATE TABLE Пользователи
    (
        ID_Пользователя INT PRIMARY KEY IDENTITY(1,1),
        ФИО NVARCHAR(200) NOT NULL,
        ДатаРождения NVARCHAR(200) NOT NULL,
        Логин NVARCHAR(100) NOT NULL,
        Пароль NVARCHAR(100) NOT NULL,
        ДатаРегистрации DATETIME NOT NULL DEFAULT GETDATE(),
        ID_Должности INT NOT NULL
    )
END";
                        using (SqlCommand command = new SqlCommand(createUsersTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Внешний ключ Пользователи -> Должности (CASCADE)
                        string addFkUsersPositions = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Пользователи_Должности')
BEGIN
    ALTER TABLE Пользователи
    ADD CONSTRAINT FK_Пользователи_Должности
    FOREIGN KEY (ID_Должности)
    REFERENCES Должности(ID_Должности)
    ON DELETE CASCADE
END";
                        using (SqlCommand command = new SqlCommand(addFkUsersPositions, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Добавляем должности (IF NOT EXISTS внутри)
                        InsertPositionIfNotExists(connection, transaction, "Сотрудник");
                        InsertPositionIfNotExists(connection, transaction, "Специалист");
                        InsertPositionIfNotExists(connection, transaction, "Администратор");

                        // Добавляем пользователей (IF NOT EXISTS внутри)
                        InsertUserIfNotExists(connection, transaction, "1", "1", "Иван Иванов", "05.04.1999", "Сотрудник");
                        InsertUserIfNotExists(connection, transaction, "2", "2", "Пётр Петров", "02.24.2004", "Специалист");
                        InsertUserIfNotExists(connection, transaction, "3", "3", "Алексей Алексеев", "11.10.2000", "Администратор");

                        // Обязательно создаём (если нет) пользователя с ID=1,
                        // чтобы DEFAULT 1 в Тестах не вызывал конфликт.
                        EnsureUserWithID1(connection, transaction);

                        // ===============================
                        // 3) Таблица СтатусыТестов
                        // ===============================
                        string createTestStatusesTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'СтатусыТестов')
BEGIN
    CREATE TABLE СтатусыТестов
    (
        ID_СтатусаТеста INT PRIMARY KEY IDENTITY(1,1),
        СтатусТеста NVARCHAR(50) NOT NULL
    )
END";
                        using (SqlCommand command = new SqlCommand(createTestStatusesTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                        InsertTestStatusIfNotExists(connection, transaction, 1, "Черновик");
                        InsertTestStatusIfNotExists(connection, transaction, 2, "Активен");
                        InsertTestStatusIfNotExists(connection, transaction, 3, "В архиве");

                        // ===============================
                        // 4) Таблица Тесты
                        // ===============================
                        // Здесь указываем DEFAULT 1 для ID_Пользователя
                        string createTestsTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Тесты')
BEGIN
    CREATE TABLE Тесты
    (
        ID_Теста INT PRIMARY KEY IDENTITY(1,1),
        НазваниеТеста NVARCHAR(200) NOT NULL,
        Описание NVARCHAR(500) NULL,
        МинимальныйБалл INT NOT NULL DEFAULT 0,
        ID_СтатусаТеста INT NOT NULL DEFAULT 1,
        ID_Пользователя INT NOT NULL DEFAULT 1
    )
END";
                        using (SqlCommand command = new SqlCommand(createTestsTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Удаляем поле "Активен", если вдруг осталось
                        string dropActiveColumn = @"
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'Активен' AND Object_ID = OBJECT_ID(N'Тесты'))
BEGIN
    ALTER TABLE Тесты DROP COLUMN Активен
END";
                        using (SqlCommand command = new SqlCommand(dropActiveColumn, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Внешний ключ Тесты -> СтатусыТестов (NO ACTION)
                        string addFkTestsStatuses = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Тесты_СтатусыТестов')
BEGIN
    ALTER TABLE Тесты
    ADD CONSTRAINT FK_Тесты_СтатусыТестов
    FOREIGN KEY (ID_СтатусаТеста)
    REFERENCES СтатусыТестов(ID_СтатусаТеста)
    ON DELETE NO ACTION
END";
                        using (SqlCommand command = new SqlCommand(addFkTestsStatuses, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Внешний ключ Тесты -> Пользователи (CASCADE)
                        string addFkTestsUsers = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Тесты_Пользователи')
BEGIN
    ALTER TABLE Тесты
    ADD CONSTRAINT FK_Тесты_Пользователи
    FOREIGN KEY (ID_Пользователя)
    REFERENCES Пользователи(ID_Пользователя)
    ON DELETE CASCADE
END";
                        using (SqlCommand command = new SqlCommand(addFkTestsUsers, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // ===============================
                        // 5) Таблица Вопросы
                        // ===============================
                        string createQuestionsTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Вопросы')
BEGIN
    CREATE TABLE Вопросы
    (
        ID_Вопроса INT PRIMARY KEY IDENTITY(1,1),
        ТекстВопроса NVARCHAR(500) NOT NULL,
        ID_Теста INT NOT NULL
    )
END";
                        using (SqlCommand command = new SqlCommand(createQuestionsTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Внешний ключ Вопросы -> Тесты (CASCADE)
                        string addFkQuestionsTests = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Вопросы_Тесты')
BEGIN
    ALTER TABLE Вопросы
    ADD CONSTRAINT FK_Вопросы_Тесты
    FOREIGN KEY (ID_Теста)
    REFERENCES Тесты(ID_Теста)
    ON DELETE CASCADE
END";
                        using (SqlCommand command = new SqlCommand(addFkQuestionsTests, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // ===============================
                        // 6) Таблица ВариантыОтветов
                        // ===============================
                        string createAnswersTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ВариантыОтветов')
BEGIN
    CREATE TABLE ВариантыОтветов
    (
        ID_Варианта INT PRIMARY KEY IDENTITY(1,1),
        ТекстВарианта NVARCHAR(500) NOT NULL,
        Правильный BIT NOT NULL DEFAULT 0,
        Баллы INT NOT NULL DEFAULT 0,
        ID_Вопроса INT NOT NULL
    )
END";
                        using (SqlCommand command = new SqlCommand(createAnswersTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Внешний ключ ВариантыОтветов -> Вопросы (CASCADE)
                        string addFkAnswersQuestions = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_ВариантыОтветов_Вопросы')
BEGIN
    ALTER TABLE ВариантыОтветов
    ADD CONSTRAINT FK_ВариантыОтветов_Вопросы
    FOREIGN KEY (ID_Вопроса)
    REFERENCES Вопросы(ID_Вопроса)
    ON DELETE CASCADE
END";
                        using (SqlCommand command = new SqlCommand(addFkAnswersQuestions, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // ===============================
                        // 7) Таблица РезультатыТестов
                        // ===============================
                        string createResultsTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'РезультатыТестов')
BEGIN
    CREATE TABLE РезультатыТестов
    (
        ID_Результата INT PRIMARY KEY IDENTITY(1,1),
        ID_Пользователя INT NOT NULL,
        ID_Теста INT NOT NULL,
        ВремяПрохождения NVARCHAR(50) NOT NULL,
        НабранныеБалл INT NOT NULL,
        МинимальныйБалл INT NOT NULL,
        Статус NVARCHAR(50) NOT NULL,
        Детали NVARCHAR(MAX) NULL
    )
END";
                        using (SqlCommand command = new SqlCommand(createResultsTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Сбрасываем старые ключи, если вдруг были
                        string dropFkResultsUsers = @"
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_РезультатыТестов_Пользователи')
BEGIN
    ALTER TABLE РезультатыТестов DROP CONSTRAINT FK_РезультатыТестов_Пользователи
END";
                        using (SqlCommand command = new SqlCommand(dropFkResultsUsers, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }
                        string dropFkResultsTests = @"
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_РезультатыТестов_Тесты')
BEGIN
    ALTER TABLE РезультатыТестов DROP CONSTRAINT FK_РезультатыТестов_Тесты
END";
                        using (SqlCommand command = new SqlCommand(dropFkResultsTests, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Внешний ключ РезультатыТестов -> Тесты (CASCADE)
                        string addFkResultsTests = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_РезультатыТестов_Тесты')
BEGIN
    ALTER TABLE РезультатыТестов
    ADD CONSTRAINT FK_РезультатыТестов_Тесты
    FOREIGN KEY (ID_Теста)
    REFERENCES Тесты(ID_Теста)
    ON DELETE CASCADE
END";
                        using (SqlCommand command = new SqlCommand(addFkResultsTests, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Внешний ключ РезультатыТестов -> Пользователи (NO ACTION)
                        string addFkResultsUsers2 = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_РезультатыТестов_Пользователи')
BEGIN
    ALTER TABLE РезультатыТестов
    ADD CONSTRAINT FK_РезультатыТестов_Пользователи
    FOREIGN KEY (ID_Пользователя)
    REFERENCES Пользователи(ID_Пользователя)
    ON DELETE NO ACTION
END";
                        using (SqlCommand command = new SqlCommand(addFkResultsUsers2, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // ===============================
                        // 8) Вставка тестов, вопросов, ответов по умолчанию
                        // ===============================
                        string insertDefaultTests = QweryCreateTests.GetQweryCreateTest();
                        using (SqlCommand command = new SqlCommand(insertDefaultTests, connection, transaction))
                        {
                            // Если внутри GetQweryCreateTest есть IF NOT EXISTS, 
                            // то повторный запуск не дублирует записи.
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Ошибка при выполнении миграций: " + ex.Message, ex);
                    }
                }
            }
        }

        // -----------------------------
        // Метод, который гарантирует, что пользователь с ID=1 существует
        // -----------------------------
        private void EnsureUserWithID1(SqlConnection connection, SqlTransaction transaction)
        {
            // Если уже есть пользователь с ID=1, ничего не делаем
            string checkUser = @"
IF NOT EXISTS (SELECT * FROM Пользователи WHERE ID_Пользователя = 1)
BEGIN
    -- Нужно временно включить IDENTITY_INSERT, чтобы явно вставить ID=1
    SET IDENTITY_INSERT Пользователи ON;
    
    INSERT INTO Пользователи 
       (ID_Пользователя, ФИО, ДатаРождения, Логин, Пароль, ДатаРегистрации, ID_Должности)
    VALUES
       (1, N'Пользователь №1', N'01.01.2000', N'1', N'1', GETDATE(), 
        (SELECT TOP 1 ID_Должности FROM Должности ORDER BY ID_Должности));  
    
    SET IDENTITY_INSERT Пользователи OFF;
END
";
            using (SqlCommand command = new SqlCommand(checkUser, connection, transaction))
            {
                command.ExecuteNonQuery();
            }
        }

        private void InsertPositionIfNotExists(SqlConnection connection, SqlTransaction transaction, string positionName)
        {
            string query = @"
IF NOT EXISTS (SELECT * FROM Должности WHERE НазваниеДолжности = @positionName)
BEGIN
    INSERT INTO Должности (НазваниеДолжности) VALUES (@positionName)
END";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@positionName", positionName);
                command.ExecuteNonQuery();
            }
        }

        private void InsertUserIfNotExists(SqlConnection connection, SqlTransaction transaction, string login, string password, string fio, string dateBirthday, string positionName)
        {
            string query = @"
IF NOT EXISTS (SELECT * FROM Пользователи WHERE Логин = @login)
BEGIN
    DECLARE @positionID INT;
    SELECT @positionID = ID_Должности 
      FROM Должности 
      WHERE НазваниеДолжности = @positionName;
    
    INSERT INTO Пользователи (ФИО, ДатаРождения, Логин, Пароль, ID_Должности)
    VALUES (@fio, @dateBirthday, @login, @password, @positionID)
END";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@fio", fio);
                command.Parameters.AddWithValue("@positionName", positionName);
                command.Parameters.AddWithValue("@dateBirthday", dateBirthday);
                command.ExecuteNonQuery();
            }
        }

        private void InsertTestStatusIfNotExists(SqlConnection connection, SqlTransaction transaction, int statusId, string statusName)
        {
            string query = @"
IF NOT EXISTS (SELECT * FROM СтатусыТестов WHERE ID_СтатусаТеста = @statusId)
BEGIN
    SET IDENTITY_INSERT СтатусыТестов ON;
    INSERT INTO СтатусыТестов (ID_СтатусаТеста, СтатусТеста) VALUES (@statusId, @statusName);
    SET IDENTITY_INSERT СтатусыТестов OFF;
END";
            using (SqlCommand command = new SqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@statusId", statusId);
                command.Parameters.AddWithValue("@statusName", statusName);
                command.ExecuteNonQuery();
            }
        }
    }
}
