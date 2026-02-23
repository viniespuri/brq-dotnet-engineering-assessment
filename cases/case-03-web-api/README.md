# Case 3 - Web API

## Problemas encontrados
- A rota `[Route("v1/users/")]` nao expoe claramente o `id` no caminho (`GET /v1/users/{id}`), deixando o contrato ambiguo.
- O metodo nao valida entrada invalida (`id <= 0`), entao pode processar requisicoes incorretas sem retornar `400 BadRequest`.
- Nao ha tratamento explicito de usuario nao encontrado para retornar `404 NotFound`.
- Instancia direta com `new UserService()` acopla controller a implementacao concreta e dificulta testes.
- Ausencia de padrao assincrono e cancelamento para operacoes de I/O (na versao mais robusta esperada em APIs modernas).

## Correcao sugerida
- Ajustar a rota para `GET /v1/users/{id}` e manter a assinatura compativel quando for a versao no estilo do teste.
- Validar `id` no inicio do metodo (`id > 0`), retornando `BadRequest` quando invalido.
- Retornar `NotFound` quando o servico nao localizar o usuario.
- Retornar `Ok(usuario)` no sucesso (`200`).
- Separar duas abordagens:
  - Versao aderente ao teste: Web API classico com `IHttpActionResult`, `[FromUri]` e `new UserService()`.
  - Versao best practices: ASP.NET Core com DI (`IUserService`), async, `CancellationToken` e `ActionResult<UserDto>`.

## Conclusao objetiva
O endpoint original possui falhas de contrato de rota, validacao e tratamento de status HTTP. A correcao minima e ajustar rota/validacao/retornos mantendo o estilo legado solicitado. A correcao completa aplica boas praticas modernas (DI, async e tipagem de resposta) sem quebrar compatibilidade.
