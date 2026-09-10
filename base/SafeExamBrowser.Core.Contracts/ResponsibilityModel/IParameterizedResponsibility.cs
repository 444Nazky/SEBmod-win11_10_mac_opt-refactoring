/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

namespace SafeExamBrowser.Core.Contracts.ResponsibilityModel
{
	/// <summary>
	/// Defines a parameterized responsibility additionally capable of receiving a parameter and providing a return value when assuming a task.
	/// </summary>
	public interface IParameterizedResponsibility<T> : IResponsibility<T>
	{
		/// <summary>
		/// Attempts to assume the given task with the specified parameter and the requested return value type. Returns <c>true</c> if successfull,
		/// otherwise <c>false</c>.
		/// </summary>
		bool TryAssume<TParam, TResult>(T task, TParam parameter, out TResult result) where TResult : class;
	}
}
