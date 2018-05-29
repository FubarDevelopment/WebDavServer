using System.Data;

using FluentMigrator;

namespace FubarDev.WebDavServer.NHibernate.Migrations
{
    [Migration(20180529153500)]
    public class WebDavPropsStoreMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("props")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("entry_id").AsGuid().ForeignKey("filesystementries", "id").OnDelete(Rule.Cascade)
                .WithColumn("name").AsString(150)
                .WithColumn("language").AsString(150).Nullable()
                .WithColumn("value").AsString().Nullable();

            Create.Index().OnTable("props")
                .OnColumn("entry_id").Ascending()
                .OnColumn("name").Ascending()
                .WithOptions().Unique();
        }
    }
}
