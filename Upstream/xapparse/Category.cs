using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xap
{
	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class Category : Entity
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
					m_public = Parser.ParseInt( value, line );
					break;

				case "Background Music":
					m_backgroundMusic = Parser.ParseInt( value, line );
					break;

				case "Volume":
					m_volume = Parser.ParseInt( value, line );
					break;
					
				case "Category Entry":
				case "Parent":	// So say the docs, but not the XAP file!
					m_parentCategory = new CategoryEntry();
					m_parentCategory.Parse( source, ref line, OwnerProject );
					break;

				case "Instance Limit":
					m_instanceLimit = new InstanceLimit();
					m_instanceLimit.Parse( source, ref line, OwnerProject );
					break;

				default:
					return false;
			}

			return true;
		}

		#region Member variables

		public string m_name;
		public string m_comment;
		public int m_public;
		public int m_backgroundMusic;
		public int m_volume;

		public CategoryEntry m_parentCategory;
		public InstanceLimit m_instanceLimit;

		#endregion

		#region Properties

		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		#endregion
	}

	[Serializable]
	[DebuggerDisplay( "{m_name}" )]
	public class CategoryEntry : Entity
	{
		public override bool SetProperty( string propertyName, string value, string[] source, ref int line )
		{
			if ( propertyName == "Name" )
			{
				m_name = value;
				return true;
			}

			return false;
		}

		public Category FindCategory()
		{
			foreach ( Category category in OwnerProject.m_globalSettings.m_categories )
			{
				if ( category.m_name == m_name )
				{
					return category;
				}
			}

			return null;
		}

		#region Member variables

		// NOTE name does not always get set, that's intentional (ie. for root level categories).
		string m_name = "";

		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		#endregion
	}
}
