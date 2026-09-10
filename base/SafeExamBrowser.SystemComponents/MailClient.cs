/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MimeKit;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.SystemComponents
{
	public class MailClient : IMailClient
	{
		private readonly AppConfig appConfig;
		private readonly ILogger logger;

		public MailClient(AppConfig appConfig, ILogger logger)
		{
			this.appConfig = appConfig;
			this.logger = logger;
		}

		public void OpenDefault(string subject, string body, params string[] attachements)
		{
			try
			{
				var fileName = $"{DateTime.Now:yyyy-MM-dd_hh\\hmm\\mss\\s}.eml";
				var path = Path.Combine(appConfig.TemporaryDirectory, fileName);

				SaveMessage(subject, body, path, attachements);
				StartDefaultClient(path);
				DeleteWithDelay(path);
			}
			catch (Exception e)
			{
				logger.Error("Unexpected error while trying to open e-mail with default mail client!", e);
			}
		}

		private void DeleteWithDelay(string path)
		{
			Task.Delay(1000).ContinueWith(_ =>
			{
				File.Delete(path);
				logger.Info($"Deleted temporary e-mail message '{path}'.");
			});
		}

		private void SaveMessage(string subject, string body, string path, string[] attachements)
		{
			using (var message = new MimeMessage())
			{
				var builder = new BodyBuilder() { HtmlBody = body };
				var sender = new MailboxAddress($"{appConfig.ProgramTitle} for Windows", "no-reply@safeexambrowser.org");

				foreach (var attachement in attachements)
				{
					builder.Attachments.Add(attachement);
				}

				message.Body = builder.ToMessageBody();
				message.From.Add(sender);
				message.Subject = subject;

				using (var stream = new FileStream(path, FileMode.Create))
				{
					message.WriteTo(stream);
				}
			}

			logger.Info($"Successfully saved temporary e-mail message as '{path}'.");
		}

		private void StartDefaultClient(string path)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = path,
				UseShellExecute = true
			});

			logger.Info("Successfully started default e-mail client.");
		}
	}
}
