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

                // Создание таблицы Должности
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

                // Создание таблицы Пользователи
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
                InsertUserIfNotExists(connection, "1", "1", "Иван Иванов", "Сотрудник");
                InsertUserIfNotExists(connection, "2", "2", "Пётр Петров", "Специалист");
                InsertUserIfNotExists(connection, "3", "3", "Алексей Алексеев", "Администратор");

                // Создание таблицы Тесты с добавлением поля МинимальныйБалл
                string createTestsTable = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Тесты')
            BEGIN
                CREATE TABLE Тесты
                (
                    ID_Теста INT PRIMARY KEY IDENTITY(1,1),
                    НазваниеТеста NVARCHAR(200) NOT NULL,
                    Активен BIT NOT NULL DEFAULT 0,
                    Описание NVARCHAR(500) NULL,
                    МинимальныйБалл INT NOT NULL DEFAULT 0
                )
            END";
                using (var command = new SqlCommand(createTestsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Создание таблицы Вопросы с полем ID_Теста
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
                using (var command = new SqlCommand(createQuestionsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Создание таблицы ВариантыОтветов с добавлением поля Баллы
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
                using (var command = new SqlCommand(createAnswersTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Вставка теста, вопросов и вариантов ответов по умолчанию
                string insertDefaultTests = @"
-- Вариант 1
IF NOT EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 1')
BEGIN
    INSERT INTO Тесты (НазваниеТеста, Описание, Активен, МинимальныйБалл)
    VALUES (N'Тест по охране труда. Вариант 1', N'Автоматически созданный тест по охране труда (Вариант 1)', 1, 1);
END;

IF EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 1')
BEGIN
    DECLARE @TestId1 INT;
    SELECT @TestId1 = ID_Теста FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 1';

    -- Вопрос 1
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'КЕМ УТВЕРЖДАЮТСЯ ПРАВИЛА ВНУТРЕННЕГО ТРУДОВОГО РАСПОРЯДКА ОРГАНИЗАЦИИ?' AND ID_Теста = @TestId1)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'КЕМ УТВЕРЖДАЮТСЯ ПРАВИЛА ВНУТРЕННЕГО ТРУДОВОГО РАСПОРЯДКА ОРГАНИЗАЦИИ?', @TestId1);
    END;
    DECLARE @Q1Id INT;
    SELECT @Q1Id = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'КЕМ УТВЕРЖДАЮТСЯ ПРАВИЛА ВНУТРЕННЕГО ТРУДОВОГО РАСПОРЯДКА ОРГАНИЗАЦИИ?' AND ID_Теста = @TestId1;
    
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Работодателем.' AND ID_Вопроса = @Q1Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Работодателем.', 0, 0, @Q1Id);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Работодателем с учетом мнения представительного органа работников организации.' AND ID_Вопроса = @Q1Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Работодателем с учетом мнения представительного органа работников организации.', 1, 1, @Q1Id);
    END;

    -- Вопрос 2
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'ПРИ КАКОЙ ЧИСЛЕННОСТИ ОРГАНИЗАЦИИ ВВОДИТСЯ ДОЛЖНОСТЬ СПЕЦИАЛИСТА ПО ОХРАНЕ ТРУДА?' AND ID_Теста = @TestId1)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'ПРИ КАКОЙ ЧИСЛЕННОСТИ ОРГАНИЗАЦИИ ВВОДИТСЯ ДОЛЖНОСТЬ СПЕЦИАЛИСТА ПО ОХРАНЕ ТРУДА?', @TestId1);
    END;
    DECLARE @Q2Id INT;
    SELECT @Q2Id = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'ПРИ КАКОЙ ЧИСЛЕННОСТИ ОРГАНИЗАЦИИ ВВОДИТСЯ ДОЛЖНОСТЬ СПЕЦИАЛИСТА ПО ОХРАНЕ ТРУДА?' AND ID_Теста = @TestId1;
    
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'При численности более 10 человек.' AND ID_Вопроса = @Q2Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'При численности более 10 человек.', 0, 0, @Q2Id);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'При численности более 50 человек.' AND ID_Вопроса = @Q2Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'При численности более 50 человек.', 1, 1, @Q2Id);
    END;

    -- Вопрос 3
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'В КАКОМ СЛУЧАЕ РАБОТНИК, ЗАНЯТЫЙ НА РАБОТАХ С ВРЕДНЫМИ УСЛОВИЯМИ ТРУДА, ДОЛЖЕН ПРОХОДИТЬ ПЕРИОДИЧЕСКИЕ МЕДИЦИНСКИЕ ОСМОТРЫ?' AND ID_Теста = @TestId1)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'В КАКОМ СЛУЧАЕ РАБОТНИК, ЗАНЯТЫЙ НА РАБОТАХ С ВРЕДНЫМИ УСЛОВИЯМИ ТРУДА, ДОЛЖЕН ПРОХОДИТЬ ПЕРИОДИЧЕСКИЕ МЕДИЦИНСКИЕ ОСМОТРЫ?', @TestId1);
    END;
    DECLARE @Q3Id INT;
    SELECT @Q3Id = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'В КАКОМ СЛУЧАЕ РАБОТНИК, ЗАНЯТЫЙ НА РАБОТАХ С ВРЕДНЫМИ УСЛОВИЯМИ ТРУДА, ДОЛЖЕН ПРОХОДИТЬ ПЕРИОДИЧЕСКИЕ МЕДИЦИНСКИЕ ОСМОТРЫ?' AND ID_Теста = @TestId1;
    
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'В любом случае.' AND ID_Вопроса = @Q3Id)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'В любом случае.', 1, 1, @Q3Id);
    END;
