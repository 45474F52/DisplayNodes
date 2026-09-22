namespace DisplayNodes.Playground.Compilation
{
	/// <summary>
	/// Абстракция над компилятором C#.
	/// </summary>
	internal interface ICompiler
	{
		/// <summary>
		/// Скомпилировать исходный код (уже обёрнутый в UserScript).
		/// </summary>
		CompileResult Compile(string wrappedCode, string[] referencedAssemblies);
	}
}
