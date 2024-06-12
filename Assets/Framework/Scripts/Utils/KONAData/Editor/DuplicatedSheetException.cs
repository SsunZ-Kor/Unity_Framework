using System;
using System.Linq;
using System.Collections.Generic;

namespace KONA.Data.Converter
{
	public class DuplicatedSheetException : Exception
	{
		private string[] duplicatedSheetNames = null;

		public DuplicatedSheetException(params string[] sheetNames)
		{
			duplicatedSheetNames = sheetNames;
		}

		public DuplicatedSheetException(IEnumerable<string> sheetNames)
		{
			duplicatedSheetNames = sheetNames.ToArray();
		}

		public override string Message
		{
			get
			{
				return $"Duplicated Sheets: {string.Join(",", duplicatedSheetNames)}";
			}
		}
	}
}