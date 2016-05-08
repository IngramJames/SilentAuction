using Common.Attributes;
using SilentAuction.Common;
using SilentAuction.Resources;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SilentAuction.Models
{
	public enum ParameterType
	{
		Text=1,
		Boolean=2,
		Integer=3,
		Double=4,
		Currency=5,
        Enumeration=6
	}

	public enum ParameterCategory
	{
		Registration=0,
		Bid=1,
        Technical=2,
        Global=3,
        Hidden=4,       // for things that are not displayed on the config page because they have their own page. Fonts, for example.
        Currency=5
	}

	/// <summary>
	/// A configuration parameter for dictating behaviour of the application
	/// </summary>
	public class ConfigurationParameter
	{

		public string m_key;

		public int Id { get; set; }

		[Required]
		[StringLength(50)]		// hidden field; no localization needed
		[DataType(DataType.Text)]
		public string Key
		{
			get { return m_key; }
			set
			{
				m_key = value;
				Decorate();
			}
		}

		// Setting - string requires some defining.
		[DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "Setting")]
		[StringLengthLocalized(100, 0, typeof(SilentAuction.Resources.TextStrings), "ValidationErrorMinLength")]
		[DataType(DataType.Text)]
		public string SettingAsString { get; set; }

		// other setting types
		public int SettingAsInt { get; set; }
		public decimal SettingAsDecimal { get; set; }
		public double SettingAsDouble { get; set; }
		public bool SettingAsBool { get; set; }

        [Required]
        [DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "AdminSetting")]
        public bool AdminSetting { get; set; }

        [Required]
        [DisplayNameLocalized(typeof(SilentAuction.Resources.TextStrings), "UserOverridable")]
        public bool UserOverridable { get; set; }

        [Required]
		public ParameterType Type { get; set; }

		[Required]
		public ParameterCategory Category { get; set; }

		[NotMapped]
		public string ReadableName { get; set; }

		[NotMapped]
		public string ReadableDescription { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public ConfigurationParameter()
		{
			ReadableDescription = string.Empty;
			ReadableName = string.Empty;
		}

		/// <summary>
		/// Add translationable text using the key
		/// </summary>
		private void Decorate()
		{
            // Manual lookup by key because this text needs to be in a resource file.
            ReadableName = ParameterDetails.ResourceManager.GetString(Key + "Name");
            ReadableDescription = ParameterDetails.ResourceManager.GetString(Key + "Description");

            // Default to the key if not handled above.
            if (ReadableName == null)
            {
                ReadableName = Key + " (text not found)";
            }

            if (ReadableDescription == null)
            {
                ReadableDescription = Key + " (text not found)";
            }
		}



		/// <summary>
		/// Return setting value as a formatted string for display
		/// </summary>
		/// <returns></returns>
		public string FormattedSetting()
		{
			switch(Type)
			{
				case ParameterType.Boolean:
					if (SettingAsBool)
					{
						return ParameterDetails.BooleanYes;
					}
					else
					{
						return ParameterDetails.BooleanNo;
					}

				case ParameterType.Currency:
					decimal valDecimal = SettingAsDecimal;
                    return Formatting.FormatCurrency(valDecimal);
				case ParameterType.Double:
					double valDouble = SettingAsDouble;
					return valDouble.ToString("n");
				case ParameterType.Integer:
					int valInt = SettingAsInt;
					return valInt.ToString("n");
                case ParameterType.Enumeration:
                    // TODO implement combo boxes and lookups
                    // For now, just treat as an int and rely on magic dev knowledge
                    int valEnum = SettingAsInt;
                    return valEnum.ToString("n");
                default:
					return SettingAsString;

			}
		}
	}
}