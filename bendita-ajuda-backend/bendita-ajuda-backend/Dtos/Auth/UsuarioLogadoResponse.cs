using bendita_ajuda_backend.Data.Entidades;

namespace bendita_ajuda_backend.Dtos.Auth;

public record UsuarioLogadoResponse(Guid Id, string Nome, string? Celular, string Papel, bool EhPrestador)
{
    public static UsuarioLogadoResponse De(Usuario usuario) =>
        new(usuario.Id, usuario.Nome, usuario.Celular, usuario.Papel.ToString(), EhPrestador: usuario.Prestador is not null);
}
