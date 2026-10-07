using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace bendita_ajuda_backend.Migrations
{
    /// <inheritdoc />
    public partial class Second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prestadores",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Cep = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    Bairro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    FotoUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Visivel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prestadores", x => x.UsuarioId);
                    table.ForeignKey(
                        name: "FK_Prestadores_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    NomePlural = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PalavrasChave = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Ordem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServicosSugeridos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Descricao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PrestadorId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicosSugeridos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicosSugeridos_Prestadores_PrestadorId",
                        column: x => x.PrestadorId,
                        principalTable: "Prestadores",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PrestadoresServicos",
                columns: table => new
                {
                    PrestadorId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ServicoId = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrestadoresServicos", x => new { x.PrestadorId, x.ServicoId });
                    table.ForeignKey(
                        name: "FK_PrestadoresServicos_Prestadores_PrestadorId",
                        column: x => x.PrestadorId,
                        principalTable: "Prestadores",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrestadoresServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Servicos",
                columns: new[] { "Id", "Nome", "NomePlural", "Ordem", "PalavrasChave" },
                values: new object[,]
                {
                    { "eletricista", "Eletricista", "Eletricistas", 1, "tecnico eletrico, tecnico em eletrica, eletrotecnico, eletrica, eletrico, luz, tomada, chuveiro, disjuntor, fio, fiacao, lampada, interruptor, curto, energia, queimou, choque, quadro de luz" },
                    { "encanador", "Encanador", "Encanadores", 2, "bombeiro hidraulico, hidraulica, agua, pia, vazamento, vazando, vaza, cano, torneira, descarga, privada, vaso sanitario, entupido, entupida, entupiu, esgoto, caixa d agua, registro, ralo" },
                    { "faxina", "Faxina", "Profissionais de faxina", 3, "faxineira, faxineiro, diarista, limpeza, limpar, passar roupa, lavar, sujeira" },
                    { "jardineiro", "Jardineiro", "Jardineiros", 6, "jardinagem, jardim, grama, planta, plantas, poda, podar, arvore, mato, quintal, rocar, horta" },
                    { "montador-de-moveis", "Montador de móveis", "Montadores de móveis", 7, "montar, montagem, movel, moveis, guarda roupa, armario, cama, estante, desmontar, prateleira, rack, mudanca" },
                    { "pedreiro", "Pedreiro", "Pedreiros", 4, "obra, reforma, parede, piso, reboco, muro, telhado, goteira, rachadura, azulejo, cimento, construcao, calcada, laje" },
                    { "pintor", "Pintor", "Pintores", 5, "pintura, pintar, tinta, descascando, mofo, textura, grafiato, verniz" },
                    { "tecnico-ar-condicionado", "Técnico de ar-condicionado", "Técnicos de ar-condicionado", 8, "ar, ar condicionado, split, climatizacao, nao gela, gelando, refrigeracao" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prestadores_Uf_Cidade_Bairro",
                table: "Prestadores",
                columns: new[] { "Uf", "Cidade", "Bairro" });

            migrationBuilder.CreateIndex(
                name: "IX_PrestadoresServicos_ServicoId",
                table: "PrestadoresServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_Nome",
                table: "Servicos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicosSugeridos_CriadoEm",
                table: "ServicosSugeridos",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosSugeridos_PrestadorId",
                table: "ServicosSugeridos",
                column: "PrestadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrestadoresServicos");

            migrationBuilder.DropTable(
                name: "ServicosSugeridos");

            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "Prestadores");
        }
    }
}
