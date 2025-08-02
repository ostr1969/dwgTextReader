using Microsoft.WindowsAPICodePack.Dialogs;

namespace vscodeBlazorApp.Components.Models
{
    public interface IFolderPicker
    {
        public Task<string> DisplayFolderPicker();
    }

    public class FolderPicker : IFolderPicker
    {
        public async Task<string> DisplayFolderPicker()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
                return dialog.FileName;
            return "";
        }
    }
    public class FolderNode
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public List<FolderNode> Children { get; set; } = new();
        public static FolderNode GetFolderTree(string path)
        {
            var node = new FolderNode
            {
                Name = Path.GetFileName(path),
                FullPath = path
            };

            try
            {
                foreach (var dir in Directory.GetDirectories(path))
                {
                    node.Children.Add(GetFolderTree(dir));
                }
            }
            catch { /* Access denied, etc. */ }

            return node;
        }
    }
}
