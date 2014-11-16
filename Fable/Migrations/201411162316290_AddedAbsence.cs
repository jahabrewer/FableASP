namespace Fable.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAbsence : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Absences",
                c => new
                    {
                        AbsenceId = c.Int(nullable: false, identity: true),
                        Location = c.String(maxLength: 32),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        Description = c.String(maxLength: 512),
                        Absentee_Id = c.String(maxLength: 128),
                        Fulfiller_Id = c.String(maxLength: 128),
                        School_SchoolId = c.Int(),
                    })
                .PrimaryKey(t => t.AbsenceId)
                .ForeignKey("dbo.AspNetUsers", t => t.Absentee_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Fulfiller_Id)
                .ForeignKey("dbo.Schools", t => t.School_SchoolId)
                .Index(t => t.Absentee_Id)
                .Index(t => t.Fulfiller_Id)
                .Index(t => t.School_SchoolId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Absences", "School_SchoolId", "dbo.Schools");
            DropForeignKey("dbo.Absences", "Fulfiller_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Absences", "Absentee_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Absences", new[] { "School_SchoolId" });
            DropIndex("dbo.Absences", new[] { "Fulfiller_Id" });
            DropIndex("dbo.Absences", new[] { "Absentee_Id" });
            DropTable("dbo.Absences");
        }
    }
}
