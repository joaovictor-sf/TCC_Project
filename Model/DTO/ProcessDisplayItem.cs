namespace TCC_MVVM.Model.DTO
{
    /// <summary>
    /// Representa um item de processo monitorado para exibição na interface,
    /// contendo o nome do aplicativo e o título da janela ativa.
    /// </summary>
    /// <param name="AppName">Nome do processo ou aplicativo monitorado.</param>
    /// <param name="WindowTitle">Título da janela ativa associada ao processo.</param>
    public record ProcessDisplayItem(string AppName, string WindowTitle);

    /*class ProcessDisplayItem {
        public string AppName { get; set; }
        public string WindowTitle { get; set; }

        public override bool Equals(object? obj) =>
            obj is ProcessDisplayItem other &&
            AppName == other.AppName &&
            WindowTitle == other.WindowTitle;

        public override int GetHashCode() => HashCode.Combine(AppName, WindowTitle);
    }*/
}
