using System.Data;
using Data.Core;
using Microsoft.Data.SqlClient;


namespace Infrastructure
{
    public class ExceptionHandler
    {
        private readonly CoreSQLDbHelper _sqlHelper;

        public ExceptionHandler(CoreSQLDbHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public void LogError(Exception ex)
        {
            try
            {
                var query = @"INSERT INTO core.ErrorLogs (ErrorMessage, StackTrace, Source, CreatedAt)
                              VALUES (@ErrorMessage, @StackTrace, @Source, GETDATE())";

                var parameters = new[]
                {
                    new SqlParameter("@ErrorMessage", ex.Message),
                    new SqlParameter("@StackTrace", (object)ex.StackTrace ?? DBNull.Value),
                    new SqlParameter("@Source", (object)ex.Source ?? DBNull.Value)
                };
                
                _sqlHelper.ExecuteNonQuery(query, parameters);
            }
            catch
            {
                // Avoid throwing exceptions inside the logger to prevent infinite loops
            }
        }
    }
}
