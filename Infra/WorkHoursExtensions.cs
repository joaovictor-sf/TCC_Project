using TCC_MVVM.Model.Enum;

namespace TCC_MVVM.Infra
{
    public static class WorkHoursExtensions
    {
        public static TimeSpan ToTimeSpan(this WorkHours horas) {
            return horas switch
            {
                WorkHours.QUATRO_HORAS => TimeSpan.FromHours(4),
                WorkHours.SEIS_HORAS => TimeSpan.FromHours(6),
                WorkHours.OITO_HORAS => TimeSpan.FromHours(8),
                _ => TimeSpan.Zero
            };
        }
    }
}
