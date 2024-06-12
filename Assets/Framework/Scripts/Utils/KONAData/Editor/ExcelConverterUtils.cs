using System.Text.RegularExpressions;

namespace KONA.Data.Converter
{
	public static class ExcelConverterUtil
	{
		public const string ExcelFileNameMatchExpression = @"^((?!(~\$)).*\.(xlsx|xls$))$";

		public static Regex ExcelFileNameRegex
		{
			get => new Regex(ExcelFileNameMatchExpression);
		}
	}
}