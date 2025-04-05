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
                using (SqlCommand cmd = new SqlCommand("SELECT ID_Теста, НазваниеТеста, Описание, ID_СтатусаТеста, МинимальныйБалл FROM Тесты", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tests.Add(new TestEntity
                        {
                            Id = Convert.ToInt32(reader["ID_Теста"]),
                            Name = reader["НазваниеТеста"].ToString(),
                            Description = reader["Описание"].ToString(),
                            StatusId = Convert.ToInt32(reader["ID_СтатусаТеста"]),
                            MinimalScore = Convert.ToInt32(reader["МинимальныйБалл"])
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
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Тесты (НазваниеТеста, Описание, ID_СтатусаТеста) OUTPUT INSERTED.ID_Теста VALUES (@name, @desc, @statusId)",
                    connection))
                {
                    cmd.Parameters.AddWithValue("@name", test.Name);
                    cmd.Parameters.AddWithValue("@desc", test.Description);
                    cmd.Parameters.AddWithValue("@statusId", test.StatusId);
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

                using (SqlCommand cmdTest = new SqlCommand("SELECT ID_Теста, НазваниеТеста, Описание, ID_СтатусаТеста, МинимальныйБалл FROM Тесты WHERE ID_Теста = @id", connection))
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
                                StatusId = Convert.ToInt32(reader["ID_СтатусаТеста"]),
                                MinimalScore = Convert.ToInt32(reader["МинимальныйБалл"])
                            };
                        }
                    }
                }
                if (test == null)
                    return null;

                // Загрузка вопросов и вариантов ответов остается без изменений...
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
                                    ID = Convert.ToInt32(reader["ID_Варианта"]),
                                    Text = reader["ТекстВарианта"].ToString(),
                                    IsCorrect = Convert.ToBoolean(reader["Правильный"]),
                                    Points = Convert.ToInt32(reader["Баллы"])
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
                using (SqlCommand cmdTest = new SqlCommand(
                    "UPDATE Тесты SET НазваниеТеста = @name, Описание = @desc, ID_СтатусаТеста = @statusId, МинимальныйБалл = @minScore WHERE ID_Теста = @id",
                    connection))
                {
                    cmdTest.Parameters.AddWithValue("@name", test.Name);
                    cmdTest.Parameters.AddWithValue("@desc", test.Description);
                    cmdTest.Parameters.AddWithValue("@statusId", test.StatusId);
                    cmdTest.Parameters.AddWithValue("@minScore", test.MinimalScore);
                    cmdTest.Parameters.AddWithValue("@id", test.Id);
                    cmdTest.ExecuteNonQuery();
                }

                // Обновление вопросов и вариантов ответов остаётся без изменений
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

                    foreach (var answer in question.Answers)
                    {
                        if (answer.ID > 0)
                        {
                            using (SqlCommand cmdAnswer = new SqlCommand("UPDATE ВариантыОтветов SET ТекстВарианта = @text, Правильный = @correct, Баллы = @points WHERE ID_Варианта = @id", connection))
                            {
                                cmdAnswer.Parameters.AddWithValue("@text", answer.Text);
                                cmdAnswer.Parameters.AddWithValue("@correct", answer.IsCorrect);
                                cmdAnswer.Parameters.AddWithValue("@points", answer.Points);
                                cmdAnswer.Parameters.AddWithValue("@id", answer.ID);
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


        public static void DeleteTest(int testId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();

                // Сначала удаляем варианты ответов для вопросов теста
                string deleteAnswers = "DELETE FROM ВариантыОтветов WHERE ID_Вопроса IN (SELECT ID_Вопроса FROM Вопросы WHERE ID_Теста = @testId)";
                using (SqlCommand cmd = new SqlCommand(deleteAnswers, connection))
                {
                    cmd.Parameters.AddWithValue("@testId", testId);
                    cmd.ExecuteNonQuery();
                }

                // Затем удаляем вопросы теста
                string deleteQuestions = "DELETE FROM Вопросы WHERE ID_Теста = @testId";
                using (SqlCommand cmd = new SqlCommand(deleteQuestions, connection))
                {
                    cmd.Parameters.AddWithValue("@testId", testId);
                    cmd.ExecuteNonQuery();
                }

                // Наконец, удаляем сам тест
                string deleteTest = "DELETE FROM Тесты WHERE ID_Теста = @testId";
                using (SqlCommand cmd = new SqlCommand(deleteTest, connection))
                {
                    cmd.Parameters.AddWithValue("@testId", testId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteQuestion(int questionId)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();

                // Сначала удаляем варианты ответов для данного вопроса
                string deleteAnswers = "DELETE FROM ВариантыОтветов WHERE ID_Вопроса = @questionId";
                using (SqlCommand cmd = new SqlCommand(deleteAnswers, connection))
                {
                    cmd.Parameters.AddWithValue("@questionId", questionId);
                    cmd.ExecuteNonQuery();
                }

                // Затем удаляем сам вопрос
                string deleteQuestion = "DELETE FROM Вопросы WHERE ID_Вопроса = @questionId";
                using (SqlCommand cmd = new SqlCommand(deleteQuestion, connection))
                {
                    cmd.Parameters.AddWithValue("@questionId", questionId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static List<TestStatus> GetTestStatuses()
        {
            var statuses = new List<TestStatus>();
            using (SqlConnection connection = new SqlConnection(ConnectionString._connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT ID_СтатусаТеста, СтатусТеста FROM СтатусыТестов", connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            statuses.Add(new TestStatus
                            {
                                ID = Convert.ToInt32(reader["ID_СтатусаТеста"]),
                                Name = reader["СтатусТеста"].ToString()
                            });
                        }
                    }
                }
            }
            return statuses;
        }

    }
}
