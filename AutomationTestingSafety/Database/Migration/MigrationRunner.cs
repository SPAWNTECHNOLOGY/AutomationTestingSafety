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
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Таблица Должности
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

                        // Таблица Пользователи
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
        ID_Должности INT NOT NULL,
        CONSTRAINT FK_Пользователи_Должности FOREIGN KEY (ID_Должности)
            REFERENCES Должности(ID_Должности)
    )
END";
                        using (SqlCommand command = new SqlCommand(createUsersTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Добавляем должности
                        InsertPositionIfNotExists(connection, transaction, "Сотрудник");
                        InsertPositionIfNotExists(connection, transaction, "Специалист");
                        InsertPositionIfNotExists(connection, transaction, "Администратор");

                        // Добавляем пользователей
                        InsertUserIfNotExists(connection, transaction, "1", "1", "Иван Иванов", "05.04.1999", "Сотрудник");
                        InsertUserIfNotExists(connection, transaction, "2", "2", "Пётр Петров", "02.24.2004", "Специалист");
                        InsertUserIfNotExists(connection, transaction, "3", "3", "Алексей Алексеев", "11.10.2000", "Администратор");

                        // Таблица СтатусыТестов
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

                        // Таблица Тесты с полем МинимальныйБалл, ID_СтатусаТеста и ID_Пользователя
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

                        // Если существует старое поле "Активен", удаляем его
                        string dropActiveColumn = @"
IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'Активен' AND Object_ID = OBJECT_ID(N'Тесты'))
BEGIN
    ALTER TABLE Тесты DROP COLUMN Активен
END";
                        using (SqlCommand command = new SqlCommand(dropActiveColumn, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Добавляем внешний ключ для поля ID_СтатусаТеста
                        string addFkStatus = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Тесты_СтатусыТестов')
BEGIN
    ALTER TABLE Тесты
    ADD CONSTRAINT FK_Тесты_СтатусыТестов FOREIGN KEY (ID_СтатусаТеста) REFERENCES СтатусыТестов(ID_СтатусаТеста)
END";
                        using (SqlCommand command = new SqlCommand(addFkStatus, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Добавляем внешний ключ для поля ID_Пользователя
                        string addFkEmployee = @"
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'FK_Тесты_Пользователи')
BEGIN
    ALTER TABLE Тесты
    ADD CONSTRAINT FK_Тесты_Пользователи FOREIGN KEY (ID_Пользователя) REFERENCES Пользователи(ID_Пользователя)
END";
                        using (SqlCommand command = new SqlCommand(addFkEmployee, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Таблица Вопросы
                        string createQuestionsTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Вопросы')
BEGIN
    CREATE TABLE Вопросы
    (
        ID_Вопроса INT PRIMARY KEY IDENTITY(1,1),
        ТекстВопроса NVARCHAR(500) NOT NULL,
        ID_Теста INT NOT NULL,
        CONSTRAINT FK_Вопросы_Тесты FOREIGN KEY (ID_Теста)
            REFERENCES Тесты(ID_Теста)
    )
END";
                        using (SqlCommand command = new SqlCommand(createQuestionsTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Таблица ВариантыОтветов
                        string createAnswersTable = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ВариантыОтветов')
BEGIN
    CREATE TABLE ВариантыОтветов
    (
        ID_Варианта INT PRIMARY KEY IDENTITY(1,1),
        ТекстВарианта NVARCHAR(500) NOT NULL,
        Правильный BIT NOT NULL DEFAULT 0,
        Баллы INT NOT NULL DEFAULT 0,
        ID_Вопроса INT NOT NULL,
        CONSTRAINT FK_ВариантыОтветов_Вопросы FOREIGN KEY (ID_Вопроса)
            REFERENCES Вопросы(ID_Вопроса)
    )
END";
                        using (SqlCommand command = new SqlCommand(createAnswersTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Таблица РезультатыТестов
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
        Детали NVARCHAR(MAX) NULL,
        CONSTRAINT FK_РезультатыТестов_Пользователи FOREIGN KEY (ID_Пользователя)
            REFERENCES Пользователи(ID_Пользователя),
        CONSTRAINT FK_РезультатыТестов_Тесты FOREIGN KEY (ID_Теста)
            REFERENCES Тесты(ID_Теста)
    )
END";
                        using (SqlCommand command = new SqlCommand(createResultsTable, connection, transaction))
                        {
                            command.ExecuteNonQuery();
                        }

                        // Вставка тестов, вопросов и вариантов ответов по умолчанию (три варианта)
                        string insertDefaultTests = @"
-- Вариант 1
IF NOT EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 1')
BEGIN
    INSERT INTO Тесты (НазваниеТеста, Описание, МинимальныйБалл, ID_СтатусаТеста)
    VALUES (N'Тест по охране труда. Вариант 1', N'Автоматически созданный тест (Вариант 1)', 1, 2);
END;
IF EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 1')
BEGIN
    DECLARE @TestId1 INT;
    SELECT @TestId1 = ID_Теста FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 1';
    -- Вопрос 1
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'КЕМ УТВЕРЖДАЮТСЯ ПРАВИЛА ВНУТРЕННЕГО ТРУДОВОГО РАСПОРЯДКА?' AND ID_Теста = @TestId1)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'КЕМ УТВЕРЖДАЮТСЯ ПРАВИЛА ВНУТРЕННЕГО ТРУДОВОГО РАСПОРЯДКА?', @TestId1);
    END;
    DECLARE @Q1Id INT;
    SELECT @Q1Id = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'КЕМ УТВЕРЖДАЮТСЯ ПРАВИЛА ВНУТРЕННЕГО ТРУДОВОГО РАСПОРЯДКА?' AND ID_Теста = @TestId1;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Работодателем.' AND ID_Вопроса = @Q1Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Работодателем.', 0, 0, @Q1Id);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Работодателем с учетом мнения работников.' AND ID_Вопроса = @Q1Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Работодателем с учетом мнения работников.', 1, 1, @Q1Id);
    END;
    -- Вопрос 2
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'При какой численности вводится должность специалиста?' AND ID_Теста = @TestId1)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'При какой численности вводится должность специалиста?', @TestId1);
    END;
    DECLARE @Q2Id INT;
    SELECT @Q2Id = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'При какой численности вводится должность специалиста?' AND ID_Теста = @TestId1;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Более 10 человек.' AND ID_Вопроса = @Q2Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Более 10 человек.', 0, 0, @Q2Id);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Более 50 человек.' AND ID_Вопроса = @Q2Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Более 50 человек.', 1, 1, @Q2Id);
    END;
    -- Вопрос 3
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'Когда обязан проходить медицинский осмотр?' AND ID_Теста = @TestId1)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'Когда обязан проходить медицинский осмотр?', @TestId1);
    END;
    DECLARE @Q3Id INT;
    SELECT @Q3Id = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'Когда обязан проходить медицинский осмотр?' AND ID_Теста = @TestId1;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'В любом случае.' AND ID_Вопроса = @Q3Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'В любом случае.', 1, 1, @Q3Id);
    END;
