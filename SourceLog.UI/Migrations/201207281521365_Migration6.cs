namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("ChangedFiles", "OldVersion", c => c.String());
            AddColumn("ChangedFiles", "NewVersion", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("ChangedFiles", "NewVersion");
            DropColumn("ChangedFiles", "OldVersion");
        }
    }
}
