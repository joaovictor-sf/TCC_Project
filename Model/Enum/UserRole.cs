namespace TCC_MVVM.Model.Enum
{
    /// <summary>
    /// Define os papéis possíveis dos usuários no sistema.
    /// </summary>
    public enum UserRole {
        /// <summary>
        /// Administrador com acesso total.
        /// </summary>
        ADMIN,

        /// <summary>
        /// Recursos Humanos, com permissões limitadas de gestão.
        /// </summary>
        RH,

        /// <summary>
        /// Desenvolvedor, com acesso ao monitoramento.
        /// </summary>
        DEV
    }
}