END;

-- Вариант 2
IF NOT EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 2')
BEGIN
    INSERT INTO Тесты (НазваниеТеста, Описание, Активен, МинимальныйБалл)
    VALUES (N'Тест по охране труда. Вариант 2', N'Автоматически созданный тест по охране труда (Вариант 2)', 1, 2);
END;

IF EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 2')
BEGIN
    DECLARE @TestId2 INT;
    SELECT @TestId2 = ID_Теста FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 2';

    -- Вопрос 1
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'ИМЕЕТ ЛИ ПРАВО СПЕЦИАЛИСТ ПО ОХРАНЕ ТРУДА ОРГАНИЗАЦИИ ПРЕДЪЯВЛЯТЬ РУКОВОДИТЕЛЯМ ПОДРАЗДЕЛЕНИЙ ПРЕДПИСАНИЯ ОБ УСТРАНЕНИИ НАРУШЕНИЙ ТРЕБОВАНИЙ ОХРАНЫ ТРУДА?' AND ID_Теста = @TestId2)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'ИМЕЕТ ЛИ ПРАВО СПЕЦИАЛИСТ ПО ОХРАНЕ ТРУДА ОРГАНИЗАЦИИ ПРЕДЪЯВЛЯТЬ РУКОВОДИТЕЛЯМ ПОДРАЗДЕЛЕНИЙ ПРЕДПИСАНИЯ ОБ УСТРАНЕНИИ НАРУШЕНИЙ ТРЕБОВАНИЙ ОХРАНЫ ТРУДА?', @TestId2);
    END;
    DECLARE @Q21 INT;
    SELECT @Q21 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'ИМЕЕТ ЛИ ПРАВО СПЕЦИАЛИСТ ПО ОХРАНЕ ТРУДА ОРГАНИЗАЦИИ ПРЕДЪЯВЛЯТЬ РУКОВОДИТЕЛЯМ ПОДРАЗДЕЛЕНИЙ ПРЕДПИСАНИЯ ОБ УСТРАНЕНИИ НАРУШЕНИЙ ТРЕБОВАНИЙ ОХРАНЫ ТРУДА?' AND ID_Теста = @TestId2;
    
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
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'КАКИЕ ИНСТРУКЦИИ ПО ОХРАНЕ ТРУДА ДОЛЖНЫ БЫТЬ РАЗРАБОТАНЫ ДЛЯ РАБОТНИКА?' AND ID_Теста = @TestId2)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'КАКИЕ ИНСТРУКЦИИ ПО ОХРАНЕ ТРУДА ДОЛЖНЫ БЫТЬ РАЗРАБОТАНЫ ДЛЯ РАБОТНИКА?', @TestId2);
    END;
    DECLARE @Q22 INT;
    SELECT @Q22 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'КАКИЕ ИНСТРУКЦИИ ПО ОХРАНЕ ТРУДА ДОЛЖНЫ БЫТЬ РАЗРАБОТАНЫ ДЛЯ РАБОТНИКА?' AND ID_Теста = @TestId2;
    
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
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'ЯВЛЯЕТСЯ ЛИ ОБЯЗАТЕЛЬНЫМ ОБУЧЕНИЕ И ПРОВЕРКА ЗНАНИЙ ПО ОХРАНЕ ТРУДА ДЛЯ РУКОВОДИТЕЛЯ ПОДРАЗДЕЛЕНИЯ?' AND ID_Теста = @TestId2)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'ЯВЛЯЕТСЯ ЛИ ОБЯЗАТЕЛЬНЫМ ОБУЧЕНИЕ И ПРОВЕРКА ЗНАНИЙ ПО ОХРАНЕ ТРУДА ДЛЯ РУКОВОДИТЕЛЯ ПОДРАЗДЕЛЕНИЯ?', @TestId2);
    END;
    DECLARE @Q23 INT;
    SELECT @Q23 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'ЯВЛЯЕТСЯ ЛИ ОБЯЗАТЕЛЬНЫМ ОБУЧЕНИЕ И ПРОВЕРКА ЗНАНИЙ ПО ОХРАНЕ ТРУДА ДЛЯ РУКОВОДИТЕЛЯ ПОДРАЗДЕЛЕНИЯ?' AND ID_Теста = @TestId2;
    
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Да.' AND ID_Вопроса = @Q23)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Да.', 1, 1, @Q23);
    END;
