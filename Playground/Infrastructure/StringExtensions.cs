namespace DisplayNodes.Playground.Infrastructure
{
	internal static class StringExtensions
	{
		public static bool IsNullOrWhiteSpace(this string value)
		{
			if (value == null)
				return true;

			for (int i = 0; i < value.Length; i++)
			{
				if (!char.IsWhiteSpace(value[i]))
					return false;
			}

			return true;
		}
	}
}
