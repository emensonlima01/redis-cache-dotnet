# Minimal API .NET (Pagamentos com Redis)

Exemplo didático em ASP.NET Core Minimal APIs (.NET 8) para processar pagamentos (CRUD) usando o Redis como banco de dados. Não há integração real com gateway; os dados são persistidos no Redis via StackExchange.Redis e expiram após 30 minutos.

## Requisitos
- .NET SDK 8.0+
- Docker 24+ e Docker Compose

## Como Executar

### Opção A) Docker Compose (API + Redis)
1. Entre na pasta `Solution` e suba os serviços:
   - `cd Solution`
   - `docker compose up -d --build`
2. A API sobe em `http://localhost:8080` (profile de container). O Redis fica acessível em `localhost:6379` (exposto pelo compose).
3. Para parar/remover: `docker compose down -v`

O arquivo `Solution/docker-compose.yml` já configura:
- Serviço `redis` (imagem `redis:7-alpine`, AOF on).
- Serviço `payment-api` com `Redis__ConnectionString=redis:6379` e porta `8080` publicada.

### Opção B) Local (SDK) + Redis em contêiner
1. Inicie o Redis localmente:
   - `docker run -d --name payment-redis -p 6379:6379 redis:7-alpine`
   - ou `cd Solution && docker compose up -d redis`
2. Rode a API em modo Development:
   - `dotnet run --project Solution/Payment.Api`
   - Endereços padrão: `https://localhost:7061` e `http://localhost:5175`
   - Se necessário, confie o certificado dev: `dotnet dev-certs https --trust`

Configuração do Redis (conexão) pode ser feita via `appsettings.json` ou variável de ambiente `Redis__ConnectionString`:
- `Solution/Payment.Api/appsettings.json` define "Redis:ConnectionString": "localhost:6379" por padrão.

## Endpoints
Base path: `/payments`

- POST `/payments` — cria um pagamento
  - Exemplo body:
    ```json
    {
      "cardNumber": "4111111111111111",
      "cardHolderName": "Maria Silva",
      "expirationDate": "12/28",
      "cvv": "123",
      "amount": 150.75,
      "currency": "BRL",
      "description": "Pedido #123"
    }
    ```
  - Resposta: 201 Created com `PaymentResponse` (número do cartão mascarado)
- GET `/payments/{id}` — busca por id
- PUT `/payments/{id}` — atualiza status/descrição
  - Body: `{ "status": "Approved|Pending|Declined|Cancelled|Refunded", "description": "..." }`
- DELETE `/payments/{id}` — remove

Exemplos rápidos (substitua `{id}` pelo `transactionId` retornado no POST):
- `curl -X POST http://localhost:8080/payments -H "Content-Type: application/json" -d '{"cardNumber":"4111111111111111","cardHolderName":"Maria Silva","expirationDate":"12/28","cvv":"123","amount":150.75,"currency":"BRL","description":"Pedido #123"}'`
- `curl http://localhost:8080/payments/{id}`
- `curl -X PUT http://localhost:8080/payments/{id} -H "Content-Type: application/json" -d '{"status":"Approved","description":"OK"}'`
- `curl -X DELETE http://localhost:8080/payments/{id}`

## Como o Redis é usado
- Conexão via `StackExchange.Redis` e `IConnectionMultiplexer` em `Solution/Payment.Api/Program.cs`.
- Camada de cache genérica: `IRedisCache` e `RedisCache` em `Solution/Payment.Api/Repositories/Base`.
- Repositório de pagamentos: `PaymentRepository` em `Solution/Payment.Api/Repositories` usa prefixo de chave `payment:` e TTL de 30 minutos por registro.

Principais trechos:
- Registro de serviços e endpoints: `Solution/Payment.Api/Program.cs:1`
- Repositório com TTL e prefixo: `Solution/Payment.Api/Repositories/PaymentRepository.cs:1`
- Docker Compose (Redis + API): `Solution/docker-compose.yml:1`
- Configuração de conexão: `Solution/Payment.Api/appsettings.json:1`

## Estrutura
```
Solution/Payment.Api
├── Program.cs                # Minimal API (CRUD pagamentos) + DI Redis
├── Repositories/
│  ├── Base/IRedisCache.cs    # Contrato cache genérico
│  ├── Base/RedisCache.cs     # Implementação via StackExchange.Redis
│  └── PaymentRepository.cs   # Repositório (prefixo payment:, TTL 30m)
├── Models/                   # Contracts (request/response/enums)
├── appsettings*.json
├── Dockerfile
└── Properties/launchSettings.json

Solution/docker-compose.yml   # Redis + API
```

## Observações
- Exemplo didático: não processa pagamento real nem armazena PAN completo; o cartão é mascarado na resposta.
- Ajuste `Redis:ConnectionString` conforme seu ambiente (ex.: `redis:6379` no compose, `localhost:6379` local).
- Tempo de expiração pode ser alterado no `PaymentRepository`.

## Referências
- Minimal APIs (ASP.NET Core): https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis
- StackExchange.Redis: https://stackexchange.github.io/StackExchange.Redis/
- .NET 8 SDK: https://dotnet.microsoft.com/download
- ASP.NET em contêiner: https://learn.microsoft.com/aspnet/core/host-and-deploy/docker/building-container-images

