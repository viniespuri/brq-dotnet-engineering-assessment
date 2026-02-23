# Case 01 — Incidente em Banco de Dados & Serviço de Processamento Periódico

## O SERVIÇO TEVE LEVE ALTERAÇÃO PARA RODAR EM DOTNET8, MAS A SIMULAÇÃO DO PROBLEMA FOI REALIZADA COM SUCESSO
## AS REFERÊNCIAS DE LINHA ESTÃO BASEADAS NO NOVO CÓDIGO QUE ESTÁ NESSE REPOSITÓRIO

## 1. Contexto do Problema
Esse incidente aconteceu porque um serviço rodava o tempo todo, sem pausa, consultando o banco e gerando arquivos sem parar.  
Na prática, isso não só sobrecarregou o banco como aumentou uso de CPU e disco e ainda gerou resultados repetidos no processamento.

## 2. Análise de Causa Raiz
O problema principal foi o loop infinito na linha 17 do Program.cs, que chama 'engine.Execute()' sem intervalos.  
Com isso, o sistema tentava processar no limite máximo o tempo inteiro, resultando em timeout da aplicação por esgotamento do pool de conexões.

Além disso, os objetos de acesso ao banco  
**SqlConnection da linha 31**  
**SqlCommand da linha 32**  
**ExecuteReader na linha 34**  
não estavam sendo liberados corretamente, o que acumula conexões e piora a saúde do banco com o tempo.

Outro ponto crítico: a consulta sempre busca os "últimos 5 minutos" sem guardar até onde já processou.  
Na prática isso faz com que os mesmos dados voltem a ser processados várias vezes.

Outro ponto, na linha 40, é que cada execução cria um novo arquivo com 'Guid', o que gera muitos arquivos rapidamente e pressiona o disco.

Usar `new` como está sendo utilizado na linha 16 deixa o código acoplado e difícil de evoluir.  
Para objetos simples, ok. Para serviços, uma boa prática seria usar injeção de dependência.

## 3. Estratégia de Mitigação Imediata
Para estabilizar rápido em produção:

1. Controlar a frequência do serviço (colocar pausa entre ciclos) para reduzir carga no banco.  
2. Garantir liberação correta de conexão/comando/leitura a cada execução.  
3. Salvar um checkpoint (último item processado) para não repetir dados.  
4. Limitar quantos registros processar por ciclo para evitar picos.  
5. Fazer ajuste operacional temporário (reduzir ritmo do serviço e limpar arquivos antigos).  

Essas ações não resolvem tudo, mas reduzem risco imediato e mantêm o sistema de pé.

## 4. Melhorias Estruturais
**Observabilidade:** acompanhar métricas como tempo de execução, erros, volume processado e crescimento de arquivos ajuda a detectar problemas cedo.

**Resiliência:** aplicar timeout, tentativa de novo com intervalo e proteção contra falhas em sequência evita que o serviço entre em efeito cascata quando algo falha.

**Idempotência:** garantir que, se o mesmo evento entrar duas vezes, o resultado final continue único, evitando duplicidade de saída.

**Configuração e segurança:** mover parâmetros para configuração externa e proteger credenciais. São ajustes rápidos, com menor risco de vazamento.

## 5. Proposta de Redesenho Arquitetural
A proposta é trocar o modelo de “ficar perguntando ao banco” por um modelo orientado a eventos.

Em vez de o serviço consultar o banco o tempo todo, os eventos são enviados para uma fila.  
Um consumidor lê a fila, processa e confirma quando terminar.  
Com isso, fica mais fácil escalar horizontalmente e controlar progresso com offset/checkpoint real.

**Trade-off:** essa abordagem é mais robusta, mas também traz mais componentes para operar (fila, monitoramento, tratamento de falha).

## 6. Trade-offs e Considerações Operacionais
A correção rápida é mais barata e mais simples, mas tem limite de escala no médio prazo.

A arquitetura orientada a eventos aguenta melhor crescimento e falhas, porém custa mais para manter e exige mais disciplina operacional.

Para evitar exagero de solução, o ideal é evoluir por fases:
1. Estabilizar agora com controle de ciclo + checkpoint.  
2. Fortalecer observabilidade e proteção contra falhas.  
3. Migrar para eventos quando volume e criticidade justificarem.

## 7. Improvements Implemented in SafeExecutionEngine
- A execução foi separada do loop infinito do `Program.cs`, adotando um ciclo controlado por `Throttle()` e intervalo configurável (`HOTFIX_POLL_INTERVAL_MS`).
- O acesso ao banco passou a usar `using` para `SqlConnection`, `SqlCommand` e `SqlDataReader`, eliminando vazamento de conexões e reduzindo risco de esgotamento do pool.
- A consulta foi parametrizada com `@BatchSize`, `@Checkpoint` e `@CycleStart`, removendo SQL dinâmico e melhorando previsibilidade de execução.
- O processamento agora é incremental via checkpoint persistido em arquivo (`LoadCheckpoint`/`SaveCheckpoint`), evitando releitura contínua da mesma janela temporal.
- O limite de lote (`HOTFIX_BATCH_SIZE`) reduz picos de I/O e memória por ciclo, melhorando estabilidade sob carga.
- Foi aplicado controle de concorrência por `lock (_sync)`, evitando execução simultânea do mesmo engine e corrida de atualização de checkpoint.
- A geração de arquivos saiu de padrão “um arquivo por execução” para append diário com rotação (`RotateOutputFiles`), reduzindo crescimento descontrolado no disco.
- Configurações críticas foram externalizadas por variáveis de ambiente com validação e clamp (`ReadInt`), aumentando segurança operacional.
- Como limitação atual, o hotfix melhora robustez imediata, mas ainda não implementa `CancellationToken`, retry com backoff e circuit breaker.
