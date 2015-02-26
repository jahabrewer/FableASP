namespace Fable.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAbsenceState : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Absences", "Absentee_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Absences", "School_SchoolId", "dbo.Schools");
            DropIndex("dbo.Absences", new[] { "Absentee_Id" });
            DropIndex("dbo.Absences", new[] { "School_SchoolId" });
            AddColumn("dbo.Absences", "State", c => c.Byte(nullable: false));
            AlterColumn("dbo.Absences", "Absentee_Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.Absences", "School_SchoolId", c => c.Int(nullable: false));
            CreateIndex("dbo.Absences", "Absentee_Id");
            CreateIndex("dbo.Absences", "School_SchoolId");
            AddForeignKey("dbo.Absences", "Absentee_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Absences", "School_SchoolId", "dbo.Schools", "SchoolId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Absences", "School_SchoolId", "dbo.Schools");
            DropForeignKey("dbo.Absences", "Absentee_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Absences", new[] { "School_SchoolId" });
            DropIndex("dbo.Absences", new[] { "Absentee_Id" });
            AlterColumn("dbo.Absences", "School_SchoolId", c => c.Int());
            AlterColumn("dbo.Absences", "Absentee_Id", c => c.String(maxLength: 128));
            DropColumn("dbo.Absences", "State");
            CreateIndex("dbo.Absences", "School_SchoolId");
            CreateIndex("dbo.Absences", "Absentee_Id");
            AddForeignKey("dbo.Absences", "School_SchoolId", "dbo.Schools", "SchoolId");
            AddForeignKey("dbo.Absences", "Absentee_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
