# DevTrails Bank API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat&logo=docker)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=flat&logo=microsoft-sql-server)
![Tests](https://img.shields.io/badge/Tests-xUnit-102039?style=flat)

Uma API bancária feita para o projeto DevTrails .NET da Solutis.
---

## Tecnologias utilizadas

- **.NET 8**
- **Entity Framework Core** 
- **SQL Server** 
- **Identity e JWT Bearer** 
- **Docker e Docker Compose** 
- **xUnit e Moq** 
- **FluentValidation**
- **Swagger/OpenAPI** 

---

## Arquitetura 

### 1. Concorrência Otimista 
Prevenção contra **Race Conditions*.
- **O Problema:** Dois dispositivos tentando sacar o saldo total simultaneamente.
- **A Solução:** Implementação de `RowVersion` no SQL Server. O EF Core verifica se o registro foi alterado por outra transação antes de salvar. Se houver conflito, a segunda operação é rejeitada automaticamente, garantindo consistência absoluta.

### 2. Integridade Transacional 
Uso do padrão **Unit of Work** para orquestrar operações de escrita.
- Garante que operações complexas (Criar Cliente + Criar Conta) sejam **Atômicas**. Ou tudo passa com sucesso, ou tudo falha (Rollback automático em caso de falha).

### 3. Polimorfismo no Banco de Dados 
Aplicação de conceitos de Orientação a Objetos.
- Utilização da estratégia **Table-Per-Hierarchy (TPH)** do EF Core.
- Uma única tabela `Accounts` armazena tanto `CheckingAccount` quanto `SavingsAccount`.

### 4. Segurança de Propriedade
Implementação de **Resource-Based Authorization**.
- Não basta ter um Token JWT válido. O sistema verifica se o `UserId` da conta é o mesmo do usuário da conta que está tentando movimentar. Garantindo que apenas o dono da conta possa fazer alterações.

### 5. Validações
- **FluentValidation:** Todas as entradas são validadas antes mesmo de chegarem à camada de serviço, economizando processamento.
- **Global Exception Handler:** Middleware customizado que captura erros e retorna respostas padronizadas para o cliente.

---

## Segurança e Regras de Negócio

### Autenticação e Autorização
- **Login Seguro:** Integração com ASP.NET Core Identity.
- **Proteção de Rotas:** Endpoints críticos exigem Token JWT (`[Authorize]`).
- **Validação de Propriedade:** Um usuário logado só pode visualizar, sacar ou inativar **suas próprias contas**. O sistema valida o `UserId` do token contra o dono do recurso no banco.

### Regras Bancárias
- **Atomicidade:** Saques, depósitos e transferências ocorrem dentro de uma transação. Se uma etapa falhar, tudo é desfeito (Rollback).
- **Saldo:** Impedimento de saques/transferências sem saldo suficiente.
- **Contas:**
  - Um cliente pode ter apenas uma Conta Corrente e uma Conta Poupança.
  - Contas só podem ser inativadas se o saldo for zero.
  - Taxas automáticas aplicadas em transferências.
- **Validações:** CPF, maioridade (16+ anos), valores positivos, e e-mails válidos garantidos via `FluentValidation`.

---

## Como Rodar (Docker)

O projeto foi configurado para rodar utilizando o Docker Compose.

**Pré-requisitos:** Docker Desktop instalado e rodando.

**Para rodar, faça os seguintes passos:**

1. Clone o repositório:
   ```bash
   git clone [https://github.com/seu-usuario/DevTrails-BankProject.git](https://github.com/seu-usuario/DevTrails-BankProject.git)](https://github.com/prestozin/DevTrails-BankProject.git)

2. Suba os containers:
   ```bash
    docker-compose up --build
  
3. Acesse o Swagger:
    ```bash
    http://localhost:5000/swagger

  ---
  
  # xUNIT Tests

  ## Como rodar os testes

1. Via VS Code/Visual Studio: Abra o Test Explorer e clique em "Run All".
   
3. Via Terminal:
   Rode os seguintes comandos:
   ```bash
    cd DevTrails.Tests
    dotnet test
   
  ---
  
 ## Testes Disponíveis:

- TransactionServiceTest:	Testa Depósitos, Saques (com e sem saldo), Transferências (cálculo de taxas) e Rollbacks.
- AccountServiceTest:	Testa as regras de conta única por tipo e inativação segura (apenas saldo zero).
- ClientServiceTest:	Testa a criação do usuário (Cliente + Conta) e validação de duplicidade de CPF.
- TokenServiceTest:	Faz uma simulação do appsettings e geração válida de JWT.
- ValidatorTest:	Faz verificação das regras de entrada (FluentValidation).
