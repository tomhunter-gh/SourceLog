namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration4 : DbMigration
    {
        public override void Up()
        {
            DropColumn("LogSubscriptions", "MaxDateTimeRetrieved");
        }
        
        public override void Down()
        {
            AddColumn("LogSubscriptions", "MaxDateTimeRetrieved", c => c.DateTime(nullable: false));
        }
    }
}
