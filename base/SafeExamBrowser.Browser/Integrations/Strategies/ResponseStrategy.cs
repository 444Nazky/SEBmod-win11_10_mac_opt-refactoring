/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using CefSharp;

namespace SafeExamBrowser.Browser.Integrations.Strategies
{
	/// <summary>
	/// Attempts to search a user identifier from a web response.
	/// </summary>
	internal delegate bool ResponseStrategy(IRequest request, IResponse response, out string userIdentifier);
}
