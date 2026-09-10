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
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.SystemComponents.Contracts;

namespace SafeExamBrowser.Logging
{
	public class CrashLog : ICrashLog
	{
		private readonly IFileSystem fileSystem;
		private readonly string path;
		private readonly ILogger logger;

		public bool HasData => fileSystem.Exists(path);

		public CrashLog(AppConfig appConfig, IFileSystem fileSystem, ILogger logger)
		{
			this.fileSystem = fileSystem;
			this.logger = logger;
			this.path = Path.Combine(appConfig.TemporaryDirectory, "CrashLog.txt");
		}

		public void Clear()
		{
			fileSystem.Delete(path);
			logger.Info("Cleared crash log.");
		}

		public string[] Get()
		{
			var data = new string[0];

			if (fileSystem.TryRead(path, out var raw))
			{
				data = Sanitize(raw).ToArray();
				logger.Info($"Retrieved crash log with {data.Length} items.");
			}

			return data;
		}

		public void Set(params string[] data)
		{
			var content = string.Join(Environment.NewLine, data);

			fileSystem.Save(content, path);
			logger.Info($"Saved crash log with {data.Length} items.");
		}

		private IEnumerable<string> Sanitize(string raw)
		{
			var lines = raw?.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

			foreach (var line in lines)
			{
				if (line != default && line.EndsWith(".log"))
				{
					yield return line;
				}
			}
		}
	}
}
