using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Consts
{
    ////////////////////////////////////////////
    // Parameters
    ////////////////////////////////////////////
    public static class ConfigurationParameterConsts
	{
        public const string ParamName_AllowEmail = "AllowEmail";
        public const string ParamName_AllowRegistration = "AllowRegistration";
        public const string ParamName_UsersAllowedAnyAuction = "UsersAllowedAnyAuction";
        public const string ParamName_DefaultMinimumBid = "DefaultMinimumBid";
        public const string ParamName_DefaultFontTitle = "DefaultFontTitle";
        public const string ParamName_DefaultFontBody = "DefaultFontBody";
        public const string ParamName_CurrencyText = "CurrencyText";
        public const string ParamName_CurrencyPosition = "CurrencyPosition";
        public const string ParamName_CurrencyAddSpace = "CurrencyAddSpace";
        public const string ParamName_CurrencyDecimalPlaces = "CurrencyDecimalPlaces";

        // default values
        public const string ParamDefault_DefaultFontTitle = "Sorts Mill Goudy";
        public const string ParamDefault_DefaultFontBody = "Sorts Mill Goudy";

        public static string GetCategoryName(ParameterCategory category)
		{
			switch(category)
			{
				case ParameterCategory.Bid:
					return SilentAuction.Resources.TextStrings.CategoryBid;

                case ParameterCategory.Registration:
                    return SilentAuction.Resources.TextStrings.CategoryRegistration;

                case ParameterCategory.Technical:
                    return SilentAuction.Resources.TextStrings.CategoryTechnical;

                case ParameterCategory.Global:
                    return SilentAuction.Resources.TextStrings.CategoryGlobal;

                case ParameterCategory.Currency:
                    return SilentAuction.Resources.TextStrings.Currency;
                default:
					return "Category not found";
			}
		}
	}


}