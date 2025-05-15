# O projeto
O projeto é um sistema de analise e monitoramento de logs focado com o objetivo de criar uma aplicação de gerenciamento de funcionários em home office, onde o sistema irá monitorar os logs de acesso e saída dos funcionários, além de permitir o gerenciamento de tarefas e atividades.

# Ha fazer
- [x] Criar aplicação basica de monitoramento
- [x] Criar o basico da UI da aplicação
- [x] Conectar o back ao front
- [x] Testar o upload dados utilizando o banco PostgreSQL
- [ ] Sistema de Login (Login, Cadastro e Logout)
	- Logica do sistema de login:
		- O Usuario fará login com o id e senha
		- Os usuarios receberão um id e senha gerados pelo sistema
		- A logica da senha pode ser algo como: 4 primeiros digitos do CPF + dia e mes de nascimento. Ex: 85991708
		- O usuario poderá alterar a senha
		- O RH será responsável por criar os usuarios e enviar o id e senha para os funcionarios
		- [TALVEZ] O sistema envie um email para o usuario com o id e senha
- [ ] Melhorar UI
- [ ] Melhorar a formadação dos dados
- [x] Criar executavel <!-- C:source\repos\TCC_WPF\TCC_WPF\bin\Release\net8.0-windows -->
- [ ] Criar o icone do sistema
- [ ] Adicionar o icone do sistema
- [ ] Adicionar executavel ao github
- [ ] Conectar ao banco na nuvem
- [ ] Estudar e avaliar mais opções de monitoramento
	- [x] Inatividade
	- [ ] Envio de screenshots
- [ ] [OPÇÃO A] Bloquear o botão de fechamento se o monitoramento estiver ativo
- [ ] [OPÇÃO B] Ao fechar a aplicação, verificar se o monitoramento está ativo e, se estiver, em vez de parar a aplicação, apenas fechar a janela e fazer a aplicação continuar rodando em segundo plano.(Necessário criar um icone na barra de tarefas para o usuário poder acessar a aplicação novamente))
- [ ] Testar a aplicação em outros computadores
- [ ] Testar a aplicação em outros sistemas operacionais
- [x] Monitorar a inatividade do usuario
- [ ] Enviar os dados de inatividade para o banco
- [ ] Testar o sistema de inatividade