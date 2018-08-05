using CryptoBlock.Utils.IO.SQLite.Queries;
using CryptoBlock.Utils.IO.SQLite.Schema;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.IO.SQLite;

namespace CryptoBlock
{
    namespace Utils.IO.SqLite
    {
        public class SQLiteDatabaseHandler
        {
            public class SQLiteDatabaseHandlerException : Exception
            {
                private string databaseFilePath;

                public SQLiteDatabaseHandlerException(
                    string databaseFilePath,
                    string exceptionDetails = null,
                    Exception innerException = null)
                    : base(formatExceptionMessage(databaseFilePath, exceptionDetails), innerException)
                {
                    this.databaseFilePath = databaseFilePath;
                }

                public string DatabaseFilePath
                {
                    get { return databaseFilePath; }
                }

                private static string formatExceptionMessage(string databaseFilePath, string exceptionDetails = null)
                {
                    string messagePostfix = exceptionDetails != null ?
                        string.Format(": {0}", exceptionDetails)
                        : ".";

                    return string.Format(
                        "An exception occurred while performing an operation on '{0}'{1}",
                        databaseFilePath,
                        messagePostfix);
                }
            }

            public class ConnectionToDatabaseAlreadyOpenException : SQLiteDatabaseHandlerException
            {
                public ConnectionToDatabaseAlreadyOpenException(string databaseFilePath, string operationName)
                    : base(databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Connection to database must be closed prior to performing following operation: '{0}'.",
                        operationName);
                }
            }

            public class ConnectionToDatabaseNotOpenException : SQLiteDatabaseHandlerException
            {
                public ConnectionToDatabaseNotOpenException(
                    string databaseFilePath,
                    string operationName)
                    : base(databaseFilePath, formatExceptionMessage(operationName))
                {

                }

                private static string formatExceptionMessage(string operationName)
                {
                    return string.Format(
                        "Connection to database must be opened prior to performing following operation: '{0}'.",
                        operationName);
                }
            }

            private readonly string filePath;

            private SQLiteConnection connection;

            public SQLiteDatabaseHandler(string filePath)
            {
                this.filePath = filePath;
            }

            public string FilePath
            {
                get { return filePath; }
            }

            public bool ConnectionOpen
            {
                get { return connection != null; }
            }

            public void OpenConnection()
            {
                assertConnectionToDatabaseNotAlreadyOpen("OpenConnection()");

                string connectionString = string.Format("Data Source={0}", filePath);

                // try opening a Sqlite connection 
                try
                {
                    connection = new SQLiteConnection(connectionString);
                    connection.Open();
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }

            }

            public void CloseConnection()
            {
                assertConnectionToDatabaseOpen("CloseConnection()");

                // try closing Sqlite connection 
                try
                {
                    connection.Close();
                    connection = null;
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            // creates a new db file, overwriting old one if exists
            public void CreateNewFile()
            {
                assertConnectionToDatabaseNotAlreadyOpen("CreateNewFile()");

                // try creating a new db file
                try
                {
                    SQLiteConnection.CreateFile(filePath);
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            public void CreateTable(TableSchema tableSchema)
            {
                // build query string
                string queryString = string.Format("CREATE {0}", tableSchema.GetQueryString());

                // execute query
                try
                {
                    executeWriteQuery(queryString);
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);                    
                }
            }

            public int UpdateTable(UpdateQuery updateQuery)
            {
                int numAffectedRows;

                // execute query
                try
                {
                    numAffectedRows = executeWriteQuery(updateQuery.QueryString);

                    return numAffectedRows;
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }         
            }

            public void DropTable(string tableName)
            {
                // build query string
                string queryString = string.Format("DROP TABLE {0}", tableName);
             
                // execute query
                try
                {
                    executeWriteQuery(queryString);
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            public void TruncateTable(string tableName)
            {
                // build query string
                string queryString = string.Format("DELETE FROM {0}", tableName);

                // execute query
                try
                {
                    executeWriteQuery(queryString);
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            public int InsertIntoTable(InsertQuery insertQuery)
            {
                // execute query
                try
                {
                    int numAffectedRows = executeWriteQuery(insertQuery.QueryString);

                    return numAffectedRows;
                }
                catch (SQLiteException sqliteException)
                {
                    throw new SQLiteDatabaseHandlerException(filePath, null, sqliteException);
                }
            }

            public ResultSet SelectFromTable(SelectQuery selectQuery)
            {
                return executeReadQuery(selectQuery.QueryString);
            }

            private int executeWriteQuery(string query)
            {
                SQLiteCommand command = new SQLiteCommand(query, connection);

                return command.ExecuteNonQuery();
            }

            private ResultSet executeReadQuery(string query)
            {
                SQLiteCommand command = new SQLiteCommand(query, connection);
                SQLiteDataReader sqliteDataReader = command.ExecuteReader();

                ResultSet resultSet = new ResultSet(sqliteDataReader);

                return resultSet;
            }

            private void assertConnectionToDatabaseNotAlreadyOpen(string operationName)
            {
                if (ConnectionOpen)
                {
                    throw new ConnectionToDatabaseAlreadyOpenException(filePath, operationName);
                }
            }

            private void assertConnectionToDatabaseOpen(string operationName)
            {
                if (!ConnectionOpen)
                {
                    throw new ConnectionToDatabaseNotOpenException(filePath, operationName);
                }
            }
        }

    }
}