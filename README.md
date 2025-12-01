# DevTrails Bank API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat&logo=docker)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=flat&logo=microsoft-sql-server)                                                      
![Tests](https://img.shields.io/badge/Tests-xUnit-102039?style=flat)

<img width="300" height="300" alt="SP-Studio (1)" src="https://github.com/user-attachments/assets/bfc6b38f-8ae2-43dc-95eb-37d7fb2d8961" />

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
  - Saldo Inicial Zero: Por segurança e consistência contábil, toda nova conta nasce com Saldo = 0.00.
  - Cada tipo de conta tem sua própria taxa:
  -   **Conta Corrente(CheckingAccount):** Tarifa de Manutenção: Possui uma taxa mensal fixa (Configurada em R$ 15,00).
  -   **Conta Poupança(SavingsAccount):** Rentabilidade: Isenta de taxas de manutenção e possui rendimento automático de 0.5% sobre o saldo.
  - **Transferências**: O sistema de transferências não altera apenas o saldo final. Para garantir auditoria completa, cada transferência gera 3 registros de transação: Taxa, Crédito e Débito.
- **Validações:** CPF, maioridade (16+ anos), valores positivos, e e-mails válidos garantidos via `FluentValidation`.


---

##  API Endpoints e Funcionalidades

Abaixo, o detalhamento do que cada controlador gerencia.

### AuthController (Autenticação & Identidade)
Responsável pela entrada segura no sistema. Utiliza **ASP.NET Core Identity**.

| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| **POST** | `/api/auth/registrar` | Cria um novo usuário no sistema (Identity User) e vincula automaticamente a um perfil de Cliente. Já realiza a validação de formato de e-mail e força de senha. |
| **POST** | `/api/auth/login` | Autentica as credenciais do usuário. Retorna um **Token JWT (Bearer)** que deve ser usado no cabeçalho `Authorization` de todas as requisições subsequentes. O token carrega as `Claims` de identidade do usuário. |

###  AccountsController (Gestão de Contas)
Gerencia o ciclo de vida das contas bancárias. Aplica validação estrita de **Ownership** (apenas o dono pode ver ou alterar sua conta).

| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| **POST** | `/api/contas` | Abre uma nova conta (Corrente ou Poupança) para o usuário logado. **Regra:** Valida se o cliente já possui uma conta daquele tipo (impedindo duplicidade). |
| **GET** | `/api/contas/{cpf}` | Lista todas as contas vinculadas a um CPF. |
| **GET** | `/api/contas/{numero}/extrato` | Gera o histórico financeiro da conta com paginação e filtro por datas. Retorna depósitos, saques, transferências e tarifas. |
| **PUT** | `/api/contas/{numero}/inativar` | Encerra uma conta. **Regra:** Só permite a inativação se o saldo for **Zero**. |
| **PUT** | `/api/contas/{numero}/reativar` | Reabre uma conta previamente inativada, permitindo novas movimentações. |

###  TransactionsController (Operações Financeiras)
Implementa o padrão **Unit of Work** para garantir que transações financeiras sejam atômicas (Tudo ou Nada).

| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| **POST** | `/api/transacoes/deposito` | Adiciona fundos a uma conta. Única operação que aumenta o saldo sem contrapartida de débito imediato (Cash-in). |
| **POST** | `/api/transacoes/saque` | Retira fundos de uma conta. **Concorrência:** Utiliza `RowVersion` para impedir saques simultâneos que negativem a conta.  |
| **POST** | `/api/transacoes/transferencia` | Realiza a movimentação entre contas (Internas). **Atomicidade:** Gera 3 registros em uma única transação (Débito Origem + Crédito Destino + Tarifa). Se qualquer passo falhar, o sistema realiza **Rollback** total. |

### ClientsController (Dados Cadastrais)
Gerencia os dados pessoais do correntista.

| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| **GET** | `/api/clientes/{cpf}` | Busca os dados cadastrais (Nome, CPF) de um cliente. Protegido para que apenas o próprio usuário ou administradores possam consultar. |
| **POST** | `/api/clientes` | (Uso Interno/Admin) Endpoint para criação de perfil de cliente, geralmente acionado automaticamente pelo fluxo de Registro de Usuário. |

### Notas de Segurança dos Endpoints

1.  **JWT Required:** Com exceção de `/auth`, todos os endpoints exigem um Token Bearer válido.
2.  **Anti-IDOR:** Todos os métodos que recebem `número da conta` ou `CPF` verificam se o recurso pertence ao ID do usuário contido no Token.
3.  **Input Validation:** Todos os `POST` e `PUT` passam pelo **FluentValidation** antes de chegar ao Controller. Requisições com dados inválidos retornam `400 Bad Request` imediatamente.

---

## Como Rodar (Docker)

O projeto foi configurado para rodar utilizando o Docker Compose.

**Pré-requisitos:** Docker Desktop instalado e rodando.

**Para rodar, faça os seguintes passos:**

1. Clone o repositório:
     ```bash
   git clone https://github.com/prestozin/DevTrails-BankProject.git

2. Abra o terminal na sua IDE e suba os containers:
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
