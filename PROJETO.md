# Bendita Ajuda

Sistema que conecta **clientes** a **prestadores de serviço** (eletricista, encanador, faxina, pedreiro etc.).

O público inclui muitos **idosos e pessoas com pouca instrução**, então a regra que guia todas as decisões é: **simples de usar, simples de manter**.

---

## Princípios de experiência (UX)

- **Buscar sem precisar entrar.** O login só aparece quando a pessoa quer chamar um prestador.
- **Sem senha.** Entrar é feito com Google ou com um código enviado por SMS.
- **"Entrar" e "criar conta" são o mesmo caminho.** O sistema descobre sozinho se a pessoa é nova; se for, pergunta só o nome.
- **Uma ação principal por tela**, botão "Voltar" sempre visível.
- **Letra a partir de 18px**, botões com no mínimo 48px de altura, alto contraste.
- **Ícone sempre acompanhado de texto.**
- **Palavras do dia a dia:** "Entrar", "Pronto", "Voltar". Nada de "autenticar", "perfil", "filtrar".
- **Mensagens de erro que dizem o que fazer**, não só o que deu errado.
- **Testar com pessoas reais do público** antes de dar uma tela por pronta.

---

## Fluxos principais

### Tela inicial (busca)
1. Pergunta grande: **"Do que você precisa?"**
2. Localização: "Perto de você — [bairro]", com opção de trocar.
3. **Botões de serviço** com ícone e nome (caminho principal, sem digitar nada).
4. Busca por **texto ou voz** (microfone), que também procura em palavras-chave
   (ex.: "pia", "vazamento" → Encanador).
5. Lista de prestadores em cards: foto, nome, profissão, bairro, estrelas e o botão
   **"Chamar no WhatsApp"**.
6. Rodapé: **"Quer oferecer seus serviços?"** → cadastro de prestador.

### Chamar no WhatsApp
- É só um **link** que abre o WhatsApp do próprio celular do cliente, já numa conversa com o
  prestador e uma mensagem pronta: `https://wa.me/55{numero}?text={mensagem}`.
  Não usa nenhuma API da Meta.
- Exige login antes, para: proteger o número do prestador, registrar o contato
  e permitir avaliação só de quem realmente chamou.
- Se a pessoa não estiver logada, vai para `/entrar?voltarPara=...` e volta depois.

### Entrar (`/entrar`)
1. **Continuar com Google** ou **Continuar com meu celular**.
2. Celular → recebe um **código de 6 números por SMS**.
3. Digita o código (teclado numérico, `autocomplete="one-time-code"`; com 6 números confirma sozinho).
4. Se for novo: **"Como você se chama?"**
5. Volta para onde estava (`voltarPara`) ou para a página inicial, já logado.

### Cadastro de prestador
Tela `/seja-prestador`. Sem login, vai para `/entrar?voltarPara=/seja-prestador` (o mesmo SMS)
e volta. Nome e celular já vêm da conta. Depois, **uma pergunta por tela** ("Passo 1 de 3"):
1. **O que você faz?** Botões de serviço (pode marcar mais de um) ou "Não achei meu serviço".
2. **Onde você atende?** Só o **CEP**: bairro, cidade e UF são preenchidos sozinhos (ViaCEP).
   O bairro pode ser corrigido ou ficar vazio (cidade pequena com CEP único).
3. **Confira** e toque em **Pronto**.

Tudo é salvo **numa chamada só, no fim**: não existe prestador pela metade no banco.
Foto e descrição são opcionais e ficam para depois.

**Endereço:** guardamos só CEP, bairro, cidade e UF. **O CEP nunca é mostrado ao cliente**
(em muitos lugares um CEP é uma rua só). Para o cliente: o bairro, ou a cidade se não tiver bairro.

**Sem aprovação manual:** quem escolhe nos botões aparece na busca na hora. A proteção vem da
lista fixa de serviços e de moderação depois do fato: botão "Denunciar" e, com várias denúncias,
o perfil some da busca (`Prestador.Visivel = false`) até alguém olhar.

**"Não achei meu serviço":** o que ele digitar é procurado nos nomes e palavras-chave
("bombeiro hidráulico" → Encanador). Achou: liga direto. Não achou: vira um `ServicoSugerido`
e aparece "em análise" para ele. Prestador só com serviço em análise ainda não aparece na busca.

**Tela do admin (`/admin`):** lista as sugestões, agrupando textos iguais. Para cada uma:
- **É o mesmo serviço que…** → liga o prestador ao serviço existente e o texto vira palavra-chave
  dele (o próximo que escrever igual cai direto, sem passar pelo admin);
- **Criar serviço novo** → cria o serviço na lista e liga o prestador;
- **Recusar** → serviço que não pode ser oferecido; só apaga.

Em todos os casos a sugestão é apagada, numa transação, e vale para todos que escreveram o
mesmo texto. *(Depois, com orçamento: IA faz essa análise e o admin só olha os casos duvidosos.)*

