using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace PathOfFilters
{
    internal class SqlWrapper
    {
        private readonly string _constring;
        private readonly string _dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PathOfFilters\Filters.s3db";

        internal SqlWrapper()
        {
            var connection = new SQLiteConnectionStringBuilder
            {
                ConnectionString = String.Format("Data source={0};Version=3;", _dbPath)
            };
            _constring = connection.ConnectionString;
            Setup();
        }

        internal void Setup()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PathOfFilters"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PathOfFilters");
                if (!File.Exists(_dbPath)) SQLiteConnection.CreateFile(_dbPath);
            }
            using (var connection = new SQLiteConnection(_constring).OpenAndReturn())
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    const string createFilters = @"CREATE TABLE IF NOT EXISTS `filters` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `name` TEXT UNIQUE, 
                                `tag` TEXT, `filter` TEXT);";
                    cmd.CommandText = createFilters;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal long CreateFilter()
        {
            try
            {
                using (var connection = new SQLiteConnection(_constring).OpenAndReturn())
                {
                    const string insertFilter = @"INSERT INTO `filters` (name, tag, filter) VALUES ('New Filter', '', '');";
                    using (var cmd = new SQLiteCommand(insertFilter, connection))
                    {
                        cmd.ExecuteNonQuery();
                        return connection.LastInsertRowId;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                if (ex.ErrorCode == 19)MessageBox.Show(@"Failed to create new filter, verify that a filter doesn't already exist with the name 'New Filter'");
                return 0;
            }
        }

        internal bool UpdateFilter(long id, string name, string tag, string filter)
        {
            using (var connection = new SQLiteConnection(_constring).OpenAndReturn())
            {
                const string updateFilter = @"UPDATE `filters` SET name=@name, tag=@tag, filter=@filter WHERE id=@id";
                using (var cmd = new SQLiteCommand(updateFilter, connection))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@tag", tag);
                    cmd.Parameters.AddWithValue("@filter", filter);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        internal List<Filter> GetFilters()
        {
            var filterList = new List<Filter>();
            using (var connection = new SQLiteConnection(_constring).OpenAndReturn())
            {
                const string selectFilters = @"SELECT * FROM `filters`;";
                using (var cmd = new SQLiteCommand(selectFilters, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id;
                            var newFilter = new Filter
                            {
                                Id = int.TryParse(reader["id"].ToString(), out id) ? id : 0,
                                Name = reader["name"].ToString(),
                                Tag = reader["tag"].ToString(),
                                FilterValue = reader["filter"].ToString()
                            };
                            filterList.Add(newFilter);
                        }
                    }
                }
            }
            return filterList;
        }
    }
}
