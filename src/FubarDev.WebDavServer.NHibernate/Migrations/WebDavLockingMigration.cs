using FluentMigrator;

namespace FubarDev.WebDavServer.NHibernate.Migrations
{
    [Migration(20180529143500)]
    public class WebDavLockingMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("locks")
                .WithColumn("id").AsString(200).PrimaryKey()
                .WithColumn("path").AsString()
                .WithColumn("href").AsString()
                .WithColumn("recursive").AsBoolean()
                .WithColumn("access_type").AsString()
                .WithColumn("share_mode").AsString()
                .WithColumn("timeout").AsInt64()
                .WithColumn("issued").AsDateTime()
                .WithColumn("last_refresh").AsDateTime().Nullable()
                .WithColumn("expiration").AsDateTime()
                .WithColumn("owner").AsString().Nullable();
        }
    }
}
