namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Результат компиляции скрипта.
	/// </summary>
	public sealed class CompileResult
	{
		public bool Success { get; set; }

		/// <summary>
		/// Байты скомпилированной сборки (если Success == true).
		/// Загружаются через Assembly.Load(byte[]), чтобы не кэшировать
		/// сборку в AppDomain — иначе повторная компиляция не подхватит
		/// изменения в UserScript.
		/// </summary>
		public byte[] AssemblyBytes { get; set; }

		/// <summary>
		/// Отформатированное сообщение об ошибках (если Success == false).
		/// </summary>
		public string ErrorOutput { get; set; }
	}
}