END;

-- Вариант 2
IF NOT EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 2')
BEGIN
    INSERT INTO Тесты (НазваниеТеста, Описание, МинимальныйБалл, ID_СтатусаТеста)
    VALUES (N'Тест по охране труда. Вариант 2', N'Автоматически созданный тест (Вариант 2)', 2, 2);
END;
IF EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 2')
BEGIN
    DECLARE @TestId2 INT;
    SELECT @TestId2 = ID_Теста FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 2';
    -- Вопрос 1
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'Имеет ли право специалист предъявлять предписания?' AND ID_Теста = @TestId2)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'Имеет ли право специалист предъявлять предписания?', @TestId2);
    END;
    DECLARE @Q21 INT;
    SELECT @Q21 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'Имеет ли право специалист предъявлять предписания?' AND ID_Теста = @TestId2;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Имеет.' AND ID_Вопроса = @Q21)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Имеет.', 1, 1, @Q21);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Не имеет.' AND ID_Вопроса = @Q21)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Не имеет.', 0, 0, @Q21);
    END;
    -- Вопрос 2
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'Какие инструкции должны быть разработаны для работника?' AND ID_Теста = @TestId2)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'Какие инструкции должны быть разработаны для работника?', @TestId2);
    END;
    DECLARE @Q22 INT;
    SELECT @Q22 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'Какие инструкции должны быть разработаны для работника?' AND ID_Теста = @TestId2;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Исходя из должности или профессии работника.' AND ID_Вопроса = @Q22)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Исходя из должности или профессии работника.', 1, 1, @Q22);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Исходя из вида выполняемой работы.' AND ID_Вопроса = @Q22)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Исходя из вида выполняемой работы.', 0, 0, @Q22);
    END;
    -- Вопрос 3
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'Является ли обязательным обучение для руководителя подразделения?' AND ID_Теста = @TestId2)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'Является ли обязательным обучение для руководителя подразделения?', @TestId2);
    END;
    DECLARE @Q23 INT;
    SELECT @Q23 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'Является ли обязательным обучение для руководителя подразделения?' AND ID_Теста = @TestId2;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Да.' AND ID_Вопроса = @Q23)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Да.', 1, 1, @Q23);
    END;
