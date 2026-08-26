# Martech Orders

API para gerenciamento de pedidos de um e-commerce simples, desenvolvida em **.NET 10** como parte de um desafio técnico para Desenvolvedor .NET Sênior.

A solução foi estruturada com foco em separação de responsabilidades, regras de domínio protegidas, testabilidade e simplicidade operacional.

## Tecnologias

- .NET 10
- ASP.NET Core Controllers
- Clean Architecture
- CQRS + MediatR
- FluentValidation
- Entity Framework Core
- SQLite
- JWT Bearer Authentication
- Serilog
- xUnit
- NSubstitute
- WebApplicationFactory
- Docker / Docker Compose

## Arquitetura

```text
src/
├── Martech.Orders.Domain
├── Martech.Orders.Application
├── Martech.Orders.Infrastructure
└── Martech.Orders.Api

tests/
├── Martech.Orders.UnitTests
└── Martech.Orders.IntegrationTests
```

### Domain

Concentra as entidades e regras de negócio do pedido:

- `Order`
- `OrderItem`
- `OrderStatus`
- regras de criação e cancelamento
- cálculo de `TotalAmount`

As invariantes permanecem protegidas pelo próprio domínio. Por exemplo, um pedido precisa possuir ao menos um item, `Quantity` e `UnitPrice` devem ser maiores que zero e apenas pedidos com status `Pending` podem ser cancelados.

### Application

Contém os casos de uso da aplicação, separados em commands e queries com MediatR:

- criação de pedido
- cancelamento de pedido
- consulta por ID
- listagem paginada
- validações com FluentValidation
- behaviors do pipeline MediatR
- abstração `IOrderRepository`

A camada Application não possui dependência de Entity Framework Core.

### Infrastructure

Responsável pela persistência:

- EF Core
- SQLite
- `ApplicationDbContext`
- implementação de `IOrderRepository`
- configurações de mapeamento
- migrations

As consultas específicas de EF Core, como `Include`, `AsNoTracking`, paginação e tracking, permanecem nesta camada.

### API

Responsável pela entrada HTTP e composição da aplicação:

- Controllers
- autenticação JWT
- configuração de dependências
- tratamento centralizado de exceções
- ProblemDetails
- OpenAPI
- configuração do Serilog

## Por que Controllers?

O desafio permite Controllers ou Minimal APIs.

Optei por **Controllers** porque Auth e Orders representam recursos HTTP bem definidos e essa abordagem deixa a camada de entrada explícita e simples de navegar durante uma revisão técnica. Os Controllers permanecem enxutos: recebem a requisição, fazem o mapeamento necessário e delegam o caso de uso ao MediatR.

As regras de negócio não ficam nos Controllers.

## Autenticação

O login utiliza o usuário fixo solicitado no desafio:

```text
Email: dev@martech.com
Password: Senha@123
```

Endpoint:

```text
POST /auth/login
```

Exemplo:

```bash
curl -X POST http://localhost:5289/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dev@martech.com","password":"Senha@123"}'
```

A resposta contém o token JWT que deve ser enviado nos endpoints de pedidos:

```text
Authorization: Bearer <token>
```

Todos os endpoints em `/api/orders` exigem autenticação.

## Endpoints

| Método | Rota | Descrição | Auth |
|---|---|---|---|
| `POST` | `/auth/login` | Autentica o usuário fixo e retorna JWT | Não |
| `POST` | `/api/orders` | Cria um pedido | Sim |
| `GET` | `/api/orders?page=1&pageSize=10` | Lista pedidos de forma paginada | Sim |
| `GET` | `/api/orders/{id}` | Consulta um pedido por ID | Sim |
| `PATCH` | `/api/orders/{id}/cancel` | Cancela um pedido `Pending` | Sim |

Exemplo de criação:

```json
{
  "customerId": "22222222-2222-2222-2222-222222222222",
  "items": [
    {
      "productName": "Widget",
      "quantity": 2,
      "unitPrice": 9.99
    }
  ]
}
```

A paginação utiliza por padrão:

```text
page = 1
pageSize = 10
```

## Validação e erros

As validações de entrada são executadas com FluentValidation através de um `ValidationBehavior` no pipeline do MediatR. O Domain mantém suas próprias invariantes independentemente dessas validações.

O tratamento de erros é centralizado com `IExceptionHandler` e `ProblemDetails`.

| Situação | HTTP |
|---|---:|
| Requisição inválida / FluentValidation | `400 Bad Request` |
| Pedido não encontrado | `404 Not Found` |
| Violação de regra de negócio | `409 Conflict` |
| Erro inesperado | `500 Internal Server Error` |

