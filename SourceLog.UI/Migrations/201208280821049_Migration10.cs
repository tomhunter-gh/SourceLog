namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration10 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "LogSubscriptions",
                c => new
                    {
                        LogSubscriptionId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 4000),
                        Url = c.String(maxLength: 4000),
                        LogProviderTypeName = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.LogSubscriptionId);
            
            CreateTable(
                "LogEntries",
                c => new
                    {
                        LogEntryId = c.Int(nullable: false, identity: true),
                        Revision = c.String(maxLength: 4000),
                        CommittedDate = c.DateTime(nullable: false),
                        Message = c.String(maxLength: 4000),
                        Author = c.String(maxLength: 4000),
                        Read = c.Boolean(nullable: false),
                        LogSubscription_LogSubscriptionId = c.Int(),
                    })
                .PrimaryKey(t => t.LogEntryId)
                .ForeignKey("LogSubscriptions", t => t.LogSubscription_LogSubscriptionId)
                .Index(t => t.LogSubscription_LogSubscriptionId);
            
            CreateTable(
                "ChangedFiles",
                c => new
                    {
                        ChangedFileId = c.Int(nullable: false, identity: true),
                        ChangeTypeValue = c.String(maxLength: 4000),
                        FileName = c.String(maxLength: 4000),
                        LeftFlowDocumentData = c.Binary(),
                        RightFlowDocumentData = c.Binary(),
                        FirstModifiedLineVerticalOffset = c.Double(nullable: false),
                        LogEntry_LogEntryId = c.Int(),
                    })
                .PrimaryKey(t => t.ChangedFileId)
                .ForeignKey("LogEntries", t => t.LogEntry_LogEntryId)
                .Index(t => t.LogEntry_LogEntryId);
            
        }
        
        public override void Down()
        {
            DropIndex("ChangedFiles", new[] { "LogEntry_LogEntryId" });
            DropIndex("LogEntries", new[] { "LogSubscription_LogSubscriptionId" });
            DropForeignKey("ChangedFiles", "LogEntry_LogEntryId", "LogEntries");
            DropForeignKey("LogEntries", "LogSubscription_LogSubscriptionId", "LogSubscriptions");
            DropTable("ChangedFiles");
            DropTable("LogEntries");
            DropTable("LogSubscriptions");
        }
    }
}
