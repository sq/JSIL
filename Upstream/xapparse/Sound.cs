using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class Sound : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Volume":
					m_volume = Parser.ParseInt( value, line );
					break;

				case "Pitch":
					m_pitch = Parser.ParseInt( value, line );
					break;

				case "Priority":
					m_priority = Parser.ParseInt( value, line );
					break;

				case "Category Entry":
					// Docs appear to be wrong about how this is arranged
					CategoryEntry entry = new CategoryEntry();
					entry.Parse( source, ref line, OwnerProject );
					m_category = entry.FindCategory();
					break;

				case "Effect Entry":
					EffectEntry effectEntry = new EffectEntry();
					effectEntry.Parse( source, ref line, OwnerProject );

					foreach ( Effect effect in OwnerProject.m_globalSettings.m_effects )
					{
						if ( effect.m_name == effectEntry.m_name )
						{
							m_effects.Add( effect );
							break;
						}
					}
					break;

				case "RPC Entry":
					RpcEntry rpcEntry = new RpcEntry();
					rpcEntry.Parse( source, ref line, OwnerProject );
					m_rpcEntries.Add( rpcEntry );
					break;

				case "Track":
					Track track = new Track();
					track.Parse( source, ref line, OwnerProject );
					m_tracks.Add( track );
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
		public int m_volume;
		public int m_pitch;
		public int m_priority;
		public string m_comment;

		public Category m_category;

		public List<Effect> m_effects = new List<Effect>();
		public List<Track> m_tracks = new List<Track>();
		public List<RpcEntry> m_rpcEntries = new List<RpcEntry>();

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class SoundEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Index":
					m_index = Parser.ParseInt( value, line );
					break;

				// Non-interactive sound entries

				case "Weight Max":
					m_weightMax = Parser.ParseInt( value, line );
					break;

				case "Weight Min":
					m_weightMin = Parser.ParseInt( value, line );
					break;

				// Interactive sound entries

				case "Variable Min":
					m_variableMin = Parser.ParseFloat( value, line );
					break;

				case "Variable Max":
					m_variableMax = Parser.ParseFloat( value, line );
					break;

				case "Linger":
					m_linger = Parser.ParseInt( value, line );
					break;
					
				default:
					return false;
			}

			return true;
		}

		public Sound FindSound( SoundBank soundBank )
		{
			foreach ( Sound sound in soundBank.m_sounds )
			{
				if ( sound.m_name == m_name )
				{
					return sound;
				}
			}

			// Not found by name, which suggests index is likely to fail too (so why
			// the XAP file format insists on providing both, I don't understand)
			if ( m_index >= 0 )
			{
				return soundBank.m_sounds[m_index];
			}

			return null;
		}

		#region Member variables
		
		public string m_name;
		public int m_index;

		// Non-interactive sound entries
		public int m_weightMax;
		public int m_weightMin;

		// Interactive sound entries
		public float m_variableMin;
		public float m_variableMax;
		public int m_linger;

		#endregion
	}

	[Serializable]
	public class Track : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
                case "Name":
                    m_name = value;
                    break;

                case "Volume":
					m_volume = Parser.ParseInt( value, line );
					break;

                case "Use Filter":
                    m_useFilter = Parser.ParseInt(value, line);
                    break;

				case "RPC Curve Entry":
					RpcCurveEntry entry = new RpcCurveEntry();
					entry.Parse( source, ref line, OwnerProject );

					RpcCurve curve = OwnerProject.m_globalSettings.m_rpcs[entry.m_rpcIndex].m_rpcCurves[entry.m_curveIndex];
					m_rpcCurves.Add( curve );
					break;

				case "Clip Entry":
					throw new NotImplementedException( "Clip Entries don't exist?" );
					 
				case "Play Wave Event":
					PlayWaveEvent playWave = new PlayWaveEvent();
					playWave.Parse( source, ref line, OwnerProject );
					m_events.Add( playWave );
					break;

				case "Play Sound Event":
					PlaySoundEvent playSound = new PlaySoundEvent();
					playSound.Parse( source, ref line, OwnerProject );
					m_events.Add( playSound );
					break;

				case "Stop Event":
					StopEvent stop = new StopEvent();
					stop.Parse( source, ref line, OwnerProject );
					m_events.Add( stop );
					break;

				case "Branch Event":	// Docs say "not supported", but I'm not sure that's true?
					BranchEvent branch = new BranchEvent();
					branch.Parse( source, ref line, OwnerProject );
					m_events.Add( branch );
					break;
					
				case "Wait Event":
					WaitEvent wait = new WaitEvent();
					wait.Parse( source, ref line, OwnerProject );
					m_events.Add( wait );
					break;

				case "Set Effect Parameter":
					SetEffectParameterEvent effectParam = new SetEffectParameterEvent();
					effectParam.Parse( source, ref line, OwnerProject );
					m_events.Add( effectParam );
					break;

				case "Set Variable Event":
					SetVariableEvent variable = new SetVariableEvent();
					variable.Parse( source, ref line, OwnerProject );
					m_events.Add( variable );
					break;

				case "Set Pitch Event":
					SetPitchEvent pitch = new SetPitchEvent();
					pitch.Parse( source, ref line, OwnerProject );
					m_events.Add( pitch );
					break;

				case "Set Volume Event":
					SetVolumeEvent volume = new SetVolumeEvent();
					volume.Parse( source, ref line, OwnerProject );
					m_events.Add( volume );
					break;

				case "Marker Event":
					MarkerEvent marker = new MarkerEvent();
					marker.Parse( source, ref line, OwnerProject );
					m_events.Add( marker );
					break;

					// These are all obsolete?
				case "Play Wave From Offset Event":
				case "Play Wave Variation Event":
				case "Set Variable Recurring Event":
				case "Set Marker Recurring Event":
				default:
					return false;
			}

			return true;
		}

		#region Member variables

        public string m_name;
		public int m_volume;
        public int m_useFilter;

		public List<RpcCurve> m_rpcCurves = new List<RpcCurve>();

		public List<EventBase> m_events = new List<EventBase>();

		#endregion
	}
}
