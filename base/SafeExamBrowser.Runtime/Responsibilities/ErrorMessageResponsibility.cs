/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;
using SafeExamBrowser.UserInterface.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal class ErrorMessageResponsibility : RuntimeResponsibility, IParameterizedResponsibility<RuntimeTask>
	{
		private readonly AppConfig appConfig;
		private readonly IMailClient mailClient;
		private readonly IRuntimeWindow runtimeWindow;
		private readonly ISplashScreen splashScreen;
		private readonly IText text;
		private readonly IUserInterfaceFactory uiFactory;

		internal ErrorMessageResponsibility(
			AppConfig appConfig,
			ILogger logger,
			IMailClient mailClient,
			RuntimeContext runtimeContext,
			IRuntimeWindow runtimeWindow,
			ISplashScreen splashScreen,
			IText text,
			IUserInterfaceFactory uiFactory) : base(logger, runtimeContext)
		{
			this.appConfig = appConfig;
			this.mailClient = mailClient;
			this.runtimeWindow = runtimeWindow;
			this.splashScreen = splashScreen;
			this.text = text;
			this.uiFactory = uiFactory;
		}

		public override void Assume(RuntimeTask task)
		{
			if (task == RuntimeTask.ShowSessionStartError || task == RuntimeTask.ShowShutdownError || task == RuntimeTask.ShowStartupError)
			{
				ShowErrorDialog(task);
			}
		}

		public bool TryAssume<TParam, TResult>(RuntimeTask task, TParam parameter, out TResult result) where TResult : class
		{
			result = default;

			if (task == RuntimeTask.ShowCrashMessage && parameter is string[] logFiles)
			{
				result = ShowErrorDialog(task, logFiles) as TResult;
			}

			return result != default;
		}

		private ErrorDialogResult ShowErrorDialog(RuntimeTask task, string[] logFiles = default)
		{
			var (message, parent, showIgnoreCheckbox, title, type) = InitializeParameters(task);
			logFiles = InitializeLogFiles(logFiles);

			var body = $"<b>Application Information:</b>{appConfig.ProgramTitle}, Version {appConfig.ProgramInformationalVersion}, Build {appConfig.ProgramBuildVersion}"
					   + $"<br /><b>Application Error Type:</b>{type}";
			var subject = $"{appConfig.ProgramTitle} Log Files";
			var sendLogFiles = new Action(() => mailClient.OpenDefault(subject, body, logFiles));

			var dialog = uiFactory.CreateErrorDialog(message, title, sendLogFiles, showIgnoreCheckbox, logFiles);
			var result = dialog.Show(parent);

			return result;
		}

		private IEnumerable<string> CollectLogFiles()
		{
			if (File.Exists(appConfig.BrowserLogFilePath))
			{
				yield return appConfig.BrowserLogFilePath;
			}

			if (File.Exists(appConfig.ClientLogFilePath))
			{
				yield return appConfig.ClientLogFilePath;
			}

			if (File.Exists(appConfig.RuntimeLogFilePath))
			{
				yield return appConfig.RuntimeLogFilePath;
			}

			if (File.Exists(appConfig.ServiceLogFilePath))
			{
				yield return appConfig.ServiceLogFilePath;
			}
		}

		private string[] InitializeLogFiles(string[] logFiles)
		{
			if (logFiles == default)
			{
				logFiles = CollectLogFiles().ToArray();
			}
			else
			{
				logFiles = logFiles.Where(f => File.Exists(f)).ToArray();
			}

			return logFiles;
		}

		private (TextKey message, IWindow parent, bool showIgnoreCheckbox, TextKey title, string type) InitializeParameters(RuntimeTask task)
		{
			var message = default(TextKey);
			var parent = default(IWindow);
			var showIgnoreCheckbox = false;
			var title = default(TextKey);
			var type = default(string);

			switch (task)
			{
				case RuntimeTask.ShowCrashMessage:
					message = TextKey.ErrorDialog_CrashMessage;
					parent = splashScreen;
					showIgnoreCheckbox = true;
					title = TextKey.ErrorDialog_CrashTitle;
					type = "Crash";
					break;
				case RuntimeTask.ShowSessionStartError:
					message = TextKey.ErrorDialog_SessionStartMessage;
					parent = runtimeWindow;
					title = TextKey.ErrorDialog_SessionStartTitle;
					type = "Session Start Error";
					break;
				case RuntimeTask.ShowShutdownError:
					message = TextKey.ErrorDialog_ShutdownMessage;
					parent = splashScreen;
					title = TextKey.ErrorDialog_ShutdownTitle;
					type = "Shutdown Error";
					break;
				case RuntimeTask.ShowStartupError:
					message = TextKey.ErrorDialog_StartupMessage;
					parent = splashScreen;
					title = TextKey.ErrorDialog_StartupTitle;
					type = "Startup Error";
					break;
			}

			return (message, parent, showIgnoreCheckbox, title, type);
		}
	}
}
