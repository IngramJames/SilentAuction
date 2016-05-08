using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;

namespace SilentAuction.Common
{
    public static class DBUtils
    {

        public static DbParameter CreateParam(DbCommand command, DbType type, string name, object value)
        {
            DbParameter param = CreateBasicParam(command, type, name);
            if (value == null)
            {
                param.Value = DBNull.Value;
            }
            else
            {
                param.Value = value;
            }
            return param;
        }


        private static DbParameter CreateBasicParam(DbCommand command, DbType type, string name)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = name;
            param.DbType = type;
            return param;
        }

        public static DbParameter CreateReturnParam(DbCommand command, DbType type, string name)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = name;
            param.DbType = type;
            param.Direction = ParameterDirection.ReturnValue;

            return param;
        }
    }
}