END;

-- Вариант 3
IF NOT EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 3')
BEGIN
    INSERT INTO Тесты (НазваниеТеста, Описание, МинимальныйБалл, ID_СтатусаТеста)
    VALUES (N'Тест по охране труда. Вариант 3', N'Автоматически созданный тест (Вариант 3)', 2, 2);
END;
IF EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 3')
BEGIN
    DECLARE @TestId3 INT;
    SELECT @TestId3 = ID_Теста FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 3';
    -- Вопрос 1
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'С какой периодичностью проходят обучение?' AND ID_Теста = @TestId3)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'С какой периодичностью проходят обучение?', @TestId3);
    END;
    DECLARE @Q31 INT;
    SELECT @Q31 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'С какой периодичностью проходят обучение?' AND ID_Теста = @TestId3;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Не реже одного раза в год.' AND ID_Вопроса = @Q31)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Не реже одного раза в год.', 1, 1, @Q31);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Не реже одного раза в два года.' AND ID_Вопроса = @Q31)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Не реже одного раза в два года.', 0, 0, @Q31);
    END;
    -- Вопрос 2
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'Нужно ли согласовывать инструкции с профсоюзом?' AND ID_Теста = @TestId3)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'Нужно ли согласовывать инструкции с профсоюзом?', @TestId3);
    END;
    DECLARE @Q32 INT;
    SELECT @Q32 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'Нужно ли согласовывать инструкции с профсоюзом?' AND ID_Теста = @TestId3;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Нужно.' AND ID_Вопроса = @Q32)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Нужно.', 1, 1, @Q32);
    END;
    -- Вопрос 3
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'Что означает термин ""Безопасные условия труда""?' AND ID_Теста = @TestId3)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'Что означает термин ""Безопасные условия труда""?', @TestId3);
    END;
    DECLARE @Q33 INT;
    SELECT @Q33 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'Что означает термин ""Безопасные условия труда""?' AND ID_Теста = @TestId3;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Условия труда, при которых воздействие не превышает норм.' AND ID_Вопроса = @Q33)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Условия труда, при которых воздействие не превышает норм.', 1, 1, @Q33);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Условия труда, при которых работник не должен пользоваться спецодеждой.' AND ID_Вопроса = @Q33)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Условия труда, при которых работник не должен пользоваться спецодеждой.', 0, 0, @Q33);
    END;
END;
";
                        using (SqlCommand command = new SqlCommand(insertDefaultTests, connection, transaction))
                        {
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
    SELECT @positionID = ID_Должности FROM Должности WHERE НазваниеДолжности = @positionName;
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
