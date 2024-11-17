using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CommentManager.Repository
{
    public class CommentRepository
    {
        private readonly string _databasePath;

        public CommentRepository(string databasePath)
        {
            _databasePath = databasePath;

            // Создаем базу данных при необходимости
            if (!File.Exists(_databasePath))
            {
                SQLiteConnection.CreateFile(_databasePath);
                CreateDatabase();
            }
        }

        // Метод для получения комментариев из базы данных по имени репозитория
        public List<(string Comment, string Username, string Date)> GetComments(string repositoryName)
        {
            var comments = new List<(string, string, string)>();

            using var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;");
            connection.Open();

            using var command = new SQLiteCommand(connection)
            {
                CommandText = "SELECT Comment, Username, Date FROM Comments WHERE RepositoryName = @repositoryName ORDER BY Date DESC"
            };
            command.Parameters.AddWithValue("@repositoryName", repositoryName);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string comment = reader["Comment"].ToString();
                string username = reader["Username"].ToString();
                string date = reader["Date"].ToString();
                comments.Add((comment, username, date));
            }

            return comments;
        }

        // Метод для добавления комментария в базу данных
        public void InsertComment(string repositoryName, string comment, string username)
        {
            using var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;");
            connection.Open();

            using var command = new SQLiteCommand(connection)
            {
                CommandText = "INSERT INTO Comments (RepositoryName, Comment, Username, Date) VALUES (@repositoryName, @comment, @username, @date)"
            };
            command.Parameters.AddWithValue("@repositoryName", repositoryName);
            command.Parameters.AddWithValue("@comment", comment);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }

        // Создание таблицы, если она не существует
        private void CreateDatabase()
        {
            using var connection = new SQLiteConnection($"Data Source={_databasePath};Version=3;");
            connection.Open();

            using var command = new SQLiteCommand(connection)
            {
                CommandText = @"
                    CREATE TABLE Comments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        RepositoryName TEXT NOT NULL,
                        Comment TEXT NOT NULL,
                        Username TEXT NOT NULL,
                        Date TEXT NOT NULL
                    );"
            };
            command.ExecuteNonQuery();
        }
    }
}
