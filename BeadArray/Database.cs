using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace BeadArray
{
    class Database
    {
        SQLiteConnection dbConnection;
        SQLiteCommand command;
        string sqlCommand;
        string dbPath = System.Environment.CurrentDirectory + "\\DB";
        string dbFilePath;

        public void createDbFile()
        {
            if (!string.IsNullOrEmpty(dbPath) && !Directory.Exists(dbPath))
                Directory.CreateDirectory(dbPath);
            dbFilePath = dbPath + "\\beadarray.db";
            if(!File.Exists(dbFilePath))
            {
                SQLiteConnection.CreateFile(dbFilePath);
            }
        }

        public string createDbConnection()
        {
            string strCon = string.Format("Data Source={0};", dbFilePath);
            dbConnection = new SQLiteConnection(strCon);
            dbConnection.Open();
            command = dbConnection.CreateCommand();
            return strCon;

        }
        public void createTables()
        {
            if (!checkIfExist("PALETTES"))
            {
                sqlCommand = "CREATE TABLE PALETTES(palette_name TEXT PRIMARY KEY, palette_colors TEXT)";
                executeQuery(sqlCommand);
            }

        }
        public bool addPalette(string name, string palette)
        {
            command.CommandText = "SELECT * FROM PALETTES WHERE palette_name=' " + name + "'";
            var result = command.ExecuteScalar();
            if (result == null)
                return false;
            executeQuery("INSERT INTO PALETTES (palette_name,palette_colors) VALUES('" + name + "','" + palette + "')");
            return true;
        }
        public void removePalette(string name)
        {
            executeQuery("DELETE FROM PALETTES WHERE palette_name = '" + name + "'");
        }
        public SQLiteDataReader readTable()
        {
            command.CommandText = "SELECT * FROM PALETTES";
            SQLiteDataReader reader = command.ExecuteReader();

            return reader;
        }

        public bool checkIfExist(string tableName)
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE name='" + tableName + "'";
            var result = command.ExecuteScalar();

            return result != null && result.ToString() == tableName ? true : false;
        }
        public void executeQuery(string sqlCommand)
        {
            SQLiteCommand triggerCommand = dbConnection.CreateCommand();
            triggerCommand.CommandText = sqlCommand;
            triggerCommand.ExecuteNonQuery();
        }

        public bool checkIfTableContainsData(string tableName)
        {
            command.CommandText = "SELECT count(*) FROM " + tableName;
            var result = command.ExecuteScalar();

            return Convert.ToInt32(result) > 0 ? true : false;
        }
    }
}
