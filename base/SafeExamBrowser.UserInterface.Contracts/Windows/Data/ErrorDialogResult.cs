/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.UserInterface.Contracts.Windows.Data
{
	/// <summary>
	/// Defines the user interaction result of an <see cref="IErrorDialog"/>.
	/// </summary>
	public class ErrorDialogResult
	{
		/// <summary>
		/// Indicates that the user would like to clear a crash message.
		/// </summary>
		public bool ClearCrash { get; set; }
	}
}
