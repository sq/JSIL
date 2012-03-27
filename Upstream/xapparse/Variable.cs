using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class Variable : Entity
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

				case "Public":
					m_isPublic = Parser.ParseInt( value, line );
					break;

				case "Global":
					m_isGlobal = Parser.ParseInt( value, line );
					break;

				case "Internal":
					m_internal = Parser.ParseInt( value, line );
					break;

				case "External":
					m_external = Parser.ParseInt( value, line );
					break;

				case "Monitored":
					m_monitored = Parser.ParseInt( value, line );
					break;

				case "Reserved":
					m_reserved = Parser.ParseInt( value, line );
					break;

				case "Read Only":
					m_readOnly = Parser.ParseInt( value, line );
					break;

				case "Time":
					m_time = Parser.ParseInt( value, line );
					break;

				case "Value":
					m_value = Parser.ParseFloat( value, line );
					break;

				case "Initial Value":
					m_initialValue = Parser.ParseFloat( value, line );
					break;

				case "Min":
					m_min = Parser.ParseFloat( value, line );
					break;

				case "Max":
					m_max = Parser.ParseFloat( value, line );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member Variables

		public string m_name;
		public string m_comment;
		public int m_isPublic;
		public int m_isGlobal;
		public int m_internal;
		public int m_external;
		public int m_monitored;
		public int m_reserved;
		public int m_readOnly;
		public int m_time;
		public float m_value;
		public float m_initialValue;
		public float m_min;
		public float m_max;

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class VariableEntry : Entity
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

				default:
					return false;
			}

			return true;
		}

		public Variable FindVariable()
		{
			foreach ( Variable variable in OwnerProject.m_globalSettings.m_variables )
			{
				if ( variable.m_name == m_name )
				{
					return variable;
				}
			}

			// Not found by name, which suggests index is likely to fail too (so why
			// the XAP file format insists on providing both, I don't understand)
			if ( m_index >= 0 )
			{
				return OwnerProject.m_globalSettings.m_variables[m_index];
			}

			return null;
		}

		#region Member variables

		public string m_name;
		public int m_index = -1;

		#endregion
	}
}