Um cliente pode virar prestador **sem criar outra conta**.

---

## Stack

| Camada | Tecnologia |
|---|---|
| Front-end | Angular com **NgModules** (não standalone), SCSS, lucide-angular, fonte Nunito |
| Back-end | **.NET 10**, ASP.NET Core com **Controllers** (não Minimal APIs) |
| Banco | **MySQL** + EF Core |
| Consultas | **Dapper** com projeções |
| Autenticação | **Cookie** nativo do ASP.NET Core (sem Identity, sem JWT) |
| Código de verificação | **SMS** via TrackMax, atrás de `IEnvioCodigo` |
| Documentação da API | OpenAPI nativo + Scalar (`/scalar` em desenvolvimento) |

---

## Arquitetura do back-end

Um único projeto de API, sem camadas separadas em projetos e **sem repositórios**
(o `DbContext` do EF Core já cumpre esse papel).

```
Controller  →  Service  →  AppDbContext (EF Core)     gravação + regras de negócio
Controller  →  Consultas (Dapper)                      leitura / telas
```

- **Controller:** porta de entrada. Recebe a requisição, chama o service, devolve a resposta.
- **Service:** onde ficam as operações e as regras de negócio.
- **AppDbContext:** ponte com o banco para gravar.
- **Consultas:** SQL com Dapper retornando exatamente o que a tela precisa.

### Estrutura de pastas

```
Controllers/
  AuthController.cs
  PrestadoresController.cs
  ServicosController.cs
  CepController.cs
  AdminSugestoesController.cs
Data/
  AppDbContext.cs
  ServicosIniciais.cs     ← seed da lista de serviços
  Entidades/
  Consultas/              ← Dapper + projeções (leitura)
Dtos/
  Auth/  Prestadores/  Servicos/  Cep/  Admin/
Migrations/               ← geradas pelo EF Core (Add-Migration)
Services/                 ← EF Core (gravação + regras)
  AuthService.cs
  PrestadorService.cs
  SugestaoService.cs      ← admin resolvendo serviços sugeridos
  Celular.cs              ← normalização do número
  Texto.cs, PalavrasChave.cs ← normalização de texto e sinônimos
  Cep/
    IConsultaCep.cs
    ConsultaCepViaCep.cs  ← CEP → bairro, cidade, UF
  EnvioCodigo/
    IEnvioCodigo.cs
    EnvioCodigoConsole.cs ← desenvolvimento (código no log)
    EnvioCodigoSms.cs     ← SMS de verdade (TrackMax)
    SmsOptions.cs
```

### Validação
- **Formato** (obrigatório, tamanho, 6 dígitos): DataAnnotations nos DTOs.
  Com `[ApiController]`, a API devolve 400 automaticamente.
- **Regras de negócio** (já é prestador, código vencido etc.): no Service.
- Mensagens de erro em **português simples**.

---

## Autenticação

- **Cookie** `bendita_sessao`: `HttpOnly`, `Secure`, `SameSite=Lax`.
- Sessão de **90 dias** com expiração deslizante (a pessoa raramente precisa entrar de novo).
- API devolve **401/403** em vez de redirecionar para página de login.
- Chaves do **DataProtection persistidas no banco** (senão todo deploy desloga todo mundo).
- Front e API no **mesmo domínio**; em desenvolvimento, o Angular usa **proxy** para `/api`
  (`proxy.conf.json`, dispensa CORS e `withCredentials`).
- Angular: o `AuthService` chama `/api/auth/eu` ao abrir o site para saber quem está logado.

### Endpoints

| Método | Rota | O que faz |
|---|---|---|
| POST | `/api/auth/celular/enviar-codigo` | Gera e envia o código (204; 503 se o SMS falhar) |
| POST | `/api/auth/celular/confirmar` | Valida o código, cria o usuário se novo e cria o cookie |
| POST | `/api/auth/google` | Valida o ID token do Google e cria o cookie |
| GET | `/api/auth/eu` | Quem está logado (401 se ninguém) |
| POST | `/api/auth/sair` | Apaga o cookie |

### Endpoints de prestador e serviços

| Método | Rota | Quem | O que faz |
|---|---|---|---|
| GET | `/api/servicos` | todos | Lista de serviços (botões) |
| GET | `/api/cep/{cep}` | logado | Bairro, cidade e UF do CEP |
| GET | `/api/prestadores/eu` | logado | Meu cadastro de prestador (404 se não sou) |
| POST | `/api/prestadores/eu` | logado | Vira prestador: serviços + CEP, tudo junto (201) |
| GET | `/api/admin/sugestoes` | Admin | Sugestões pendentes |
| POST | `/api/admin/sugestoes/{id}/similar` | Admin | `{ servicoId }`: liga ao serviço existente |
| POST | `/api/admin/sugestoes/{id}/novo` | Admin | `{ nome, nomePlural }`: cria serviço e liga |
| DELETE | `/api/admin/sugestoes/{id}` | Admin | Recusa |