Erros inesperados não expõem stack trace ou detalhes internos na resposta HTTP.

## OpenAPI

O documento OpenAPI é disponibilizado em:

```text
GET /openapi/v1.json
```

Foi utilizado o suporte nativo do ASP.NET Core, sem adicionar uma UI adicional ao projeto.

## Executando localmente

Pré-requisitos:

- .NET 10 SDK

Na raiz do repositório:

```bash
dotnet restore
dotnet build Martech.Orders.slnx
dotnet run --project src/Martech.Orders.Api --urls http://localhost:5289
```

A API ficará disponível em:

```text
http://localhost:5289
```

Na inicialização, as migrations do EF Core são aplicadas automaticamente. Não é necessário executar `dotnet ef database update` manualmente.

A configuração local utiliza SQLite:

```text
Data Source=martech-orders.db
```

## Testes

Para executar toda a suíte:

```bash
dotnet test Martech.Orders.slnx
```

A solução possui **27 testes**:

- **22 testes unitários**
- **5 testes de integração**

### Testes unitários

Cobrem as principais invariantes do Domain e os handlers dos quatro casos de uso.

Os handlers são testados de forma isolada utilizando **NSubstitute** para `IOrderRepository`, sem EF Core ou banco de dados.

### Testes de integração

Os testes de integração utilizam `WebApplicationFactory<Program>` e exercitam a aplicação real, incluindo:

- autenticação JWT
- autorização dos endpoints
- criação de pedidos
- paginação
- cancelamento e tentativa de recancelamento
- validação HTTP
- tratamento de erros

O banco utilizado nesses testes é **SQLite real em memória** (`DataSource=:memory:`), mantendo uma conexão aberta durante a execução da factory. As migrations são aplicadas pelo startup normal da aplicação.

Não é utilizado o provider `Microsoft.EntityFrameworkCore.InMemory` nem autenticação falsa nos testes de integração.

## Logging

A aplicação utiliza Serilog com saída para console.

Commands e queries passam por um `LoggingBehavior` do MediatR, responsável por registrar:

- início da execução
- request
- response
- tempo de execução
- exceções

O behavior depende de `ILogger<T>`, mantendo a Application desacoplada do Serilog. A configuração concreta do Serilog fica na API.

A ordem principal do pipeline é:

```text
LoggingBehavior
    ↓
ValidationBehavior
    ↓
Handler
```

## Docker

Para construir e subir a aplicação:

```bash
docker compose build
docker compose up -d
```

A API fica disponível em:

```text
http://localhost:8080
```

O container utiliza build multi-stage com as imagens .NET 10 de SDK e runtime e executa com usuário não-root.

O SQLite é persistido em um named volume:

```text
martech-orders-data:/app/data
```

Por isso os pedidos permanecem disponíveis após um restart do container:

```bash
docker compose restart api
```

Para encerrar:

```bash
docker compose down
```

Esse comando remove container e network, mas preserva o volume. Para remover também os dados persistidos seria necessário usar `docker compose down -v`.

## Decisões de implementação

Algumas escolhas feitas para manter a solução simples e coerente com o tamanho do desafio:

- Controllers em vez de Minimal APIs.
- `IOrderRepository` específico para o agregado `Order`, sem repository genérico.
- regras de negócio protegidas no Domain.
- CQRS com commands e queries separados.
- mapeamento explícito entre contratos HTTP, commands e DTOs.
- Application sem dependência de EF Core.
- migrations aplicadas automaticamente no startup.
- `IExceptionHandler` + `ProblemDetails` para tratamento centralizado de erros.
- JWT simplificado e usuário fixo, conforme o escopo do desafio.
- SQLite real em memória nos testes de integração.
- Serilog integrado através das abstrações padrão de logging do .NET.

SonarQube e OpenTelemetry, listados como itens opcionais no desafio, não foram incluídos nesta entrega.

## Considerações para produção

Alguns pontos foram mantidos propositalmente simples por se tratar de um desafio técnico:

- a chave de assinatura JWT presente na configuração é apenas para desenvolvimento e deveria vir de secret store ou variável de ambiente em produção;
- o usuário de autenticação é fixo e em memória;
- uma aplicação real deveria definir uma política de redaction para logs conforme passasse a trafegar dados sensíveis;
- paginação, autenticação e observabilidade poderiam receber políticas adicionais conforme os requisitos operacionais do produto.
