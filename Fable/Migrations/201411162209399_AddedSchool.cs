namespace Fable.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSchool : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Schools",
                c => new
                    {
                        SchoolId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        StreetAddress = c.String(),
                    })
                .PrimaryKey(t => t.SchoolId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Schools");
        }
    }
}
