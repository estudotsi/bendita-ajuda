using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace bendita_ajuda_backend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CodigosVerificacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Celular = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    CodigoHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    ExpiraEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UsadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Tentativas = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosVerificacao", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FriendlyName = table.Column<string>(type: "longtext", nullable: true),
                    Xml = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Celular = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    CelularConfirmado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    GoogleId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Papel = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosVerificacao_Celular_CriadoEm",
                table: "CodigosVerificacao",
                columns: new[] { "Celular", "CriadoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Celular",
                table: "Usuarios",
                column: "Celular",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_GoogleId",
                table: "Usuarios",
                column: "GoogleId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigosVerificacao");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
