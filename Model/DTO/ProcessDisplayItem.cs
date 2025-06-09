namespace TCC_MVVM.Model.DTO
{
    class ProcessDisplayItem{
        public string AppName { get; set; }
        public string WindowTitle { get; set; }

        public override bool Equals(object? obj) =>
            obj is ProcessDisplayItem other &&
            AppName == other.AppName &&
            WindowTitle == other.WindowTitle;

        public override int GetHashCode() => HashCode.Combine(AppName, WindowTitle);
    }
}
