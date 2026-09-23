///////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 AES
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
///////////////////////////////////////////////////////////////////////////

namespace DisplayNodes.Core
{
	/// <summary>
	/// Выравнивание виджета внутри слота
	/// </summary>
	public enum Alignment
	{
		/// <summary>
		/// Прижать к началу слота
		/// </summary>
		Start,

		/// <summary>
		/// Выровнять по центру
		/// </summary>
		Center,

		/// <summary>
		/// Прижать к концу слота
		/// </summary>
		End,

		/// <summary>
		/// Растянуть
		/// </summary>
		Stretch,
	}
}
