namespace Fable.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MovedRequiredAttributeOnAbsenceSchoolToForeignKey : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Absences", name: "School_SchoolId", newName: "SchoolId");
            RenameIndex(table: "dbo.Absences", name: "IX_School_SchoolId", newName: "IX_SchoolId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Absences", name: "IX_SchoolId", newName: "IX_School_SchoolId");
            RenameColumn(table: "dbo.Absences", name: "SchoolId", newName: "School_SchoolId");
        }
    }
}
