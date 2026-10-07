namespace bendita_ajuda_backend.Data.Entidades;

public class Usuario
{
    public Guid Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Somente dígitos, com DDD e sem o 55 (ex.: "61999998888").</summary>
    public string? Celular { get; set; }

    public bool CelularConfirmado { get; set; }

    public string? Email { get; set; }

    public string? GoogleId { get; set; }

    public Papel Papel { get; set; } = Papel.Cliente;

    public DateTime CriadoEm { get; set; }

    /// <summary>Preenchido só se a pessoa também oferece serviços.</summary>
    public Prestador? Prestador { get; set; }
}
