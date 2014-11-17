namespace Fable.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedApplication : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Applications",
                c => new
                    {
                        ApplicationId = c.Int(nullable: false, identity: true),
                        ApplicationState = c.Int(nullable: false),
                        ApplicationStateModified = c.DateTime(nullable: false),
                        Absence_AbsenceId = c.Int(),
                        Applicant_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ApplicationId)
                .ForeignKey("dbo.Absences", t => t.Absence_AbsenceId)
                .ForeignKey("dbo.AspNetUsers", t => t.Applicant_Id)
                .Index(t => t.Absence_AbsenceId)
                .Index(t => t.Applicant_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Applications", "Applicant_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Applications", "Absence_AbsenceId", "dbo.Absences");
            DropIndex("dbo.Applications", new[] { "Applicant_Id" });
            DropIndex("dbo.Applications", new[] { "Absence_AbsenceId" });
            DropTable("dbo.Applications");
        }
    }
}
