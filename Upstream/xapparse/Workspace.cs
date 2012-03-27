using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay("{m_name}")]
	public class Workspace : Entity		// Not documented
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

				case "Category Entry":
					CategoryEntry category = new CategoryEntry();
					category.Parse( source, ref line, OwnerProject );
					break;

				case "Sound Entry":
					WorkspaceSoundEntry sound = new WorkspaceSoundEntry();
					sound.Parse( source, ref line, OwnerProject );
					break;

				default:
					return false;
			}

			return true;
		}

		public string m_name;
		public string m_comment;

		public List<CategoryEntry> m_categoryEntries = new List<CategoryEntry>();
		public List<WorkspaceSoundEntry> m_soundEntries = new List<WorkspaceSoundEntry>();

	}

	[Serializable]
	public class WorkspaceSoundEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Bank Name":
					m_bankName = value;
					break;

				case "Bank Index":
					m_bankIndex = Parser.ParseInt( value, line ); ;
					break;

				case "Sound Name":
					m_bankName = value;
					break;

				case "Sound Index":
					m_soundIndex = Parser.ParseInt( value, line ); ;
					break;

				default:
					return false;
			}

			return true;
		}

		public Sound FindSound()
		{
			foreach ( SoundBank soundBank in OwnerProject.m_soundBanks )
			{
				if ( soundBank.m_name == m_bankName )
				{
					foreach ( Sound sound in soundBank.m_sounds )
					{
						if ( sound.m_name == m_soundName )
						{
							return sound;
						}
					}
				}
			}

			// Not found by name, which suggests index is likely to fail too (so why
			// the XAP file format insists on providing both, I don't understand)
			if ( m_bankIndex >= 0 && m_soundIndex >= 0 )
			{
				return OwnerProject.m_soundBanks[m_bankIndex].m_sounds[m_soundIndex];
			}

			return null;
		}

		#region Member variables

		public string m_bankName;
		public int m_bankIndex = -1;
		public string m_soundName;
		public int m_soundIndex = -1;

		#endregion
	}
}
