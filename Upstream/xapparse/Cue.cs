using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay("{m_name}")]
	public class Cue : Entity
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

				case "Variation":
					m_variation = new Variation();
					m_variation.Parse( source, ref line, OwnerProject );
					break;

				case "Sound Entry":
					SoundEntry entry = new SoundEntry();
					entry.Parse( source, ref line, OwnerProject );
					m_soundEntries.Add( entry );
					break;

				case "Instance Limit":
					m_instanceLimit = new InstanceLimit();
					m_instanceLimit.Parse( source, ref line, OwnerProject );
					break;

				case "Transition Entry": // Docs say "Transition"
					TransitionEntry transitionEntry = new TransitionEntry();
					transitionEntry.Parse( source, ref line, OwnerProject );
					m_transitionEntries.Add( transitionEntry );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public SoundBank m_ownerSoundBank;

		public string m_name;
		public string m_comment;

		public Variation m_variation;
		public InstanceLimit m_instanceLimit;
		public List<SoundEntry> m_soundEntries = new List<SoundEntry>();
		public List<TransitionEntry> m_transitionEntries = new List<TransitionEntry>();

		#endregion

		#region Query for more advanced knowledge than simple member data

		public List<string> RequiredWavebanks
		{
			get
			{
				List<string> waveBanks = new List<string>();

				foreach ( SoundEntry soundEntry in m_soundEntries )
				{
					Sound sound = soundEntry.FindSound( m_ownerSoundBank );
					
					foreach ( Track track in sound.m_tracks )
					{
						foreach ( EventBase trackEvent in track.m_events )
						{
							PlayWaveEvent playWave = trackEvent as PlayWaveEvent;	// TBD any other event types?

							if ( playWave != null )
							{
								foreach ( WaveEntry waveEntry in playWave.m_waveEntries )
								{
									waveBanks.Add( waveEntry.m_bankName );
								}
							}
						}
					}
				}

				return waveBanks;
			}
		}

		public bool IsLooping()
		{
			foreach ( SoundEntry soundEntry in m_soundEntries )
			{
				Sound sound = soundEntry.FindSound( m_ownerSoundBank );

				foreach ( Track track in sound.m_tracks )
				{
					foreach ( EventBase trackEvent in track.m_events )
					{
						PlayWaveEvent playWave = trackEvent as PlayWaveEvent;	// TBD any other event types?

						if ( playWave != null )
						{
							if ( playWave.m_loopCount == 255 )
							{
								// Only care about infinite loops. Something that loops 254 times then stops is
								// still only a very long sound as far as anyone but XACT is concerned.
								return true;
							}
						}
					}
				}
			}
		
			return false;
		}

		// Minimum and maximum distances aren't really an XACT term any more because you can have
		// arbitrary distance curves. However, it's still useful to be able to know the minimum
		// distance (distance up to which volume does not attenuate) and maximum distance (distance
		// at which volume is silent) for things like culling, and volume tweaking. NOTE - after the
		// last point on the volume curve, XACT no longer attenuates the volume, so it's possible to
		// have a sound heard infinitely far away. This deals with that quite happily - the maximum
		// distance is where the volume is silent, not merely the last point on the graph.
		public void GetMinMaxDistances( ref float minDist, ref float maxDist )
		{
			minDist = float.MaxValue;
			maxDist = float.MinValue;
			bool distSet = false;

			foreach ( SoundEntry soundEntry in m_soundEntries )
			{
				Sound sound = soundEntry.FindSound( m_ownerSoundBank );

				foreach ( RpcEntry rpcEntry in sound.m_rpcEntries )
				{
					Rpc rpc = rpcEntry.FindRpc();

					if ( rpc != null )
					{
						foreach ( Variable variable in rpc.m_variables )
						{
							if ( variable.m_name == "Distance" )
							{
								foreach ( RpcCurve curve in rpc.m_rpcCurves )
								{
									if ( curve.m_property == 0 )	// 0 => volume, 1 => pitch according to the docs
									{
										foreach ( RpcPoint point in curve.m_rpcPoints )
										{
											if ( point.m_y >= 0.0f && point.m_x < minDist )
											{
												minDist = point.m_x;
											}

											// The graph goes down to -96.0f but there's no point in counting something
											// only -93dB as not silent... -64dB is close enough
											if ( point.m_y < -64.0f && point.m_x > maxDist )
											{
												maxDist = point.m_x;
											}

											distSet = true;
										}
									}
								}
							}
						}
					}
				}
			}

			if ( !distSet )
			{
				minDist = maxDist = float.MaxValue;
			}
		}

		// The "Notes" field in the XACT GUI can be used to extend XACT with features it never
		// expected. A cue's comments must also consider the sounds and categories it is attached
		// to, so that a single comment in a category affects all sounds and cues within it.
		public string FullComments
		{
			get
			{
				StringBuilder fullComments = new StringBuilder();

				if ( m_comment != null )
				{
					fullComments.Append( m_comment );
					fullComments.Append( " | " );
				}

				if ( m_ownerSoundBank.m_comment != null )
				{
					fullComments.Append( m_ownerSoundBank.m_comment );
					fullComments.Append( " | " );
				}

				foreach ( SoundEntry soundEntry in m_soundEntries )
				{
					Sound sound = soundEntry.FindSound( m_ownerSoundBank );

					if ( sound != null )
					{
						if ( sound.m_comment != null )
						{
							fullComments.Append( sound.m_comment );
							fullComments.Append( " | " );
						}

						Category category = sound.m_category;

						while ( category != null )
						{
							if ( category.m_comment != null )
							{
								fullComments.Append( category.m_comment );
								fullComments.Append( " | " );
							}

							category = category.m_parentCategory.FindCategory();
						}
					}
				}

				return fullComments.ToString();
			}
		}

		#endregion
	}

	[Serializable]
	public class TransitionEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Sound Index":
					m_soundIndex = Parser.ParseInt( value, line );
					break;

				case "Source Marker Min":
					m_sourceMarkerMin = Parser.ParseInt( value, line );
					break;

				case "Source Marker Max":
					m_sourceMarkerMax = Parser.ParseInt( value, line );
					break;

				case "Destination Marker Min":
					m_destMarkerMin = Parser.ParseInt( value, line );
					break;

				case "Destination Marker Max":
					m_destMarkerMax = Parser.ParseInt( value, line );
					break;

				case "Transition Type":
					m_transitionType = Parser.ParseInt( value, line );
					break;

				case "Transition Source":
					m_transitionSource = Parser.ParseInt( value, line );
					break;

				case "Transition Destination":
					m_transitionDest = Parser.ParseInt( value, line );
					break;

				case "Crossfade":
					m_crossfade = new Crossfade();
					m_crossfade.Parse( source, ref line, OwnerProject );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public int m_soundIndex;
		public int m_sourceMarkerMin;
		public int m_sourceMarkerMax;
		public int m_destMarkerMin;
		public int m_destMarkerMax;
		public int m_transitionType;
		public int m_transitionSource;
		public int m_transitionDest;
		public Crossfade m_crossfade;

		#endregion
	}
}
