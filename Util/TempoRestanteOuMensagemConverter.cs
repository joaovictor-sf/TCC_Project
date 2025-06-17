using System.Globalization;
using System.Windows.Data;

namespace TCC_MVVM.Util
{
    public class TempoRestanteOuMensagemConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            bool completouHorario = values[0] is bool b && b;
            string tempoRestante = values[1]?.ToString() ?? "";

            return completouHorario ? "Você já completou seu horário de trabalho hoje." : tempoRestante;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