END;

-- Вариант 3
IF NOT EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 3')
BEGIN
    INSERT INTO Тесты (НазваниеТеста, Описание, Активен, МинимальныйБалл)
    VALUES (N'Тест по охране труда. Вариант 3', N'Автоматически созданный тест по охране труда (Вариант 3)', 1, 2);
END;

IF EXISTS (SELECT * FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 3')
BEGIN
    DECLARE @TestId3 INT;
    SELECT @TestId3 = ID_Теста FROM Тесты WHERE НазваниеТеста = N'Тест по охране труда. Вариант 3';

    -- Вопрос 1
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'С КАКОЙ ПЕРИОДИЧНОСТЬЮ ДОЛЖНЫ ПРОХОДИТЬ ОБУЧЕНИЕ ПО ОХРАНЕ ТРУДА РУКОВОДИТЕЛИ И СПЕЦИАЛИСТЫ ОРГАНИЗАЦИИ?' AND ID_Теста = @TestId3)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'С КАКОЙ ПЕРИОДИЧНОСТЬЮ ДОЛЖНЫ ПРОХОДИТЬ ОБУЧЕНИЕ ПО ОХРАНЕ ТРУДА РУКОВОДИТЕЛИ И СПЕЦИАЛИСТЫ ОРГАНИЗАЦИИ?', @TestId3);
    END;
    DECLARE @Q31 INT;
    SELECT @Q31 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'С КАКОЙ ПЕРИОДИЧНОСТЬЮ ДОЛЖНЫ ПРОХОДИТЬ ОБУЧЕНИЕ ПО ОХРАНЕ ТРУДА РУКОВОДИТЕЛИ И СПЕЦИАЛИСТЫ ОРГАНИЗАЦИИ?' AND ID_Теста = @TestId3;
    
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
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'НУЖНО ЛИ СОГЛАСОВЫВАТЬ ИНСТРУКЦИИ ПО ОХРАНЕ ТРУДА С ПРОФСОЮЗНЫМ КОМИТЕТОМ?' AND ID_Теста = @TestId3)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'НУЖНО ЛИ СОГЛАСОВЫВАТЬ ИНСТРУКЦИИ ПО ОХРАНЕ ТРУДА С ПРОФСОЮЗНЫМ КОМИТЕТОМ?', @TestId3);
    END;
    DECLARE @Q32 INT;
    SELECT @Q32 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'НУЖНО ЛИ СОГЛАСОВЫВАТЬ ИНСТРУКЦИИ ПО ОХРАНЕ ТРУДА С ПРОФСОЮЗНЫМ КОМИТЕТОМ?' AND ID_Теста = @TestId3;
    
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Нужно.' AND ID_Вопроса = @Q32)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Нужно.', 1, 1, @Q32);
    END;

    -- Вопрос 3
    IF NOT EXISTS (SELECT * FROM Вопросы WHERE ТекстВопроса = N'ЧТО ОЗНАЧАЕТ ТЕРМИН ''БЕЗОПАСНЫЕ УСЛОВИЯ ТРУДА''?' AND ID_Теста = @TestId3)
    BEGIN
         INSERT INTO Вопросы (ТекстВопроса, ID_Теста)
         VALUES (N'ЧТО ОЗНАЧАЕТ ТЕРМИН ''БЕЗОПАСНЫЕ УСЛОВИЯ ТРУДА''?', @TestId3);
    END;
    DECLARE @Q33 INT;
    SELECT @Q33 = ID_Вопроса FROM Вопросы WHERE ТекстВопроса = N'ЧТО ОЗНАЧАЕТ ТЕРМИН ''БЕЗОПАСНЫЕ УСЛОВИЯ ТРУДА''?' AND ID_Теста = @TestId3;
    
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Условия труда, при которых воздействие на работающих вредных факторов не превышает установленных нормативов.' AND ID_Вопроса = @Q33)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Условия труда, при которых воздействие на работающих вредных факторов не превышает установленных нормативов.', 1, 1, @Q33);
    END;
    IF NOT EXISTS (SELECT * FROM ВариантыОтветов WHERE ТекстВарианта = N'Условия труда, при которых работник не должен пользоваться спецодеждой.' AND ID_Вопроса = @Q33)
    BEGIN
         INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса)
         VALUES (N'Условия труда, при которых работник не должен пользоваться спецодеждой.', 0, 0, @Q33);
    END;
END;
";
                using (var command = new SqlCommand(insertDefaultTests, connection))
                {
                    command.ExecuteNonQuery();
                }

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
