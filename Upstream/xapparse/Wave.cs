using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class Wave : Entity
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

				case "File":
					m_fileName = value;
					break;

				case "Remove Loop Tail":
					m_removeLoopTail = Parser.ParseInt( value, line );
					break;

				case "Ignore Loop Region":
					m_ignoreLoopRegion = Parser.ParseInt( value, line );
					break;

				case "Build Settings Last Modified Low":
					m_buildSettingsLastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "Build Settings Last Modified High":
					m_buildSettingsLastModifiedHigh = Parser.ParseUint( value, line );
					break;

				case "Cache":
					m_cache = new WaveCache();
					m_cache.Parse( source, ref line, OwnerProject );
					break;

				case "Compression Preset Name":
					m_compressionPresetName = value;
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables
		
		public string m_name;
		public string m_fileName;
		public string m_comment;
		public int m_removeLoopTail;
		public int m_ignoreLoopRegion;
		internal uint m_buildSettingsLastModifiedLow;
		internal uint m_buildSettingsLastModifiedHigh;
		public string m_compressionPresetName;

		public WaveCache m_cache;

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "Bank {m_bankName}, Wave {m_entryName}" )]
	public class WaveEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Bank Name":
					m_bankName = value;
					break;

				case "Bank Index":
					m_bankIndex = Parser.ParseInt( value, line );
					break;

				case "Entry Name":
					m_entryName = value;
					break;

				case "Entry Index":
					m_entryIndex = Parser.ParseInt( value, line );
					break;

				case "Weight":
					m_weight = Parser.ParseInt( value, line );
					break;

				case "Weight Min":
					m_weightMin = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		public Wave FindWave()
		{
			foreach ( WaveBank waveBank in OwnerProject.m_waveBanks )
			{
				if ( waveBank.m_name == m_bankName )
				{
					foreach ( Wave wave in waveBank.m_waves )
					{
						if ( wave.m_name == m_entryName )
						{
							return wave;
						}
					}
				}
			}

			// Not found by name, which suggests index is likely to fail too (so why
			// the XAP file format insists on providing both, I don't understand)
			if ( m_bankIndex >= 0 && m_entryIndex >= 0 )
			{
				return OwnerProject.m_waveBanks[m_bankIndex].m_waves[m_entryIndex];
			}

			return null;
		}		

		#region Member variables

		public int m_weight;
		public int m_weightMin;

		public string m_bankName;
		public int m_bankIndex;
		public string m_entryName;
		public int m_entryIndex;

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class WaveCache : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "File":
					m_file = value;
					break;

				case "Format Tag":
					m_formatTag = Parser.ParseInt( value, line );
					break;

				case "Channels":
					m_channels = Parser.ParseInt( value, line );
					break;

				case "Sampling Rate":
					m_sampleRate = Parser.ParseInt( value, line );
					break;

				case "Bits Per Sample":
					m_bitsPerSample = Parser.ParseInt( value, line );
					break;

				case "Play Region Offset":
					m_playRegionOffset = Parser.ParseInt( value, line );
					break;

				case "Play Region Length":
					m_playRegionLength = Parser.ParseInt( value, line );
					break;

				case "Loop Region Offset":
					m_loopRegionOffset = Parser.ParseInt( value, line );
					break;

				case "Loop Region Length":
					m_loopRegionLength = Parser.ParseInt( value, line );
					break;

				case "File Type":
					m_fileType = Parser.ParseInt( value, line );
					break;

				case "Last Modified Low":
					m_lastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "Last Modified High":
					m_lastModifiedHigh = Parser.ParseUint( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public string m_name;
		public string m_file;
		public int m_formatTag;
		public int m_channels;
		public int m_sampleRate;
		public int m_bitsPerSample;
		public int m_playRegionOffset;
		public int m_playRegionLength;
		public int m_loopRegionOffset;
		public int m_loopRegionLength;
		public int m_fileType;
		internal uint m_lastModifiedLow;
		internal uint m_lastModifiedHigh;

		#endregion
	}
}
 