### Regras do código de verificação
- 6 dígitos gerados com `RandomNumberGenerator`; salvo **só o hash** (HMACSHA256).
- Validade de **10 minutos**; máximo de **5 tentativas** por código.
- Limite de **3 códigos por número a cada 15 minutos** + rate limit por IP.
- Se o SMS não sair, o código é descartado (não conta no limite) e a pessoa vê
  "Não conseguimos enviar o SMS agora. Tente de novo em alguns minutos."
- Erro sempre com a mesma mensagem genérica (não revela se o número tem conta).
- Número novo sem nome → `{ precisaNome: true }`.
- Mensagem de SMS **curta e sem acentos** (acento reduz o limite de 160 para 70 caracteres
  e dobra o custo).

### Configuração do SMS
Em `appsettings.Development.json` (fora do git):

```json
"Sms": {
  "Token": "tmx_..."
}
```

Token gerado em **Meu Perfil** no portal TrackMax. Sem token, o código só aparece no log
(`EnvioCodigoConsole`) e nenhum crédito é gasto.

---

## Modelagem

| Entidade | Campos principais |
|---|---|
| `Usuario` | Id, Nome, Celular (único), CelularConfirmado, Email?, GoogleId?, Papel, CriadoEm |
| `Prestador` | UsuarioId (PK/FK, 1:1), Cep, Bairro?, Cidade, Uf, Bio?, FotoUrl?, Visivel, CriadoEm |
| `Servico` | Id (texto, ex.: `eletricista`), Nome, NomePlural, PalavrasChave, Ordem |
| `ServicoSugerido` | Id, Descricao, PrestadorId, CriadoEm (só existe enquanto está pendente) |
| `CodigoVerificacao` | Id, Celular, CodigoHash, ExpiraEm, UsadoEm, Tentativas, CriadoEm |
| `Contato` | Id, ClienteId, PrestadorId, CriadoEm |

- `enum Papel { Cliente, Admin }`. **Ser prestador = ter registro em `Prestador`.**
- `Prestador` ↔ `Servico` é **muitos para muitos**. No código são só as 2 classes (cada uma com
  a lista da outra); a tabela de ligação `PrestadoresServicos` é criada e mantida pelo EF Core.
- A lista de serviços é **fixa e padronizada**, mantida por nós via seed (`ServicosIniciais`).
  Não existe agrupamento em categorias por enquanto.
- Avaliações (`MediaAvaliacao`, `TotalAvaliacoes`) entram junto com o registro de contatos.
- O primeiro **Admin é criado por seed**; o cadastro público nunca cria admin.
- `CodigoVerificacao` é ligado ao **número**, não ao usuário (o usuário pode ainda não existir).
- Celular sempre salvo **normalizado** (só dígitos, DDD + número, sem o 55).
- CEP salvo só com os 8 dígitos; cidade e UF sempre vêm da consulta do CEP (não do que a tela manda).
- Collation `utf8mb4_0900_ai_ci` (ignora acentos e maiúsculas na busca).

---

## Linguagem ubíqua

Domínio em **português**; termos técnicos do framework em **inglês**
(`Controller`, `Service`, `DbContext`).

| Termo | Significado |
|---|---|
| **Usuário** | Qualquer pessoa com conta |
| **Cliente** | Usuário que procura e chama prestadores |
| **Prestador** | Usuário que oferece serviços (tem registro em `Prestador`) |
| **Admin** | Quem administra o sistema (lista de serviços, denúncias) |
| **Serviço** | O que o prestador faz, da lista padronizada (Eletricista, Encanador…) |
| **Contato** | Registro de que um cliente chamou um prestador |
| **Código** | Número de 6 dígitos enviado por SMS para entrar |

---

## Status

- [x] Modelagem e decisões de arquitetura
- [x] Tela inicial (busca) com dados mock
- [x] Envio de código por SMS (TrackMax)
- [x] Tela `/entrar` ligada ao back-end (celular + código + nome)
- [ ] Login com Google
- [x] Tabelas `Prestador` e `Servico` (muitos para muitos) + seed dos serviços
- [x] Cadastro de prestador (CEP + serviços + "não achei meu serviço")
- [x] Tela do admin para serviços sugeridos
- [ ] Editar cadastro de prestador (serviços, bairro, foto, descrição)
- [ ] Busca real com Dapper
- [ ] Registro de contatos e avaliações
- [ ] Denúncias (esconder perfil) e o primeiro Admin por seed

## Pendências e decisões em aberto

- **Idempotência nas gravações:** hoje o front trava o botão enquanto espera e o back barra
  duplicados por regra (limite de SMS, "já é prestador", "sugestão já resolvida"). Quando
  entrar o registro de contatos (botão do WhatsApp), usar uma chave de idempotência por ação
  (header `Idempotency-Key`), ou uma regra como "um contato por cliente e prestador a cada X minutos".
- **Derrubar sessões à força** (`SessionVersion` no usuário): planejado para depois.
- **App nativo:** se um dia existir, precisará de token (JWT) só para ele.
