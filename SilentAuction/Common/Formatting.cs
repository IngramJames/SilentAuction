using SilentAuction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Common
{
    public static class Formatting
    {

        public static string FormatCurrency(decimal amount)
        {
            ConfigurationParameters configParams = new ConfigurationParameters();
            ConfigurationParameter currencyStringParam = configParams.GetParameterByKey(Consts.ConfigurationParameterConsts.ParamName_CurrencyText);
            ConfigurationParameter currencyPositionParam = configParams.GetParameterByKey(Consts.ConfigurationParameterConsts.ParamName_CurrencyPosition);
            ConfigurationParameter currencySpaceParam = configParams.GetParameterByKey(Consts.ConfigurationParameterConsts.ParamName_CurrencyAddSpace);
            ConfigurationParameter currencyDecimalPlaces = configParams.GetParameterByKey(Consts.ConfigurationParameterConsts.ParamName_CurrencyDecimalPlaces);

            // get no of decimal places and prepare the formatting substring as appropriate
            bool hasDecimalPlaces = currencyDecimalPlaces.SettingAsBool;
            string decimalPlacesFormat = string.Empty;

            if (hasDecimalPlaces)
            {
                decimalPlacesFormat = ".00";
            }

            // get complete format string and format the value as a number
            string valueFormat = "#,##0" + decimalPlacesFormat;
            string formattedAmount = string.Format("{0}", amount.ToString(valueFormat));

            // get symbol, position, and whether or not to add a space
            string symbol = currencyStringParam.SettingAsString;
            Consts.CurrencyTextPosition textPosition = (Consts.CurrencyTextPosition)currencyPositionParam.SettingAsInt;
            bool addSpace = currencySpaceParam.SettingAsBool;

            // prepare a space to add if necessary
            string space = string.Empty;        
            if (addSpace)
            {
                space = " ";
            }

            string formatString = "{0}{1}{2}";
            string formattedResult;
            if (textPosition == Consts.CurrencyTextPosition.Left)
            {
                // symbol at left so format is: sybmol, space, amount. eg "USD 5,000", "EUR200"
                formattedResult = string.Format(formatString, symbol, space, formattedAmount);
            }
            else
            {
                // symbol at right: amount, space, symbol. eg "5000USD", "1,234.56 CAD"
                formattedResult = string.Format(formatString, formattedAmount, space, symbol);
            }

            return formattedResult;
        }
    }
}