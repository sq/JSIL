using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}, quality {m_quality}" )]
	public class CompressionPreset : Entity
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

				case "Xbox Format Tag":
					m_xboxFormatTag = Parser.ParseInt( value, line );
					break;

				case "Target Sample Rate":
					m_sampleRate = Parser.ParseInt( value, line );
					break;

				case "Quality":
					m_quality = Parser.ParseInt( value, line );
					break;

				case "Find Best Quality":
					m_findBestQuality = Parser.ParseInt( value, line );
					break;

				case "High Freq Cut":
					m_highFreqCut = Parser.ParseInt( value, line );
					break;

				case "Loop":
					m_loop = Parser.ParseInt( value, line );
					break;

				case "PC Format Tag":
					m_pcFormatTag = Parser.ParseInt( value, line );
					break;

				case "Samples Per Block":
					m_samplesPerBlock = Parser.ParseInt( value, line );
					break;

                case "XMA Quality":
                    m_xmaQuality = Parser.ParseInt(value, line);
                    break;

                case "WMA Quality":
                    m_wmaQuality = Parser.ParseInt(value, line);
                    break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public string m_name;
		public string m_comment;
		public int m_xboxFormatTag;
		public int m_pcFormatTag;
		public int m_sampleRate;
		public int m_quality;
		public int m_findBestQuality;
		public int m_highFreqCut;
		public int m_loop;
		public int m_samplesPerBlock;
        public int m_xmaQuality;
        public int m_wmaQuality;

		#endregion
	}
}
