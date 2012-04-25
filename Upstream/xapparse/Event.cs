using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	// NOTE the documentation for this stuff in particular is VERY lacking! Several events 
	// have no details (do they even exist?) and some of it is just plain wrong. It's also
	// often unclear whether a number is an int or a float, and at least sometimes it can
	// be either depending on context.

	[Serializable]
	public abstract class EventBase : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			// Derived classes should attempt to SetProperty first, and call into this class if they can't
			switch ( propertyName )
			{
				case "Event Header":
					m_header = new EventHeader();
					m_header.Parse( source, ref line, OwnerProject );
					break;

				default:
					return false;
			}

			return true;
		}

		public EventHeader m_header;
	}

	#region Event types

	[Serializable]
	public class PlayWaveEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Loop Count":
					m_loopCount = Parser.ParseInt( value, line );
					break;

				case "Break Loop":
					m_breakLoop = Parser.ParseInt( value, line );
					break;

				case "Use Speaker Position":
					m_useSpeakerPosition = Parser.ParseInt( value, line );
					break;

				case "Use Center Speaker":
					m_useCentreSpeaker = Parser.ParseInt( value, line );
					break;

				case "New Speaker Position On Loop":
					m_newSpeakerPositionOnLoop = Parser.ParseInt( value, line );
					break;

				case "Speaker Position Angle":
					m_speakerPositionAngle = Parser.ParseFloat( value, line );
					break;

				case "Speaer Position Arc":		// XACT typo, not mine!
				case "Speaker Position Arc":	// In case they fix the typo
					m_speakerPositionArc = Parser.ParseFloat( value, line );
					break;

				case "Wave Entry":
					WaveEntry wave = new WaveEntry();
					wave.Parse( source, ref line, OwnerProject );
					m_waveEntries.Add( wave );
					break;

				case "Pitch Variation":
					m_pitchVariation = new PitchVariation();
					m_pitchVariation.Parse( source, ref line, OwnerProject );
					break;

				case "Volume Variation":
					m_volumeVariation = new VolumeVariation();
					m_volumeVariation.Parse( source, ref line, OwnerProject );
					break;

				case "Variation":
					m_variation = new Variation();
					m_variation.Parse( source, ref line, OwnerProject );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public int m_loopCount;
		public int m_breakLoop;
		public int m_useSpeakerPosition;
		public int m_useCentreSpeaker;
		public int m_newSpeakerPositionOnLoop;
		public float m_speakerPositionAngle;
		public float m_speakerPositionArc;

		public PitchVariation m_pitchVariation;
		public VolumeVariation m_volumeVariation;
		public Variation m_variation;

		public List<WaveEntry> m_waveEntries = new List<WaveEntry>();
	}

	[Serializable]
	public class PlaySoundEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Variation":
					m_variation = new Variation();
					m_variation.Parse( source, ref line, OwnerProject );
					break;

				case "Sound Entry":
					SoundEntry soundEntry = new SoundEntry();
					soundEntry.Parse( source, ref line, OwnerProject );
					m_soundEntries.Add( soundEntry );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public EventHeader m_eventHeader;
		public Variation m_variation;
		public List<SoundEntry> m_soundEntries = new List<SoundEntry>();
	}

	[Serializable]
	public class StopEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Immediate":
					m_immediate = Parser.ParseInt( value, line );
					break;

				case "Stop Cue":
					m_stopCue = Parser.ParseInt( value, line );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public int m_immediate;
		public int m_stopCue;
	}

	[Serializable]
	public class BranchEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				// Docs say "not supported" but I'm not sure that that's true!

				case "Conditional":
					m_conditional = new Conditional();
					m_conditional.Parse( source, ref line, OwnerProject );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public Conditional m_conditional;
	}

	[Serializable]
	public class WaitEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Pause Playback":
					m_pause = Parser.ParseInt( value, line );
					break;

				case "Conditional":
					m_conditional = new Conditional();
					m_conditional.Parse( source, ref line, OwnerProject );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public int m_pause;
		public Conditional m_conditional;
	}

	[Serializable]
	public class SetEffectParameterEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Minimum":
					m_min = Parser.ParseIntOrFloat( value, line );
					break;

				case "Maximum":
					m_max = Parser.ParseIntOrFloat( value, line );
					break;

				case "Value":
					m_value = Parser.ParseIntOrFloat( value, line );
					break;

				case "Type":
					m_type = Parser.ParseInt( value, line );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public string m_name;
		public float m_min;
		public float m_max;
		public float m_value;
		public int m_type;		// 0: float, 1: integer.... nice
	}

	[Serializable]
	public class SetVariableEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Variable Entry":
					m_variable = new VariableEntry();
					m_variable.Parse( source, ref line, OwnerProject );
					break;
				
				case "Equation":
					m_equation = new Equation();
					m_equation.Parse( source, ref line, OwnerProject );
					break;

				case "Recurrence":
					m_recurrence = new Recurrence();
					m_recurrence.Parse( source, ref line, OwnerProject );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public VariableEntry m_variable;
		public Equation m_equation;
		public Recurrence m_recurrence;
	}

	[Serializable]
	public class SetPitchEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Ramp":
					m_ramp = new Ramp();
					m_ramp.Parse( source, ref line, OwnerProject );
					break;

				case "Equation":
					m_equation = new Equation();
					m_equation.Parse( source, ref line, OwnerProject );
					break;

				case "Recurrence":
					m_recurrence = new Recurrence();
					m_recurrence.Parse( source, ref line, OwnerProject );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public Ramp m_ramp;
		public Equation m_equation;
		public Recurrence m_recurrence;
	}

	[Serializable]
	public class SetVolumeEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Ramp":
					m_ramp = new Ramp();
					m_ramp.Parse( source, ref line, OwnerProject );
					break;

				case "Equation":
					m_equation = new Equation();
					m_equation.Parse( source, ref line, OwnerProject );
					break;

				case "Recurrence":
					m_recurrence = new Recurrence();
					m_recurrence.Parse( source, ref line, OwnerProject );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public Ramp m_ramp;
		public Equation m_equation;
		public Recurrence m_recurrence;
	}

	[Serializable]
	public class MarkerEvent : EventBase
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Data":
					m_data = Parser.ParseInt( value, line );
					break;

				case "Recurrence":
					m_recurrence = new Recurrence();
					m_recurrence.Parse( source, ref line, OwnerProject );
					break;

				default:
					return base.SetProperty( propertyName, value, source, ref line );
			}

			return true;
		}

		public int m_data;
		public Recurrence m_recurrence;
	}

	#endregion

	#region Event attributes

	[Serializable]
	public class EventHeader : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Comment":
					m_comment = Parser.ParseComment( value, source, ref line );
					break;

				case "Timestamp":
					m_timestamp = Parser.ParseInt( value, line );
					break;

				case "Relative":
					m_relative = Parser.ParseInt( value, line );
					break;

				case "Relative Event Index":
					m_relativeEventIndex = Parser.ParseInt( value, line );
					break;

				case "Relative To Start":
					m_relativeToStart = Parser.ParseInt( value, line );
					break;

				case "Random Offset":
					m_randomOffset = Parser.ParseInt( value, line );
					break;

				case "Random Recurrence":
					m_randomRecurrence = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		public int m_timestamp;
		public int m_relative;
		public int m_relativeEventIndex;
		public int m_relativeToStart;
		public int m_randomOffset;
		public int m_randomRecurrence;
		public string m_comment;
	}

	[Serializable]
	public class Conditional : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Type":
					m_type = Parser.ParseInt( value, line );
					break;

				case "Operand":
					Operand operand = new Operand();
					operand.Parse( source, ref line, OwnerProject );
					m_operands.Add( operand );
					break;

				case "Operator1":
					m_operator1 = Parser.ParseInt( value, line );
					break;

				case "Operator2":
					m_operator2 = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		public int m_type;
		public List<Operand> m_operands;
		public int m_operator1;
		public int m_operator2 = -1;

	}

	[Serializable]
	public class Operand : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Variable Entry":
					m_variableEntry = new VariableEntry();
					m_variableEntry.Parse( source, ref line, OwnerProject );
					break;

				case "Constant":
					m_constant = Parser.ParseFloat( value, line );
					break;

				case "Min Random":
					m_minRandom = Parser.ParseFloat( value, line );
					break;

				case "Max Random":
					m_maxRandom = Parser.ParseFloat( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		// Docs say "only one of the following can exist", remember that when it comes to writing them out

		public VariableEntry m_variableEntry;

		public float m_constant = float.NaN;

		public float m_minRandom = float.NaN;
		public float m_maxRandom = float.NaN;
	}

	[Serializable]
	public class Equation : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Operator":
					m_operator = Parser.ParseInt( value, line );
					break;

				case "Operand":
					m_operand = new Operand();
					m_operand.Parse( source, ref line, OwnerProject );
					break;

				default:
					return false;
			}

			return true;
		}

		public Operand m_operand;
		public int m_operator;
	}

	[Serializable]
	public class Recurrence : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Count":
					m_count = Parser.ParseInt( value, line );
					break;

				case "Frequency":
					m_frequency = Parser.ParseInt( value, line );
					break;

				case "Beats Per Minute":
					m_bpm = Parser.ParseInt( value, line );
					break;

				case "Beats Per Minute Frequency":	// Not in docs
					m_bpmFrequency = Parser.ParseFloat( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		public int m_count;
		public int m_frequency;
		public int m_bpm;
		public float m_bpmFrequency;
	}

	[Serializable]
	public class Ramp : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Initial Value":
					m_initialValue = Parser.ParseFloat( value, line );
					break;

				case "Slope":
					m_slope = Parser.ParseFloat( value, line );
					break;

				case "Slope Delta":
					m_slopeDelta = Parser.ParseFloat( value, line );
					break;

				case "Duration":
					m_duration = Parser.ParseInt( value, line );
					break;				

				default:
					return false;
			}

			return true;
		}

		public float m_initialValue;
		public float m_slope;
		public float m_slopeDelta;
		public int m_duration;
	}

	#endregion
}
