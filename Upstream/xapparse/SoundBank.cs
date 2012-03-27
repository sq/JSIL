using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}: {m_cues.Count} Cues" )]
	public class SoundBank : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Exclude Cue Names":
					m_excludeCueNames = Parser.ParseInt( value, line );
					break;

				case "File":	// Deprecated
					break;

				case "Xbox File":
					m_xboxFileName = value;
					break;

				case "Windows File":
					m_windowsFileName = value;
					break;

				case "Xbox Bank Path Edited":
					m_xboxBankPathEdited = Parser.ParseInt( value, line );
					break;

				case "Windows Bank Path Edited":
					m_windowsBankPathEdited = Parser.ParseInt( value, line );
					break;

				case "Bank Last Modified High":
					m_bankLastModifiedHigh = Parser.ParseUint( value, line );
					break;

				case "Bank Last Modified Low":
					m_bankLastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "Header Last Modified High":
					m_headerLastModifiedHigh = Parser.ParseUint( value, line );
					break;

				case "Header Last Modified Low":
					m_headerLastModifiedLow = Parser.ParseUint( value, line );
					break;

				case "Header File":
					m_headerFile = value;
					break;

				case "Clip":
					Clip clip = new Clip();
					clip.Parse( source, ref line, OwnerProject );
					m_clips.Add( clip );
					break;

				case "Sound":
					Sound sound = new Sound();
					sound.Parse( source, ref line, OwnerProject );
					m_sounds.Add( sound );
					break;

				case "Cue":
					Cue cue = new Cue();
					cue.m_ownerSoundBank = this;
					cue.Parse( source, ref line, OwnerProject );
					m_cues.Add( cue );
					break;

				case "Comment":
					m_comment = Parser.ParseComment( value, source, ref line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public string m_name;
		public string m_comment;
		public string m_xboxFileName;
		public string m_windowsFileName;
		public int m_excludeCueNames;
		public string m_headerFile;
		internal uint m_bankLastModifiedLow;
		internal uint m_bankLastModifiedHigh;
		internal uint m_headerLastModifiedLow;
		internal uint m_headerLastModifiedHigh;
		public int m_xboxBankPathEdited;
		public int m_windowsBankPathEdited;

		public List<Clip> m_clips = new List<Clip>();
		public List<Sound> m_sounds = new List<Sound>();
		public List<Cue> m_cues = new List<Cue>();

		#endregion
	}

	[Serializable]
	public class Clip : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			throw new NotImplementedException( "The method or operation is not implemented." );
		}
	}

	[Serializable]
	public class ClipEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			throw new NotImplementedException( "The method or operation is not implemented." );
		}
	}
}
