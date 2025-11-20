using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace StudentPlanner.Data
{
    public class SQLiteService
    {
        private readonly string _connectionString;

        public SQLiteService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SQLiteConnection")
                ?? "Data Source=Data/StudyResources.db";

            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            var builder = new SqliteConnectionStringBuilder(_connectionString);
            var dataSource = builder.DataSource;

            var folder = Path.GetDirectoryName(dataSource);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText =
                @"CREATE TABLE IF NOT EXISTS StudyTips (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TipText TEXT NOT NULL
                  );";
            createTableCmd.ExecuteNonQuery();

            var countCmd = connection.CreateCommand();
            countCmd.CommandText = "SELECT COUNT(*) FROM StudyTips;";
            long count = (long)(countCmd.ExecuteScalar() ?? 0);

            if (count == 0)
            {
                var insertCmd = connection.CreateCommand();
                insertCmd.CommandText =
                    @"INSERT INTO StudyTips (TipText) VALUES
                      ('Break your study sessions into focused 25-minute blocks.'),
                      ('Review your notes within 24 hours to improve retention.'),
                      ('Alternate between different courses to avoid burnout.');";
                insertCmd.ExecuteNonQuery();
            }
        }

        public List<string> GetStudyTips()
        {
            var tips = new List<string>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT TipText FROM StudyTips;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tips.Add(reader.GetString(0));
            }

            return tips;
        }
    }
}
