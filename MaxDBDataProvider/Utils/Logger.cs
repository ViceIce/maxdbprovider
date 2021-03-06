//	Copyright (C) 2005-2006 Dmitry S. Kataev
//
//	This program is free software; you can redistribute it and/or
//	modify it under the terms of the GNU General Public License
//	as published by the Free Software Foundation; either version 2
//	of the License, or (at your option) any later version.
//
//	This program is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//	GNU General Public License for more details.
//
//	You should have received a copy of the GNU General Public License
//	along with this program; if not, write to the Free Software
//	Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

using System;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using MaxDB.Data.MaxDBProtocol;
using System.Globalization;

namespace MaxDB.Data.Utilities
{
	internal enum MaxDBTraceLevel
	{
		None = 0,
		SqlOnly = 1,
		Full = 2
	}

	internal class MaxDBTraceSwitch : Switch
	{
		public MaxDBTraceSwitch(string displayName, string description)
			: base(displayName, description)
		{
		}

		public MaxDBTraceLevel Level
		{
			get
			{
				switch (SwitchSetting)
				{
					case (int)MaxDBTraceLevel.None:
						return MaxDBTraceLevel.None;
					case (int)MaxDBTraceLevel.SqlOnly:
						return MaxDBTraceLevel.SqlOnly;
					case (int)MaxDBTraceLevel.Full:
						return MaxDBTraceLevel.Full;
					default:
						return MaxDBTraceLevel.None;
				}
			}
		}

		public bool TraceSQL
		{
			get
			{
				return (Level != MaxDBTraceLevel.None);
			}
		}

		public bool TraceFull
		{
			get
			{
				return (Level == MaxDBTraceLevel.Full);
			}
		}
	}

	internal class MaxDBLogger
	{
		public const int
			NumSize = 4,
			TypeSize = 16,
			LenSize = 10,
			InputSize = 10,
			DataSize = 256;

		public const string Null = "NULL";

		private MaxDBTraceSwitch mSwitcher = new MaxDBTraceSwitch("TraceLevel", "Trace Level");

		public MaxDBLogger()
		{
		}

		public bool TraceSQL
		{
			get
			{
				return mSwitcher.TraceSQL;
			}
		}

		public bool TraceFull
		{
			get
			{
				return mSwitcher.TraceFull;
			}
		}

		public void SqlTrace(DateTime dt, string msg)
		{
            if (mSwitcher.TraceSQL)
            {
                Trace.WriteLine(dt.ToString(Consts.TimeStampFormat, CultureInfo.InvariantCulture) + " " + msg);
            }
		}

		public void SqlTraceParseInfo(DateTime dt, object objInfo)
		{
			MaxDBParseInfo parseInfo = (MaxDBParseInfo)objInfo;
			if (mSwitcher.TraceSQL)
			{
				if (parseInfo.ParamInfo != null && parseInfo.ParamInfo.Length > 0)
				{
					SqlTrace(dt, "PARAMETERS:");
					SqlTrace(dt, "I   T              L    P   IO    N");
					foreach (DBTechTranslator info in parseInfo.ParamInfo)
					{
						Trace.Write(dt.ToString(Consts.TimeStampFormat, CultureInfo.InvariantCulture) + " ");

						SqlTraceTransl(info);

						if (FunctionCode.IsQuery(parseInfo.FuncCode))
						{
							if (!info.IsOutput)
							{
								if (info.IsInput)
								{
									if (info.IsOutput)
										Trace.Write(" INOUT ");// ... two in one. We must reduce the overall number !!!
									else
										Trace.Write(" IN    ");
								}
								else
									Trace.Write(" OUT   ");
							}
						}
						else
						{
							if (info.IsInput)
							{
								if (info.IsOutput)
									Trace.Write(" INOUT ");// ... two in one. We must reduce the overall number !!!
								else
									Trace.Write(" IN    ");
							}
							else
								Trace.Write(" OUT   ");
						}

						Trace.WriteLine(info.ColumnName);
					}
				}

				if (parseInfo.ColumnInfo != null && parseInfo.ColumnInfo.Length > 0)
				{
					SqlTrace(dt, "COLUMNS:");
					SqlTrace(dt, "I   T              L           P           N");
					foreach (DBTechTranslator info in parseInfo.ColumnInfo)
					{
						Trace.Write(dt.ToString(Consts.TimeStampFormat, CultureInfo.InvariantCulture) + " ");
						SqlTraceTransl(info);
						Trace.WriteLine(info.ColumnName);
					}
				}
			}
		}

		public void SqlTraceDataHeader(DateTime dt)
		{
			SqlTrace(dt, "I".PadRight(NumSize) + "T".PadRight(TypeSize) + "L".PadRight(LenSize) + "I".PadRight(InputSize) + "DATA");
		}

		private static void SqlTraceTransl(DBTechTranslator info)
		{
			Trace.Write(info.ColumnIndex.ToString(CultureInfo.InvariantCulture).PadRight(4));
			Trace.Write(info.ColumnTypeName.PadRight(15));
			Trace.Write((info.PhysicalLength - 1).ToString(CultureInfo.InvariantCulture).PadRight(12));
			Trace.Write(info.Precision.ToString(CultureInfo.InvariantCulture).PadRight(12));
		}

		public void Flush()
		{
			if (mSwitcher.TraceSQL)
			{
				Trace.Flush();
			}
		}
	}
}
