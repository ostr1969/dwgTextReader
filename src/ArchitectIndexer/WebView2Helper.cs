using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArchitectIndexer
{
	public static class WebView2Helper
	{
		public static readonly DependencyProperty HtmlProperty =
			DependencyProperty.RegisterAttached(
				"Html",
				typeof(string),
				typeof(WebView2Helper),
				new PropertyMetadata(null, OnHtmlChanged));

		public static string GetHtml(DependencyObject obj) => (string)obj.GetValue(HtmlProperty);
		public static void SetHtml(DependencyObject obj, string value) => obj.SetValue(HtmlProperty, value);

		private static async void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is WebView2 webView && e.NewValue is string html)
			{
				if (webView.CoreWebView2 == null)
				{
					await webView.EnsureCoreWebView2Async();
				}

				webView.NavigateToString(html);
			}
		}
	}
}
