/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.UserInterface.Desktop.Windows
{
	public partial class ErrorDialog : Window, IErrorDialog
	{
		private readonly Action sendMailCallback;
		private readonly IText text;

		private bool clearCrash;

		internal ErrorDialog(TextKey message, TextKey title, Action sendMailCallback, bool showIgnoreCheckbox, IText text, params string[] logFiles)
		{
			this.sendMailCallback = sendMailCallback;
			this.text = text;

			InitializeComponent();
			InitializeErrorDialog(message, title, showIgnoreCheckbox, logFiles);
		}

		public ErrorDialogResult Show(IWindow parent = null)
		{
			return Dispatcher.Invoke(() =>
			{
				var result = new ErrorDialogResult { ClearCrash = false };

				if (parent is Window window && window.Dispatcher.CheckAccess())
				{
					Owner = window;
					WindowStartupLocation = WindowStartupLocation.CenterOwner;
				}

				if (ShowDialog() == true)
				{
					result.ClearCrash = clearCrash;
				}

				return result;
			});
		}

		private void InitializeErrorDialog(TextKey message, TextKey title, bool showIgnoreCheckbox, params string[] logFiles)
		{
			Loaded += (o, args) => Activate();
			Message.Text = text.Get(message);
			Title = text.Get(title);
			WindowStartupLocation = WindowStartupLocation.CenterScreen;

			IgnoreCheckbox.Content = text.Get(TextKey.ErrorDialog_IgnoreCrash);
			IgnoreCheckbox.Click += (o, args) => clearCrash = IgnoreCheckbox.IsChecked == true;
			IgnoreCheckbox.Visibility = showIgnoreCheckbox ? Visibility.Visible : Visibility.Collapsed;

			OkButton.Content = text.Get(TextKey.ErrorDialog_Ok);
			OkButton.Click += (o, args) => Close();
			OkButton.IsDefault = true;

			SendMailButton.Content = text.Get(TextKey.ErrorDialog_SendMail);
			SendMailButton.Click += SendMailButton_Click;

			foreach (var file in logFiles)
			{
				var link = new Hyperlink(new Run(file));
				var textBlock = new TextBlock(link);

				link.Click += (o, args) => Process.Start(file);
				textBlock.Margin = new Thickness(0, 5, 0, 0);
				textBlock.TextWrapping = TextWrapping.WrapWithOverflow;

				LogFiles.Children.Add(textBlock);
			}
		}

		private void SendMailButton_Click(object sender, RoutedEventArgs e)
		{
			sendMailCallback?.Invoke();
			clearCrash = true;
		}
	}
}
