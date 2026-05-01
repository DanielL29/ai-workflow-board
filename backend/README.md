# AI Workflow Board Backend

Base inicial em .NET 8 para um MVP de board infinito com IA inspirado no board do Freepik.

## O que ja vem pronto

- Solucao em camadas com API, worker, application, domain, infrastructure e shared.
- CRUD inicial de `Board`, `Node` e `Edge`.
- Fila simples para jobs de geracao e worker assincromo.
- `SignalR` preparado para colaboracao em tempo real.
- Endpoint inicial de assistente virtual com RAG e suporte a Ollama.
- `Docker Compose` com API, worker, Postgres e Redis.
- Postgres preparado com extensao `pgvector` no ambiente Docker.
- Base inicial de memoria semantica por board com chunks e embeddings.
- Ingestao automatica de nos para memoria vetorial e contexto do assistente.
- Pontos de extensao para plugar providers de imagem, LLM, embeddings, RAG e assistente virtual.

## Fluxo do MVP

1. Criar um board.
2. Adicionar nos de texto, prompt, imagem ou assistente.
3. Encadear os nos por meio de edges.
4. Enfileirar a geracao de um node.
5. O worker consome o job e grava um resultado mockado no node.
6. Documentos e nos alimentam a memoria semantica do board para RAG.
7. O assistente usa busca semantica e pode chamar o Ollama quando habilitado.

## Migrations

Criar migration:

```bash
dotnet ef migrations add <NomeDaMigration> --project src/AiBoard.Infrastructure --startup-project src/AiBoard.Api --context AiBoard.Infrastructure.Persistence.AiBoardDbContext --output-dir Persistence/Migrations
```

Aplicar no banco:

```bash
dotnet ef database update --project src/AiBoard.Infrastructure --startup-project src/AiBoard.Api --context AiBoard.Infrastructure.Persistence.AiBoardDbContext
```

## Proximos passos recomendados

1. Adicionar autenticacao e multi-tenant.
2. Trocar o provider stub por Ollama local, OpenAI, Freepik, Stability, Flux ou outro provider.
3. Persistir eventos de colaboracao do board via SignalR.
4. Ligar geracao de imagens e chat a providers reais conforme a stack escolhida.
5. Criar migrations do EF e pipeline de CI.
