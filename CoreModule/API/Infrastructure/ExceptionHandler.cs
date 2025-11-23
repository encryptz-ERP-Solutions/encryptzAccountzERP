using System;
using System.Data;
using Npgsql;

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
                var query = @"INSERT INTO core.error_logs (error_message, stack_trace, source, created_at)
                              VALUES (@ErrorMessage, @StackTrace, @Source, NOW() AT TIME ZONE 'UTC')";

                var parameters = new[]
                {
                    new NpgsqlParameter("@ErrorMessage", ex.Message),
                    new NpgsqlParameter("@StackTrace", (object)ex.StackTrace ?? DBNull.Value),
                    new NpgsqlParameter("@Source", (object)ex.Source ?? DBNull.Value)
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
