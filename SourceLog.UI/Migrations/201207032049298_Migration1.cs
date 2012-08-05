namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "LogEntries",
                c => new
                    {
                        LogEntryId = c.Int(nullable: false, identity: true),
                        Revision = c.Int(nullable: false),
                        CommittedDate = c.DateTime(nullable: false),
                        Message = c.String(maxLength: 4000),
                        Author = c.String(maxLength: 4000),
                        Read = c.Boolean(nullable: false),
                        LogSubscription_LogSubscriptionId = c.Int(),
                    })
                .PrimaryKey(t => t.LogEntryId)
                .ForeignKey("LogSubscriptions", t => t.LogSubscription_LogSubscriptionId)
                .Index(t => t.LogSubscription_LogSubscriptionId);
            
            AddColumn("LogSubscriptions", "Url", c => c.String(maxLength: 4000));
            AddColumn("LogSubscriptions", "LogProviderTypeName", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropIndex("LogEntries", new[] { "LogSubscription_LogSubscriptionId" });
            DropForeignKey("LogEntries", "LogSubscription_LogSubscriptionId", "LogSubscriptions");
            DropColumn("LogSubscriptions", "LogProviderTypeName");
            DropColumn("LogSubscriptions", "Url");
            DropTable("LogEntries");
        }
    }
}
