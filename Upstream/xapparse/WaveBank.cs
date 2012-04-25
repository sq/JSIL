using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}: {m_waves.Count} Waves" )]
	public class WaveBank : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Comment":
					m_comment = Parser.ParseComment( value, source, ref line );
					break;

				case "Xbox File":
					m_xboxFileName = value;
					break;

				case "File":	// Deprecated
					break;

				case "Windows File":
					m_windowsFileName = value;
					break;

				case "Seek Tables":
					m_seekTables = Parser.ParseInt( value, line );
					break;

				case "Header File":
					m_headerFile = value;
					break;

				case "Alignment":
					m_alignment = Parser.ParseInt( value, line );
					break;

				case "Streaming":
					m_isStreaming = Parser.ParseInt( value, line );
					break;

				case "Entry Names":
					m_entryNames = Parser.ParseInt( value, line );
					break;

				case "Compact":
					m_compact = Parser.ParseInt( value, line );
					break;

				case "Audition Sync Disabled":
					m_syncDisabled = Parser.ParseInt( value, line );
					break;

				case "Xbox Bank Path Edited":
					m_xboxBankPathEdited = Parser.ParseInt( value, line );
					break;

				case "Windows Bank Path Edited":
					m_windowsBankPathEdited = Parser.ParseInt( value, line );
					break;

				case "Xbox Bank Last Modified Low":
					m_xboxBankLastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "Xbox Bank Last Modified High":
					m_xboxBankLastModifiedHigh = Parser.ParseUint( value, line );
					break;

				case "PC Bank Last Modified Low":
					m_windowsBankLastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "PC Bank Last Modified High":
					m_windowsBankLastModifiedHigh = Parser.ParseUint( value, line );
					break;

				case "Header Last Modified Low":
					m_headerLastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "Header Last Modified High":
					m_headerLastModifiedHigh = Parser.ParseUint( value, line );
					break;

				case "Bank Last Revised Low":
					m_bankLastRevisedLow = Parser.ParseUint( value, line );
					break;

				case "Bank Last Revised High":
					m_bankLastRevisedHigh = Parser.ParseUint( value, line );
					break;

					// What would be wrong with a "Compression Preset Entry" then, like every similar type?
				case "Compression Preset Name":
					m_compressionPresetName = value;
					break;
				case "Compression Preset ID":
					m_compressionPresetIndex = Parser.ParseInt( value, line );
					break;

				case "Wave":
					Wave wave = new Wave();
					wave.Parse( source, ref line, OwnerProject );
					m_waves.Add( wave );
					break;

				default:
					return false;
			}

			return true;
		}
		
		#region Member variables

		public string m_name;
		public string m_xboxFileName;
		public string m_windowsFileName;
		public int m_xboxBankPathEdited;
		public int m_windowsBankPathEdited;
		public int m_seekTables;
		public string m_headerFile;
		public int m_alignment;
		public int m_isStreaming;
		public int m_entryNames;
		public int m_compact;
		public int m_syncDisabled;
		internal uint m_xboxBankLastModifiedLow;
		internal uint m_xboxBankLastModifiedHigh;
		internal uint m_windowsBankLastModifiedLow;
		internal uint m_windowsBankLastModifiedHigh;
		internal uint m_headerLastModifiedLow;
		internal uint m_headerLastModifiedHigh;
		internal uint m_bankLastRevisedLow;
		internal uint m_bankLastRevisedHigh;
		public string m_comment;

		public string m_compressionPresetName;
		public int m_compressionPresetIndex;

		public List<Wave> m_waves = new List<Wave>();

		#endregion
	}
}
