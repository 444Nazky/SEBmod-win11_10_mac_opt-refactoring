/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Logging.Contracts
{
	/// <summary>
	/// The crash log allows to store and retrieve data about application crashes.
	/// </summary>
	public interface ICrashLog
	{
		/// <summary>
		/// Indicates whether the is any crash data (i.e. whether a crash happened previously).
		/// </summary>
		bool HasData { get; }

		/// <summary>
		/// Clears all stored crash data.
		/// </summary>
		void Clear();

		/// <summary>
		/// Gets all stored crash data.
		/// </summary>
		string[] Get();

		/// <summary>
		/// Stores the given crash data persistently.
		/// </summary>
		void Set(params string[] data);
	}
}
