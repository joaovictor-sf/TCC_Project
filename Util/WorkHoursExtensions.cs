using TCC_MVVM.Model.Enum;

namespace TCC_MVVM.Util
{
    /// <summary>
    /// Classe de extensão que converte valores do enum <see cref="WorkHours"/> para <see cref="TimeSpan"/>.
    /// </summary>
    public static class WorkHoursExtensions
    {
        // <summary>
        /// Converte o valor de <see cref="WorkHours"/> em um <see cref="TimeSpan"/> correspondente.
        /// </summary>
        /// <param name="horas">Valor do enum <see cref="WorkHours"/> a ser convertido.</param>
        /// <returns>Um <see cref="TimeSpan"/> correspondente à carga horária definida.</returns>
        public static TimeSpan ToTimeSpan(this WorkHours horas) {
            return horas switch
            {
                WorkHours.TESTE_30_SEGUNDOS => TimeSpan.FromSeconds(30),
                WorkHours.QUATRO_HORAS => TimeSpan.FromHours(4),
                WorkHours.SEIS_HORAS => TimeSpan.FromHours(6),
                WorkHours.OITO_HORAS => TimeSpan.FromHours(8),
                _ => TimeSpan.Zero
            };
        }
    }
}
