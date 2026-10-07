using bendita_ajuda_backend.Data;
using bendita_ajuda_backend.Services;

namespace bendita_ajuda_backend.Tests;

/// <summary>Frase → serviço, com as palavras-chave reais que nascem no banco.</summary>
public class BuscaPorTextoTests
{
    private static string[] Servicos(string texto) =>
        BuscaPorTexto.ServicosDoTexto(ServicosIniciais.Todos, texto).Select(s => s.Id).ToArray();

    [Theory]
    [InlineData("minha pia está vazando", "encanador")]
    [InlineData("vazamento", "encanador")]
    [InlineData("DESCARGA", "encanador")]
    [InlineData("bombeiro hidráulico", "encanador")]
    [InlineData("Chuveiro", "eletricista")]
    [InlineData("tomada", "eletricista")]
    [InlineData("elétrica", "eletricista")]
    [InlineData("preciso de uma diarista", "faxina")]
    [InlineData("montar um guarda-roupa", "montador-de-moveis")]
    [InlineData("meu ar não gela", "tecnico-ar-condicionado")]
    public void Acha_o_servico_pelo_problema(string texto, string esperado) =>
        Assert.Equal([esperado], Servicos(texto));

    [Theory]
    [InlineData("encan", "encanador")]
    [InlineData("vaza", "encanador")]
    [InlineData("eletric", "eletricista")]
    [InlineData("tecnico", "tecnico-ar-condicionado")]
    public void Acha_enquanto_a_pessoa_digita(string texto, string esperado) =>
        Assert.Equal([esperado], Servicos(texto));

    [Theory]
    [InlineData("as tomadas da sala", "eletricista")]
    [InlineData("trocar as lâmpadas", "eletricista")]
    [InlineData("acender as luzes", "eletricista")]
    [InlineData("os canos estouraram", "encanador")]
    [InlineData("pias entupidas", "encanador")]
    public void Acha_no_plural(string texto, string esperado) =>
        Assert.Equal([esperado], Servicos(texto));

    [Fact]
    public void Termo_de_varias_palavras_so_vale_inteiro()
    {
        // "quadro de luz" é do Eletricista, mas "quadro" sozinho não é.
        Assert.Empty(Servicos("pendurar um quadro"));
        Assert.Equal(["eletricista"], Servicos("o quadro de luz desarmou"));
    }

    [Fact]
    public void Frase_com_palavras_de_dois_servicos_traz_os_dois() =>
        Assert.Equal(["encanador", "pedreiro"], Servicos("infiltração de água na parede"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("minha porta está emperrada")]
    [InlineData("preciso de ajuda aqui em casa")]
    public void Nao_inventa_servico(string texto) =>
        Assert.Empty(Servicos(texto));

    [Fact]
    public void Palavras_uteis_tiram_as_vazias_e_as_curtas() =>
        Assert.Equal(["pia", "vazando"], BuscaPorTexto.PalavrasUteis("minha pia esta vazando"));
}
