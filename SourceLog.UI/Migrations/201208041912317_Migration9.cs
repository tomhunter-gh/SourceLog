namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration9 : DbMigration
    {
        public override void Up()
        {
            AddColumn("ChangedFiles", "FirstModifiedLineVerticalOffset", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("ChangedFiles", "FirstModifiedLineVerticalOffset");
        }
    }
}
