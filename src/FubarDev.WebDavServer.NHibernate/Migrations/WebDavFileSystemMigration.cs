using System.Data;

using FluentMigrator;

namespace FubarDev.WebDavServer.NHibernate.Migrations
{
    [Migration(20180529103400)]
    public class WebDavFileSystemMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("filesystementries")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("parent_id").AsGuid()
                .WithColumn("path").AsString()
                .WithColumn("name").AsString(200)
                .WithColumn("invariant_name").AsString(200).Indexed()
                .WithColumn("collection").AsBoolean()
                .WithColumn("mtime").AsDateTime()
                .WithColumn("ctime").AsDateTime()
                .WithColumn("length").AsInt64()
                .WithColumn("etag").AsString(80);

            Create.Table("filesystementrydata")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("data").AsBinary();

            Create.Index()
                .OnTable("filesystementries")
                .OnColumn("parent_id").Ascending()
                .OnColumn("invariant_name").Ascending()
                .WithOptions().Unique();

            Create.ForeignKey("fk_entry_parent")
                .FromTable("filesystementries").ForeignColumn("parent_id")
                .ToTable("filesystementries").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);

            Create.ForeignKey("fk_data_entry")
                .FromTable("filesystementrydata").ForeignColumn("id")
                .ToTable("filesystementries").PrimaryColumn("id")
                .OnDelete(Rule.Cascade);
        }
    }
}
