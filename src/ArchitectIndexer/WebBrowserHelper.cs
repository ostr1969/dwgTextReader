using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArchitectIndexer
{
	public static class WebBrowserHelper
	{
		public static readonly DependencyProperty HtmlProperty =
			DependencyProperty.RegisterAttached(
				"Html",
				typeof(string),
				typeof(WebBrowserHelper),
				new PropertyMetadata(null, OnHtmlChanged));

		public static string GetHtml(DependencyObject obj) => (string)obj.GetValue(HtmlProperty);
		public static void SetHtml(DependencyObject obj, string value) => obj.SetValue(HtmlProperty, value);

		private static async void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			//if (d is WebBrowser webView && e.NewValue is string html)
			//{
			//	if (webView.CoreWebView2 == null)
			//	{
			//		await webView.EnsureCoreWebView2Async();
			//	}

			//	webView.NavigateToString(html);
			//}
			if (d is System.Windows.Controls.WebBrowser webBrowser && e.NewValue is string html)
			{
				webBrowser.NavigateToString(html);
			}
			else
			{
				throw new InvalidOperationException("WebBrowserHelper can only be used with a System.Windows.Controls.WebBrowser control.");
			}
		}
	}
}
