namespace SourceLog.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Migration7 : DbMigration
    {
        public override void Up()
        {
            AddColumn("ChangedFiles", "ChangeTypeValue", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("ChangedFiles", "ChangeTypeValue");
        }
    }
}
