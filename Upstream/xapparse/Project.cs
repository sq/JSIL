using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_waveBanks.Count} WaveBanks, {m_soundBanks.Count} SoundBanks")]
	public class Project
	{
		// Xap.Project does NOT inherit from Xap.Entity, as it lacks the curly braces,
		// but is otherwise very similar (and everything it contains is an Entity)

		/// <summary>
		/// Parses in a XAP file.
		/// </summary>
		/// <param name="source">A collection of the strings in the XAP file, for example
		/// as received from System.IO.File.ReadAllLines().</param>
		public void Parse( string[] source ) 
		{
			int line = 0;
			string property;
			string value;

			while ( line < source.Length )
			{
				if ( Parser.TokeniseLine( source[line], out property, out value ) )
				{
					if ( !SetProperty( property, value, source, ref line ) )
					{
						throw new InvalidContentException( "Line " + line.ToString() + " is unexpected ('" + property + "', '" + value + "')" );
					}
				}

				++line;
			}
		}

		public bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			if ( Parser.TokeniseLine( source[line], out propertyName, out value ) )
			{
				switch ( propertyName )
				{
					case "Signature":
						m_signature = value;
						break;

					case "Version":
						m_version = Parser.ParseInt( value, line );
						break;

					case "Content Version":
						m_contentVersion = Parser.ParseInt( value, line );
						break;

					case "Release":
						m_release = value;
						break;

					case "Options":
						m_options = new Options();
						m_options.Parse( source, ref line, this );
						break;

					case "Global Settings":
						m_globalSettings = new GlobalSettings();
						m_globalSettings.Parse( source, ref line, this );
						break;

					case "Wave Bank":
						WaveBank waveBank = new WaveBank();
						waveBank.Parse( source, ref line, this );
						m_waveBanks.Add( waveBank );
						break;

					case "Sound Bank":
						SoundBank soundBank = new SoundBank();
						soundBank.Parse( source, ref line, this );
						m_soundBanks.Add( soundBank );
						break;

					case "Workspace":
						Workspace workspace = new Workspace();
						workspace.Parse( source, ref line, this );
						m_workspaces.Add( workspace );
						break;

					default:
						return false;
				}
			}

			return true;
		}

		#region Member variables

		public string m_sourceXactFileName;	// Full path including .xap

		// TODO could later use this information to change how we parse projects, as they change over time
		public string m_signature;			// eg. "XACT2"
		public int m_version;				// eg. 16
		public int m_contentVersion;		// eg. 43
		public string m_release;			// eg. "August 2007"

		public Options m_options = new Options();
		public GlobalSettings m_globalSettings = new GlobalSettings();
		public List<WaveBank> m_waveBanks = new List<WaveBank>();
		public List<SoundBank> m_soundBanks = new List<SoundBank>();
		public List<Workspace> m_workspaces = new List<Workspace>();
		
		#endregion
	}
}
