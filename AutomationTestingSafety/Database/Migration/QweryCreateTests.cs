using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTestingSafety.Database.Migration
{
    public static class QweryCreateTests
    {
        public static string GetQweryCreateTest()
        {
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
            return insertDefaultTests;
        }
    }
}