namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration8 : DbMigration
    {
        public override void Up()
        {
            AddColumn("ChangedFiles", "LeftFlowDocumentData", c => c.Binary());
            AddColumn("ChangedFiles", "RightFlowDocumentData", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("ChangedFiles", "RightFlowDocumentData");
            DropColumn("ChangedFiles", "LeftFlowDocumentData");
        }
    }
}
