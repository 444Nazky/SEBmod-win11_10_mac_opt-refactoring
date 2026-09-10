/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.SystemComponents.Contracts
{
	/// <summary>
	/// Provides access to file system operations.
	/// </summary>
	public interface IFileSystem
	{
		/// <summary>
		/// Creates all directories and subdirectories defined by the given path.
		/// </summary>
		void CreateDirectory(string path);

		/// <summary>
		/// Deletes the item at the given path, if it exists. Directories will be completely deleted, including all subdirectories and files.
		/// </summary>
		void Delete(string path);

		/// <summary>
		/// Indicates whether the item at the given path exists, be it a file or a directory.
		/// </summary>
		bool Exists(string path);

		/// <summary>
		/// Attempts to retrieve the content of the file at the given path. Returns <c>true</c> if successful or <c>false</c> if the file does not exist.
		/// </summary>
		bool TryRead(string path, out string content);

		/// <summary>
		/// Saves the given content as a file under the specified path. If the file doesn't yet exist, it will be created, otherwise overwritten.
		/// </summary>
		void Save(string content, string path);
	}
}
