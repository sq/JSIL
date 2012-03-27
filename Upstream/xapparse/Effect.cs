using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class Effect : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Global":
					m_global = Parser.ParseInt( value, line );
					break;

				case "Comment":
					m_comment = Parser.ParseComment( value, source, ref line );
					break;

				case "Effect Parameter":
					EffectParameter parameter = new EffectParameter();
					parameter.Parse( source, ref line, OwnerProject );
					m_effectParameters.Add( parameter );
					break;

				case "Parameter Preset":
					// XXX I have only ever seen this within XACT files as "Parameter Preset = ;" ?
					m_parameterPreset = value;
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public string m_name;
		public int m_global;
		public string m_comment;

		public string m_parameterPreset;	// Not documented and seemingly meaningless but appears in the files

		public List<EffectParameter> m_effectParameters = new List<EffectParameter>();

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class EffectEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Effect Name":
					m_name = value;
					break;

				case "Effect Index":
					m_index = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public string m_name;
		public int m_index = -1;

		#endregion

		public Effect FindEffect()
		{
			foreach ( Effect effect in OwnerProject.m_globalSettings.m_effects )
			{
				if ( effect.m_name == m_name )
				{
					return effect;
				}
			}

			// Not found by name, which suggests index is likely to fail too (so why
			// the XAP file format insists on providing both, I don't understand)
			if ( m_index >= 0 )
			{
				return OwnerProject.m_globalSettings.m_effects[m_index];
			}

			return null;
		}
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class EffectParameter : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Name":
					m_name = value;
					break;

				case "Minimum":
					m_min = Parser.ParseFloat( value, line );
					break;

				case "Maximum":
					m_max = Parser.ParseFloat( value, line );
					break;

				case "Value":
					m_value = Parser.ParseFloat( value, line );
					break;

				case "Type":
					m_type = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public string m_name;
		public float m_min;
		public float m_max;
		public float m_value;
		public int m_type;

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_effectName}, {m_parameterName}" )]
	public class EffectParameterEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			switch ( propertyName )
			{
				case "Effect Name":
					m_effectName = value;
					break;

				case "Effect Index":
					m_effectIndex = Parser.ParseInt( value, line );
					break;

				case "Parameter Name":
					m_parameterName = value;
					break;

				case "Parameter Index":
					m_parameterIndex = Parser.ParseInt( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		public EffectParameter FindEffectParameter()
		{
			foreach ( Effect effect in OwnerProject.m_globalSettings.m_effects )
			{
				if ( effect.m_name == m_effectName )
				{
					foreach ( EffectParameter parameter in effect.m_effectParameters )
					{
						if ( parameter.m_name == m_parameterName )
						{
							return parameter;
						}
					}
				}
			}

			// Not found by name, which suggests index is likely to fail too (so why
			// the XAP file format insists on providing both, I don't understand)
			if ( m_effectIndex >= 0 && m_parameterIndex >= 0 )
			{
				return OwnerProject.m_globalSettings.m_effects[m_effectIndex].m_effectParameters[m_parameterIndex];
			}

			return null;
		}

		#region Member variables

		public int m_effectIndex;
		public string m_effectName;
		public int m_parameterIndex;
		public string m_parameterName;

		#endregion
	}
}
