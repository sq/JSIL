using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	public class PitchVariation : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Min":
					m_minPitch = Parser.ParseFloat( value, line );
					break;

				case "Max":
					m_maxPitch = Parser.ParseFloat( value, line );
					break;

				case "Operator":
					m_operator = Parser.ParseInt( value, line );
					break;

				case "New Variation On Loop":
					m_newVariationOnLoop = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public float m_minPitch;
		public float m_maxPitch;
		public int m_operator;
		public float m_newVariationOnLoop;

		#endregion
	}

	[Serializable]
	public class VolumeVariation : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Min":
					m_minVolume = Parser.ParseInt( value, line );
					break;

				case "Max":
					m_maxVolume = Parser.ParseInt( value, line );
					break;

				case "Volume":
					m_volume = Parser.ParseInt( value, line );
					break;

				case "New Variation On Loop":
					m_newVariationOnLoop = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public int m_minVolume;
		public int m_maxVolume;
		public int m_volume;
		public int m_newVariationOnLoop;

		#endregion
	}

	[Serializable]
	public class Variation : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Variation Type":
					m_type = Parser.ParseInt( value, line );
					break;

				case "Variation Table Type":
					m_tableType = Parser.ParseInt( value, line );
					break;

				case "New Variation on Loop":
					m_newVariationOnLoop = Parser.ParseInt( value, line );
					break;

				case "Variable Entry":	// Docs say "Variable Index", default 0xffff
					m_variableEntry = new VariableEntry();
					m_variableEntry.Parse( source, ref line, OwnerProject );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public int m_type;
		public int m_tableType;
		public int m_newVariationOnLoop;
		public VariableEntry m_variableEntry;

		#endregion
	}
}
