/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.OperationModel;
using SafeExamBrowser.Core.Contracts.OperationModel.Events;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.Runtime.Responsibilities;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Runtime.Operations.Bootstrap
{
	internal class CrashLogOperation : IOperation
	{
		private readonly AppConfig appConfig;
		private readonly ICrashLog crashLog;
		private readonly ILogger logger;
		private readonly RuntimeContext runtimeContext;

		private string[] previous;

		public event StatusChangedEventHandler StatusChanged;

		internal CrashLogOperation(AppConfig appConfig, ICrashLog crashLog, ILogger logger, RuntimeContext runtimeContext)
		{
			this.appConfig = appConfig;
			this.crashLog = crashLog;
			this.logger = logger;
			this.runtimeContext = runtimeContext;
		}

		public OperationResult Perform()
		{
			logger.Info($"Initializing crash log...");
			StatusChanged?.Invoke(TextKey.OperationStatus_InitializeCrashLog);

			if (crashLog.HasData)
			{
				var data = crashLog.Get();
				var result = runtimeContext.Responsibilities.Delegate<string[], ErrorDialogResult>(RuntimeTask.ShowCrashMessage, data);

				if (result.ClearCrash)
				{
					crashLog.Clear();
				}
				else
				{
					previous = data;
				}
			}

			crashLog.Set(appConfig.BrowserLogFilePath, appConfig.ClientLogFilePath, appConfig.RuntimeLogFilePath, appConfig.ServiceLogFilePath);

			return OperationResult.Success;
		}

		public OperationResult Revert()
		{
			logger.Info($"Finalizing crash log...");
			StatusChanged?.Invoke(TextKey.OperationStatus_FinalizeCrashLog);

			crashLog.Clear();

			if (previous != default)
			{
				crashLog.Set(previous);
			}

			return OperationResult.Success;
		}
	}
}
