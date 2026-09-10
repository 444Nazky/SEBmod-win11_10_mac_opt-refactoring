/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using SafeExamBrowser.Configuration.Contracts;
using SafeExamBrowser.Core.Contracts.ResponsibilityModel;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Runtime.Responsibilities
{
	internal abstract class RuntimeResponsibility : IResponsibility<RuntimeTask>
	{
		protected RuntimeContext Context { get; private set; }
		protected ILogger Logger { get; private set; }

		protected SessionConfiguration Session => Context.Current;
		protected bool SessionIsRunning => Session != default;

		internal RuntimeResponsibility(ILogger logger, RuntimeContext runtimeContext)
		{
			Logger = logger;
			Context = runtimeContext;
		}

		public abstract void Assume(RuntimeTask task);
	}
}
