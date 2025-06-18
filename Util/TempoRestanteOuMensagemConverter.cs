using System.Globalization;
using System.Windows.Data;

namespace TCC_MVVM.Util
{
    /// <summary>
    /// Converte o estado de conclusão da jornada de trabalho e o tempo restante em uma string apropriada para exibição.
    /// </summary>
    public class TempoRestanteOuMensagemConverter : IMultiValueConverter {

        /// <summary>
        /// Retorna uma mensagem indicando se o funcionário concluiu o horário de trabalho ou exibe o tempo restante.
        /// </summary>
        /// <param name="values">
        /// Espera-se que o primeiro valor seja um <see cref="bool"/> indicando se o horário foi concluído,
        /// e o segundo valor seja uma <see cref="string"/> representando o tempo restante.
        /// </param>
        /// <param name="targetType">Tipo de destino da conversão (normalmente <see cref="string"/>).</param>
        /// <param name="parameter">Parâmetro opcional de conversão (não utilizado).</param>
        /// <param name="culture">Cultura utilizada na conversão.</param>
        /// <returns>Uma string com a mensagem apropriada para exibição.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            bool completouHorario = values[0] is bool b && b;
            string tempoRestante = values[1]?.ToString() ?? "";

            return completouHorario ? "Você já completou seu horário de trabalho hoje." : tempoRestante;
        }

        /// <summary>
        /// Não implementado. Conversão reversa não é suportada.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
