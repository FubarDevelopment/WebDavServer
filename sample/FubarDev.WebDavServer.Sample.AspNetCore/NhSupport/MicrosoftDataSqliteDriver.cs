using System.Data;
using System.Data.Common;

using NHibernate.Driver;
using NHibernate.Engine;

namespace FubarDev.WebDavServer.Sample.AspNetCore.NhSupport
{
    public class MicrosoftDataSqliteDriver : global::NHibernate.Driver.DriverBase
    {
        public override bool UseNamedPrefixInSql => true;

        public override bool UseNamedPrefixInParameter => true;

        public override string NamedPrefix => "@";

        public override bool SupportsMultipleOpenReaders => false;

        public override bool SupportsMultipleQueries => true;

        public override bool SupportsNullEnlistment => false;

        public override bool HasDelayedDistributedTransactionCompletion => true;

        public override DbConnection CreateConnection()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection();
            connection.StateChange += Connection_StateChange;
            return connection;
        }

        public override DbCommand CreateCommand()
        {
            return new Microsoft.Data.Sqlite.SqliteCommand();
        }

        public override IResultSetsCommand GetResultSetsCommand(ISessionImplementor session)
        {
            return new BasicResultSetsCommand(session);
        }

        private static void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            if ((e.OriginalState == ConnectionState.Broken || e.OriginalState == ConnectionState.Closed || e.OriginalState == ConnectionState.Connecting) &&
                e.CurrentState == ConnectionState.Open)
            {
                var connection = (DbConnection)sender;
                using (var command = connection.CreateCommand())
                {
                    // Activated foreign keys if supported by SQLite.  Unknown pragmas are ignored.
                    command.CommandText = "PRAGMA foreign_keys = ON";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
