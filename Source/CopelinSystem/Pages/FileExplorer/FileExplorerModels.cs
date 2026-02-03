using CopelinSystem.Models;

namespace CopelinSystem.Pages.FileExplorer
{
    public class SelectionEventArgs
    {
        public SelectionType Type { get; set; }
        public string? Region { get; set; }
        public string? Location { get; set; }
        public string? FinancialYear { get; set; }
        public ProjectList? Project { get; set; }
        public FileSystemItem? Folder { get; set; }
    }

    public class ContextMenuEventArgs : SelectionEventArgs
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public enum SelectionType
    {
        Region,
        Location,
        FinancialYear,
        Project,
        Folder,
        File
    }
    public class FileSystemSearchItem
    {
        public FileSystemItem? Item { get; set; }
        public string? ProjectName { get; set; }
        public string? Region { get; set; }
        public string? Location { get; set; }
    }
}
