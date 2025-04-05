using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using AutomationTestingSafety.Database;
using AutomationTestingSafety.Entities;

namespace AutomationTestingSafety
{
    public static class TestRepository
    {
        public static List<TestEntity> GetAllTests()
        {
            var tests = new List<TestEntity>();
            using (SqlConnection connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID_Теста, НазваниеТеста, Описание, Активен FROM Тесты", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tests.Add(new TestEntity
                        {
                            Id = Convert.ToInt32(reader["ID_Теста"]),
                            Name = reader["НазваниеТеста"].ToString(),
                            Description = reader["Описание"].ToString(),
                            Active = Convert.ToBoolean(reader["Активен"])
                        });
                    }
                }
            }
            return tests;
        }

        public static int CreateTest(TestEntity test)
        {
            int newTestId = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Тесты (НазваниеТеста, Описание, Активен) OUTPUT INSERTED.ID_Теста VALUES (@name, @desc, @active)", connection))
                {
                    cmd.Parameters.AddWithValue("@name", test.Name);
                    cmd.Parameters.AddWithValue("@desc", test.Description);
                    cmd.Parameters.AddWithValue("@active", test.Active);
                    newTestId = (int)cmd.ExecuteScalar();
                }
            }
            return newTestId;
        }

        public static TestEntity GetTestById(int testId)
        {
            TestEntity test = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();

                // Получаем информацию о тесте
                using (SqlCommand cmdTest = new SqlCommand("SELECT ID_Теста, НазваниеТеста, Описание, Активен FROM Тесты WHERE ID_Теста = @id", connection))
                {
                    cmdTest.Parameters.AddWithValue("@id", testId);
                    using (SqlDataReader reader = cmdTest.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            test = new TestEntity
                            {
                                Id = Convert.ToInt32(reader["ID_Теста"]),
                                Name = reader["НазваниеТеста"].ToString(),
                                Description = reader["Описание"].ToString(),
                                Active = Convert.ToBoolean(reader["Активен"])
                            };
                        }
                    }
                }
                if (test == null)
                    return null;

                // Получаем вопросы теста
                using (SqlCommand cmdQuestions = new SqlCommand("SELECT ID_Вопроса, ТекстВопроса FROM Вопросы WHERE ID_Теста = @testId", connection))
                {
                    cmdQuestions.Parameters.AddWithValue("@testId", testId);
                    using (SqlDataReader reader = cmdQuestions.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var question = new QuestionEntity
                            {
                                Id = Convert.ToInt32(reader["ID_Вопроса"]),
                                Text = reader["ТекстВопроса"].ToString()
                            };
                            test.Questions.Add(question);
                        }
                    }
                }

                // Получаем варианты ответов для каждого вопроса
                foreach (var question in test.Questions)
                {
                    using (SqlCommand cmdAnswers = new SqlCommand("SELECT ID_Варианта, ТекстВарианта, Правильный, Баллы FROM ВариантыОтветов WHERE ID_Вопроса = @questionId", connection))
                    {
                        cmdAnswers.Parameters.AddWithValue("@questionId", question.Id);
                        using (SqlDataReader reader = cmdAnswers.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var answer = new AnswerEntity
                                {
                                    Id = Convert.ToInt32(reader["ID_Варианта"]),
                                    Text = reader["ТекстВарианта"].ToString(),
                                    IsCorrect = Convert.ToBoolean(reader["Правильный"]),
                                    Points = Convert.ToInt32(reader["Баллы"])  // Добавлено для загрузки баллов
                                };
                                question.Answers.Add(answer);
                            }
                        }
                    }
                }

            }
            return test;
        }

        public static void UpdateTest(TestEntity test)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();
                // Обновляем данные теста, включая минимальный балл
                using (SqlCommand cmdTest = new SqlCommand("UPDATE Тесты SET НазваниеТеста = @name, Описание = @desc, Активен = @active, МинимальныйБалл = @minScore WHERE ID_Теста = @id", connection))
                {
                    cmdTest.Parameters.AddWithValue("@name", test.Name);
                    cmdTest.Parameters.AddWithValue("@desc", test.Description);
                    cmdTest.Parameters.AddWithValue("@active", test.Active);
                    cmdTest.Parameters.AddWithValue("@minScore", test.MinimalScore);
                    cmdTest.Parameters.AddWithValue("@id", test.Id);
                    cmdTest.ExecuteNonQuery();
                }

                // Обновляем вопросы и ответы
                foreach (var question in test.Questions)
                {
                    if (question.Id > 0)
                    {
                        using (SqlCommand cmdQuestion = new SqlCommand("UPDATE Вопросы SET ТекстВопроса = @text WHERE ID_Вопроса = @id", connection))
                        {
                            cmdQuestion.Parameters.AddWithValue("@text", question.Text);
                            cmdQuestion.Parameters.AddWithValue("@id", question.Id);
                            cmdQuestion.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (SqlCommand cmdQuestion = new SqlCommand("INSERT INTO Вопросы (ТекстВопроса, ID_Теста) OUTPUT INSERTED.ID_Вопроса VALUES (@text, @testId)", connection))
                        {
                            cmdQuestion.Parameters.AddWithValue("@text", question.Text);
                            cmdQuestion.Parameters.AddWithValue("@testId", test.Id);
                            question.Id = (int)cmdQuestion.ExecuteScalar();
                        }
                    }

                    // Обновляем варианты ответов
                    foreach (var answer in question.Answers)
                    {
                        if (answer.Id > 0)
                        {
                            using (SqlCommand cmdAnswer = new SqlCommand("UPDATE ВариантыОтветов SET ТекстВарианта = @text, Правильный = @correct, Баллы = @points WHERE ID_Варианта = @id", connection))
                            {
                                cmdAnswer.Parameters.AddWithValue("@text", answer.Text);
                                cmdAnswer.Parameters.AddWithValue("@correct", answer.IsCorrect);
                                cmdAnswer.Parameters.AddWithValue("@points", answer.Points);
                                cmdAnswer.Parameters.AddWithValue("@id", answer.Id);
                                cmdAnswer.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            using (SqlCommand cmdAnswer = new SqlCommand("INSERT INTO ВариантыОтветов (ТекстВарианта, Правильный, Баллы, ID_Вопроса) VALUES (@text, @correct, @points, @questionId)", connection))
                            {
                                cmdAnswer.Parameters.AddWithValue("@text", answer.Text);
                                cmdAnswer.Parameters.AddWithValue("@correct", answer.IsCorrect);
                                cmdAnswer.Parameters.AddWithValue("@points", answer.Points);
                                cmdAnswer.Parameters.AddWithValue("@questionId", question.Id);
                                cmdAnswer.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

    }
}
