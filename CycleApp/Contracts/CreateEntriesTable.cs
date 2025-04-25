using Microsoft.EntityFrameworkCore.Migrations;

public partial class CreateEntriesTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Entries",
            columns: table => new
            {
                EntryId = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                Date = table.Column<DateTime>(nullable: false),
                PeriodStarted = table.Column<bool>(nullable: false),
                PeriodEnded = table.Column<bool>(nullable: false),
                Note = table.Column<string>(maxLength: 500, nullable: true),
                Heaviness = table.Column<string>(maxLength: 20, nullable: true),
                Symptoms = table.Column<string>(maxLength: 200, nullable: true),
                Sex = table.Column<string>(maxLength: 20, nullable: true),
                Mood = table.Column<string>(maxLength: 20, nullable: true),
                Discharges = table.Column<string>(maxLength: 20, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Entries", x => x.EntryId);
                table.ForeignKey(
                    name: "FK_Entries_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Entries_UserId",
            table: "Entries",
            column: "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Entries");
    }
}