using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace PathOfFilters
{
    internal class SqlWrapper
    {
        private readonly string _constring;
        private readonly SQLiteConnection _connection;
        private readonly string _dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PathOfFilters\Filters.s3db";

        internal SqlWrapper()
        {
            var constring = new SQLiteConnectionStringBuilder
            {
                ConnectionString = String.Format("Data source={0};Version=3;", _dbPath)
            };
            _connection = new SQLiteConnection
            {
                ConnectionString = constring.ToString(),
                ParseViaFramework = true
            };
            Setup();
        }

        internal void Setup()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PathOfFilters"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\PathOfFilters");
                if (!File.Exists(_dbPath)) SQLiteConnection.CreateFile(_dbPath);
            }
            using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
            {
                using (var cmd = new SQLiteCommand(connection))
                {
                    const string createFilters = @"CREATE TABLE IF NOT EXISTS `filters` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `name` TEXT UNIQUE, 
                                `tag` TEXT, `filter` TEXT, `version` INTEGER, `pastebin` TEXT);";
                    cmd.CommandText = createFilters;
                    cmd.ExecuteNonQuery();

                    const string createFilterObject = @"CREATE TABLE IF NOT EXISTS `filter_objects` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `title` TEXT, 
                                        `description` text, `order` INTEGER, `show` TINYINT);";
                    cmd.CommandText = createFilterObject;
                    cmd.ExecuteNonQuery();

                    const string createObjects = @"CREATE TABLE IF NOT EXISTS `filter_conditions` (`id` INTEGER PRIMARY KEY AUTOINCREMENT, `filter_id` INTEGER, 
                                        `condition` TEXT, `value` TEXT);";
                    cmd.CommandText = createObjects;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ClearTable(string table)
        {
            using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
            {
                var dropTable = String.Format(@"DROP TABLE `{0}`", table);
                using (var cmd = new SQLiteCommand(dropTable, connection))
                {
                    
                }
            }
        }

        /// <summary> Creates a new default Filter and returns the object with it's ID </summary>
        /// <returns></returns>
        internal Filter CreateFilter()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
                {
                    const string insertFilter = @"INSERT INTO `filters` (name, tag, filter, version, pastebin) VALUES ('New Filter', '', '', 1, '');";
                    using (var cmd = new SQLiteCommand(insertFilter, connection))
                    {
                        cmd.ExecuteNonQuery();
                        var newFilter = new Filter
                        {
                            Id = (int)connection.LastInsertRowId,
                            Name = "New Filter",
                            FilterValue = String.Format("#Generated using PathOfFilters on {0} | Developed by Ministry v{1}", DateTime.Now.ToShortDateString(), Environment.Version),
                            Tag = "",
                        };
                        return newFilter;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                if (ex.ErrorCode == 19)MessageBox.Show(@"Failed to create new filter, verify that a filter doesn't already exist with the name 'New Filter'");
                return new Filter();
            }
        }

        internal bool UpdateFilter(Filter filter)
        {
            using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
            {
                const string updateFilter = @"UPDATE `filters` SET name=@name, tag=@tag, filter=@filter, version=@version, pastebin=@pastebin WHERE id=@id";
                using (var cmd = new SQLiteCommand(updateFilter, connection))
                {
                    cmd.Parameters.AddWithValue("@name", filter.Name);
                    cmd.Parameters.AddWithValue("@tag", filter.Tag);
                    cmd.Parameters.AddWithValue("@filter", filter.FilterValue);
                    cmd.Parameters.AddWithValue("@version", filter.Version);
                    cmd.Parameters.AddWithValue("@pastebin", filter.Pastebin);
                    cmd.Parameters.AddWithValue("@id", filter.Id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        internal List<Filter> GetFilters()
        {
            var filterList = new List<Filter>();
            using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
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

        internal int CreateObjects(List<FilterObject> filterObjects)
        {
            var objectId = 0;
            using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
            {
                const string insertObject = @"INSERT INTO `filter_objects` (title, description, order, show) VALUES 
                                (@title, @description, @order, @show);";
                var transaction = connection.BeginTransaction();
                using (var cmd = new SQLiteCommand(insertObject, connection))
                {
                    cmd.Parameters.AddWithValue("@title", "");
                    cmd.Parameters.AddWithValue("@description", "");
                    cmd.Parameters.AddWithValue("@order", "");
                    cmd.Parameters.AddWithValue("@show", "");
                    foreach (var filterObject in filterObjects)
                    {
                        cmd.Parameters["@title"].Value = filterObject.Title;
                        cmd.Parameters["@description"].Value = filterObject.Description;
                        cmd.Parameters["@order"].Value = filterObject.Order;
                        cmd.Parameters["@show"].Value = filterObject.Show;
                        cmd.ExecuteNonQuery();
                        var lastId = connection.LastInsertRowId;
                        if (int.TryParse(lastId.ToString(), out objectId))
                        {
                            filterObject.Id = objectId;
                            CreateObjectConditions(filterObject, connection);
                        }
                    }
                }
                transaction.Commit();
            }
            return objectId;
        }

        private static void CreateObjectConditions(FilterObject filterObject, SQLiteConnection connection)
        {
            const string insertConditions = @"INSERT INTO `filter_conditions` (filter_id, condition, value) VALUES 
                                (@id, @condition, @value);";
            using (var cmd = new SQLiteCommand(insertConditions, connection))
            {
                cmd.Parameters.AddWithValue("@id", filterObject.Id);
                cmd.Parameters.AddWithValue("@condition", "");
                cmd.Parameters.AddWithValue("@value", "");
                foreach (var condition in filterObject.Conditions)
                {
                    cmd.Parameters["@condition"].Value = condition.Name;
                    cmd.Parameters["@value"].Value = condition.Value;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        internal List<FilterObject> GetFilterObjects()
        {
            var filterList = new List<FilterObject>();
            using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
            {
                const string selectObjects = @"SELECT * FROM `filter_objects`;";
                using (var cmd = new SQLiteCommand(selectObjects, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id, order;
                            var newObject = new FilterObject
                            {
                                Id = int.TryParse(reader["id"].ToString(), out id) ? id : -1,
                                Title = reader["title"].ToString(),
                                Description = reader["description"].ToString(),
                                Order = int.TryParse(reader["order"].ToString(), out order) ? order : -1,
                                Show = reader["show"].ToString() == "1",
                            };
                            newObject.Conditions = GetFilterConditions(id);
                            filterList.Add(newObject);
                        }
                    }
                }
            }
            return filterList;
        }

        private List<FilterCondition> GetFilterConditions(int id)
        {
            var conditionList = new List<FilterCondition>();
            using (var connection = new SQLiteConnection(_connection).OpenAndReturn())
            {
                const string selectConditions = @"SELECT condition, value FROM `filter_condition` WHERE filter_id=@id;";
                using (var cmd = new SQLiteCommand(selectConditions, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var newCondition = new FilterCondition
                            {
                                Name = reader["condition"].ToString(),
                                Value = reader["value"].ToString()
                            };
                            conditionList.Add(newCondition);
                        }
                    }
                }
            }
            return conditionList;
        }
    }
}
