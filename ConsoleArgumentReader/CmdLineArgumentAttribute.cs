using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ZhanTools.ConsoleArgumentReader
{
	/// <summary>
	/// Marked on properties of TArg 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class CmdLineArgumentAttribute : Attribute
	{
		public CmdLineArgumentAttribute()
		{
		}


		public CmdLineArgumentAttribute(string name = null, string description = null, bool isRequired = false)
		{
			this.Name = name;
			this.Description = description;
			this.IsRequired = isRequired;
		}
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsRequired { get; set; }
		public object Default { get; set; }
	}


    class ArgumentInformation
	{
		public string Name;
		public string PropertyName; //Property of the TArg, value will be assigned to this property
		public string Description; //Description of the argument
		public bool IsFlag;
		public bool IsRequired;
		public bool Visited;
		public object Default;
	}
}