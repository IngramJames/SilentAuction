using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SilentAuction.Models
{
    // TODO: make part of cache
	public class ConfigurationParameters
	{
		private ApplicationDbContext m_db;
		private Dictionary<string, ConfigurationParameter> m_systemParametersByKey;
		private Dictionary<int, ConfigurationParameter> m_systemParametersById;

		/// <summary>
		/// Constructor
		/// </summary>
		public ConfigurationParameters()
		{
			m_db = new ApplicationDbContext();

			RefreshParameters();
		}

		public void RefreshParameters()
		{
			// Initialise system parameters
			m_systemParametersByKey = m_db.SystemParameters.Where(t => t.AdminSetting == true).ToDictionary(p => p.Key);
			m_systemParametersById = m_db.SystemParameters.Where(t => t.AdminSetting == true).ToDictionary(p => p.Id);
		}

		public ConfigurationParameter GetParameterById(int id)
		{
			return m_systemParametersById[id];
		}

		public ConfigurationParameter GetParameterByKey(string key)
		{
			return m_systemParametersByKey[key];
		}

		/// <summary>
		/// Return a dictionary of dictionaries of system parameter, keyed by Category
		/// </summary>
		/// <returns></returns>
		public Dictionary<ParameterCategory, Dictionary<string, ConfigurationParameter>> GetSystemParametersByCategory()
		{
			return GetParametersByCategory(m_systemParametersByKey);
		}

        /// <summary>
        /// Returns parameters sorted by category
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
		public Dictionary<ParameterCategory, Dictionary<string, ConfigurationParameter>> GetParametersByCategory(Dictionary<string, ConfigurationParameter> parameters)
		{
			Dictionary<ParameterCategory, Dictionary<string, ConfigurationParameter>> result = new Dictionary<ParameterCategory, Dictionary<string, ConfigurationParameter>>();
			
			foreach(KeyValuePair<string, ConfigurationParameter> kvp in parameters)
			{
				Dictionary<string, ConfigurationParameter> paramsInCategory;
				if (result.ContainsKey(kvp.Value.Category))
				{
					paramsInCategory = result[kvp.Value.Category];
				}
				else
				{
					paramsInCategory = new Dictionary<string, ConfigurationParameter>();
					result.Add(kvp.Value.Category, paramsInCategory);
				}

				paramsInCategory.Add(kvp.Key, kvp.Value);
			}

			return result;
		}
	}
}