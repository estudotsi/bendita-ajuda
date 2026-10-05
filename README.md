# bendita-ajuda-producao

## Login com Google

O login usa o Google Identity Services no frontend e valida o ID token no backend.
O valor de `Google:ClientId` no `appsettings.json` deve ser o mesmo Client ID configurado
em `googleClientId` nos arquivos de ambiente do frontend. No Google Cloud Console, configure
esse OAuth Client como aplicativo Web e cadastre as origens onde o frontend será executado
(por exemplo, `http://localhost:4200` para desenvolvimento). Em produção, configure também
`Google:ClientId` no ambiente do backend com o Client ID correspondente.

## E-mail pelo Resend

O backend envia e-mails pela API do Resend usando o remetente
`Bendita Ajuda <noreply@benditaajuda.digital>`. Configure a chave da API fora do
repositório, usando a variável de ambiente `Resend__ApiKey` (ou o secret equivalente
no ambiente de hospedagem). O domínio `benditaajuda.digital` precisa estar verificado
na conta Resend usada por essa chave.
