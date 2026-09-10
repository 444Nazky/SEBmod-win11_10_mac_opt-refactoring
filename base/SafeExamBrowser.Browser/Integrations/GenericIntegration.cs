/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Linq;
using CefSharp;
using SafeExamBrowser.Browser.Integrations.Strategies;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Browser.Integrations
{
	internal class GenericIntegration : Integration
	{
		private readonly ILogger logger;

		internal GenericIntegration(ILogger logger)
		{
			this.logger = logger;
		}

		protected override IEnumerable<ResponseStrategy> ResponseStrategies => new ResponseStrategy[]
		{
			TrySearchByCookieHeader,
			TrySearchByCustomHeader
		};

		private bool TrySearchByCookieHeader(IRequest request, IResponse response, out string userIdentifier)
		{
			var cookies = response.Headers.GetValues("Set-Cookie");
			var cookie = cookies?.FirstOrDefault(i => i.StartsWith("SEB-LMS-USER-ID"));
			var id = cookie?.Split('=', ';').ElementAtOrDefault(1);

			userIdentifier = default;

			if (HasChanged(id))
			{
				userIdentifier = id;
				logger.Info($"User identifier '{id}' detected by cookie header of response.");
			}

			return userIdentifier != default;
		}

		private bool TrySearchByCustomHeader(IRequest request, IResponse response, out string userIdentifier)
		{
			var ids = response.Headers.GetValues("X-LMS-USER-ID");
			var id = ids?.FirstOrDefault();

			userIdentifier = default;

			if (HasChanged(id))
			{
				userIdentifier = id;
				logger.Info($"User identifier '{id}' detected by custom header of response.");
			}

			return userIdentifier != default;
		}
	}
}
