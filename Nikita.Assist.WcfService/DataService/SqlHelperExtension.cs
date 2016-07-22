using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nikita.Assist.WcfService
{
    /// <summary>
    /// SqlHelper��չ(����AutoMapper.dll)
    /// </summary>
    public sealed partial class SqlHelper
    {
        #region ʵ������

        public T ExecuteObject<T>(string commandText, params SqlParameter[] parms)
        {
            return ExecuteObject<T>(ConnectionString, commandText, parms);
        }

        public List<T> ExecuteObjects<T>(string commandText, params SqlParameter[] parms)
        {
            return ExecuteObjects<T>(ConnectionString, commandText, parms);
        }

        #endregion ʵ������

        #region ��̬����

        public static T ExecuteObject<T>(string connectionString, string commandText, params SqlParameter[] parms)
        {
            //DataTable dt = ExecuteDataTable(connectionString, commandText, parms);
            //return AutoMapper.Mapper.DynamicMap<List<T>>(dt.CreateDataReader()).FirstOrDefault();
            using (SqlDataReader reader = ExecuteDataReader(connectionString, commandText, parms))
            {
                return AutoMapper.Mapper.DynamicMap<List<T>>(reader).FirstOrDefault();
            }
        }

        public static List<T> ExecuteObjects<T>(string connectionString, string commandText, params SqlParameter[] parms)
        {
            //DataTable dt = ExecuteDataTable(connectionString, commandText, parms);
            //return AutoMapper.Mapper.DynamicMap<List<T>>(dt.CreateDataReader());
            using (SqlDataReader reader = ExecuteDataReader(connectionString, commandText, parms))
            {
                return AutoMapper.Mapper.DynamicMap<List<T>>(reader);
            }
        }

        #endregion ��̬����
    }
}