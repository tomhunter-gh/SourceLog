namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration3 : DbMigration
    {
        public override void Up()
        {
            //AddColumn("LogSubscriptions", "MaxDateTimeRetrieved", c => c.DateTime(nullable: false));
            AlterColumn("LogEntries", "Revision", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            AlterColumn("LogEntries", "Revision", c => c.Int(nullable: false));
            //DropColumn("LogSubscriptions", "MaxDateTimeRetrieved");
        }
    }
}
