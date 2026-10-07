using bendita_ajuda_backend.Data.Entidades;

namespace bendita_ajuda_backend.Data;

/// <summary>
/// Serviços que já nascem no banco (seed via migration). Para incluir um serviço
/// ou um sinônimo, edite aqui e gere uma nova migration.
/// </summary>
public static class ServicosIniciais
{
    public static readonly Servico[] Todos =
    [
        new()
        {
            Id = "eletricista", Nome = "Eletricista", NomePlural = "Eletricistas", Ordem = 1,
            PalavrasChave = "tecnico eletrico, tecnico em eletrica, eletrotecnico, eletrica, eletrico, luz, tomada, "
                + "chuveiro, disjuntor, fio, fiacao, lampada, interruptor, curto, energia, queimou, choque, quadro de luz",
        },
        new()
        {
            Id = "encanador", Nome = "Encanador", NomePlural = "Encanadores", Ordem = 2,
            PalavrasChave = "bombeiro hidraulico, hidraulica, agua, pia, vazamento, vazando, vaza, cano, torneira, "
                + "descarga, privada, vaso sanitario, entupido, entupida, entupiu, esgoto, caixa d agua, registro, ralo",
        },
        new()
        {
            Id = "faxina", Nome = "Faxina", NomePlural = "Profissionais de faxina", Ordem = 3,
            PalavrasChave = "faxineira, faxineiro, diarista, limpeza, limpar, passar roupa, lavar, sujeira",
        },
        new()
        {
            Id = "pedreiro", Nome = "Pedreiro", NomePlural = "Pedreiros", Ordem = 4,
            PalavrasChave = "obra, reforma, parede, piso, reboco, muro, telhado, goteira, rachadura, azulejo, "
                + "cimento, construcao, calcada, laje",
        },
        new()
        {
            Id = "pintor", Nome = "Pintor", NomePlural = "Pintores", Ordem = 5,
            PalavrasChave = "pintura, pintar, tinta, descascando, mofo, textura, grafiato, verniz",
        },
        new()
        {
            Id = "jardineiro", Nome = "Jardineiro", NomePlural = "Jardineiros", Ordem = 6,
            PalavrasChave = "jardinagem, jardim, grama, planta, plantas, poda, podar, arvore, mato, quintal, rocar, horta",
        },
        new()
        {
            Id = "montador-de-moveis", Nome = "Montador de móveis", NomePlural = "Montadores de móveis", Ordem = 7,
            PalavrasChave = "montar, montagem, movel, moveis, guarda roupa, armario, cama, estante, desmontar, "
                + "prateleira, rack, mudanca",
        },
        new()
        {
            Id = "tecnico-ar-condicionado", Nome = "Técnico de ar-condicionado", NomePlural = "Técnicos de ar-condicionado", Ordem = 8,
            PalavrasChave = "ar, ar condicionado, split, climatizacao, nao gela, gelando, refrigeracao",
        },
    ];
}
