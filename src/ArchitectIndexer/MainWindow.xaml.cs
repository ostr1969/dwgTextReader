using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArchitectIndexer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainVM VM;
		public MainWindow()
		{
			InitializeComponent();
			VM=this.DataContext as MainVM;
		}

		private async void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			var tb = (System.Windows.Controls.TextBox)sender;
			if (e.Key == Key.Enter)
			{
				VM.Query = tb.Text;
				await  VM.Search(); }

		}

		private void StackPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if(e.ClickCount==2)
				{var a = VM.SelectedResult;//= (sender as StackPanel).DataContext as DwgData;
				Debug.Print(a.file);
				Process.Start("explorer.exe", $"/select,\"{a.file}\"");
			}
		}
	}
}