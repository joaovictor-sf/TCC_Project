# O projeto
O projeto é um sistema de analise e monitoramento de logs focado com o objetivo de criar uma aplicação de gerenciamento de funcionários em home office, onde o sistema irá monitorar os logs de acesso e saída dos funcionários, além de permitir o gerenciamento de tarefas e atividades.

# DO TO LIST
- [x] Criar aplicação basica de monitoramento
- [x] Criar o basico da UI da aplicação
- [x] Conectar o back ao front
- [x] Testar o upload dados utilizando o banco PostgreSQL
- [x] Criar executavel <!-- C:source\repos\TCC_WPF\TCC_WPF\bin\Release\net8.0-windows -->
- [x] Monitorar a inatividade do usuario
- [x] Enviar os dados de inatividade para o banco
- [x] Testar o sistema de inatividade
- [x] Organizar o sistema para seguir o padrão MVVM
- [x] Sistema de login funcional
- [x] Sistema de Cadastro Funcional
- [x] Substuir o uso de Click por Command do Login
- [x] Substuir o uso de Click por Command do Cadastro
- [x] Fazer o botão de Login e Cadastro funcionar com o Enter
- [x] Mudar o nome da Role "Funcionario" para algo mais apropriado
- [x] Update do Design
- [x] Adicionar View com lista de usuários cadastrados
- [x] Mudar atributo de Role para Enum
- [x] Criar um Enum de horas de trabalho(WorkHours)(Ex: 8h, 6h, 4h, etc)
- [x] Criar um Boolean de Status de Usuário(isActive)
- [x] Implementar ao projeto principal
- [x] Adicionar a função de editar
- [x] Não permitir usuarios demitidos logarem
- [x] Adicionar de deminir usuários
- [x] Criar função de criar nova senha após o primeiro login do usuario
- [x] Atualizar a formatação dos emails validos no cadastro e edit
- [x] Restringir o acesso da Role Funcionario(User) a apenas a janela de monitoramento)
- [x] Melhorar a formadação dos dados
	- [x] Melhorar a formatação dos dados de tempo de ProcessLog
		- [x] Update UsageTime
		- [x] Remove StartTime e Timestamp para maior clareza
	- [x] Melhorar a formatação dos dados de tempo de InactivityLog
		- [x] Muda TimeStamp para Date
		- [x] Ao adicionar dados no mesmo dia, somar o tempo de inatividade ao invés de criar um novo registro
	- [x] Ajustar o Date de DailyWorkLogs
- [x] Fazer o UserList atualizar após cadastro
- [x] Melhorar o design da janela de monitoramento
	- [x] Adicionar o restante do horário de trabalho do dia do usuário
	- [x] Realizar um teste de 4 horas de trabalho para verificar se o horário é atualizado corretamente 
	- [x] Adicionar uma lista com os AppName que estão rodando no momento e serão monitorados
	- [x] Melhorar o design em si
- [x] Criar opção de Logout
- [x] Remover as MensageBoxs após o fim dos testes
- [x] Criar comentários explicativos no código(Acho que o C# tem algo semelhante ao Javadoc do Java)
	- [x] Comentar o Infra e Util
	- [x] Comentar o Model
	- [x] Comentar o Service
	- [x] Comentar o MVVM

# LISTA DE ERROS:
- (Eu acho) O sistema está respondendo como um outro dia no horário 21 por algum motivo

# MAYBE
- [ ] Enviar senha por email
- [ ] Editar o cadastro e o edit para avisar o porquê o usuário não poder ser cadastrado ou editado(ex: email já cadastrado, cpf já cadastrado, etc))
- [ ] Estudar e avaliar mais opções de monitoramento
	- [x] Inatividade
	- [ ] Envio de screenshots
- [ ] Mudar o nome username para cpf
- [ ] Decidir em como será implementado o sistema de beneficios da role Admin, que poderá acessar todas as janelas + funcionalidades exclusivas. Algumas ideias dessas funionalidades: 
	- [x] Apenas o Admin poderá cadastrar outro Admin
	- [ ] Criação de novas Roles
	- [ ] Criação de novas WorkHours
	- [ ] A capacidade de apagar completamente um usuário do banco de dados
	- [x] Apenas o ADMIN tem acesso a crianção de funcionarios com certas Roles e WorkHours
	- [ ] Apenas o ADMIN pode editar um usuario ADMIN
- [ ] Adicionar o icone do sistema
- [ ] Conectar ao banco na nuvem
- [ ] [OPÇÃO A] Bloquear o botão de fechamento se o monitoramento estiver ativo
- [ ] [OPÇÃO B] Ao fechar a aplicação, verificar se o monitoramento está ativo e, se estiver, em vez de parar a aplicação, apenas fechar a janela e fazer a aplicação continuar rodando em segundo plano.(Necessário criar um icone na barra de tarefas para o usuário poder acessar a aplicação novament)e)
- [ ] Testar a aplicação em outros computadores
- [ ] Testar a aplicação em outros sistemas operacionais
