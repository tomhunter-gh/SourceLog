namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration5 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ChangedFiles",
                c => new
                    {
                        ChangedFileId = c.Int(nullable: false, identity: true),
                        FileName = c.String(maxLength: 4000),
                        OldVersion = c.Binary(maxLength: 4000),
                        NewVersion = c.Binary(maxLength: 4000),
                        LogEntry_LogEntryId = c.Int(),
                    })
                .PrimaryKey(t => t.ChangedFileId)
                .ForeignKey("LogEntries", t => t.LogEntry_LogEntryId)
                .Index(t => t.LogEntry_LogEntryId);
            
        }
        
        public override void Down()
        {
            DropIndex("ChangedFiles", new[] { "LogEntry_LogEntryId" });
            DropForeignKey("ChangedFiles", "LogEntry_LogEntryId", "LogEntries");
            DropTable("ChangedFiles");
        }
    }
}
