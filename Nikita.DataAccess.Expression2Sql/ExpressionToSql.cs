﻿using Nikita.Base.Define;
using Nikita.DataAccess.Expression2Sql.Mapper;
using Nikita.DataAccess4DBHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Nikita.DataAccess.Expression2Sql
{
    public class ExpressionToSql<T>
    {
        public SqlType SqlType { get; private set; }

        public string ConnectionString { get; private set; }

        private IDbHelper dbHelper;
        private string _mainTableName = typeof(T).Name;
        private SqlBuilder _sqlBuilder;

        /// <summary>
        ///
        /// </summary>
        /// <param name="sqlType">数据库类型</param>
        /// <param name="strConnectionString">数据库连接字符串，如果只是转换成sql语句不需要传入，如果需要转换实体等需要传入连接串</param>
        public ExpressionToSql(SqlType sqlType, string strConnectionString)
        {
            this.SqlType = sqlType;
            this.ConnectionString = strConnectionString;
            switch (sqlType)
            {
                case SqlType.SqlServer:
                    this._sqlBuilder = new SqlBuilder(new SQLServerSqlParser());
                    break;

                case SqlType.Oracle:
                    this._sqlBuilder = new SqlBuilder(new OracleSqlParser());
                    break;

                case SqlType.SQLite:
                    this._sqlBuilder = new SqlBuilder(new SQLiteSqlParser());
                    break;

                case SqlType.MySql:
                    this._sqlBuilder = new SqlBuilder(new MySQLSqlParser());
                    break;

                case SqlType.SqlServerCe:
                case SqlType.PostgreSql:
                case SqlType.Db2:
                case SqlType.Accesss:
                    throw new Exception("暂不支持此类型数据库");
            }
            dbHelper = DbHelper.GetDbHelper(sqlType, strConnectionString);
        }

        //public ExpressionToSql(DbContext dbContext)
        //{
        //    IDbSqlParser dbSqlParser = null;
        //    switch (dbContext.SqlType)
        //    {
        //        case SqlType.SqlServer:
        //            dbSqlParser = new SQLServerSqlParser();
        //            break;
        //        case SqlType.Oracle:
        //            dbSqlParser = new OracleSqlParser();
        //            break;
        //        case SqlType.MySql:
        //            dbSqlParser = new MySQLSqlParser();
        //            break;
        //        case SqlType.SQLite:
        //            dbSqlParser = new SQLiteSqlParser();
        //            break;
        //    }
        //    this._sqlBuilder = new SqlBuilder(dbSqlParser);
        //    dbHelper = DbHelper.GetDbHelper(dbContext.SqlType, dbContext.ConnectionString);
        //}

        public Dictionary<string, object> DbParams
        {
            get
            {
                return this._sqlBuilder.DbParams;
            }
        }

        public string Sql
        {
            get
            {
                return this._sqlBuilder.Sql + ";";
            }
        }

        public ExpressionToSql<T> Avg(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this.Clear();
            Expression2SqlProvider.Avg(expression.Body, this._sqlBuilder);
            return this;
        }

        public void Clear()
        {
            this._sqlBuilder.Clear();
        }

        public ExpressionToSql<T> Count(Expression<Func<T, object>> expression = null)
        {
            this.Clear();
            if (expression == null)
            {
                string tableName = typeof(T).Name;

                this._sqlBuilder.SetTableAlias(tableName);
                string tableAlias = this._sqlBuilder.GetTableAlias(tableName);

                if (!string.IsNullOrWhiteSpace(tableAlias))
                {
                    tableName += " " + tableAlias;
                }
                this._sqlBuilder.AppendFormat("select count(*) from {0}", tableName);
            }
            else
            {
                Expression2SqlProvider.Count(expression.Body, this._sqlBuilder);
            }

            return this;
        }

        public ExpressionToSql<T> Delete()
        {
            this.Clear();
            this._sqlBuilder.IsSingleTable = true;
            this._sqlBuilder.SetTableAlias(this._mainTableName);
            this._sqlBuilder.AppendFormat("delete {0}", this._mainTableName);
            return this;
        }

        public ExpressionToSql<T> FullJoin<T2>(Expression<Func<T, T2, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser(expression, blnWithNoLock, "full ");
        }

        public ExpressionToSql<T> FullJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser2(expression, blnWithNoLock, "full ");
        }

        public ExpressionToSql<T> GroupBy(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this._sqlBuilder += "\ngroup by ";
            Expression2SqlProvider.GroupBy(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> InnerJoin<T2>(Expression<Func<T, T2, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser(expression, blnWithNoLock, "inner ");
        }

        public ExpressionToSql<T> InnerJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser2(expression, blnWithNoLock, "inner ");
        }

        public ExpressionToSql<T> Insert(Expression<Func<object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this.Clear();
            this._sqlBuilder.IsSingleTable = true;
            this._sqlBuilder.AppendFormat("insert into {0}", this._mainTableName);
            Expression2SqlProvider.Insert(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> Insert(T t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t", "Value cannot be null");
            }

            this.Clear();
            this._sqlBuilder.IsSingleTable = true;
            var refInfo = ReflectionHelper.GetInfo<T>();
            var propertys = refInfo.Properties;
            this._sqlBuilder.AppendFormat("insert into [{0}]", this._mainTableName);
            this._sqlBuilder.AppendFormat("{0}", "(");
            for (int i = 0; i < propertys.Length; i++)
            {
                var property = propertys[i];

                var fieldAttr = refInfo.GetFieldAttr(property);
                if (fieldAttr != null && fieldAttr.InsertIgnore)
                {
                    continue;
                }

                if (i == propertys.Length - 1)
                {
                    this._sqlBuilder.AppendFormat("{0}", property.Name);
                }
                else
                {
                    this._sqlBuilder.AppendFormat("{0}", property.Name + ",");
                }
            }
            this._sqlBuilder.AppendFormat("{0}", ")");
            this._sqlBuilder.AppendFormat("{0}", "Values(");
            for (int i = 0; i < propertys.Length; i++)
            {
                var property = propertys[i];
                var fieldAttr = refInfo.GetFieldAttr(property);
                if (fieldAttr != null && fieldAttr.InsertIgnore)
                {
                    continue;
                }
                string dbParamName = this._sqlBuilder.SqlParser.DbParamPrefix + "param" + i;
                if (i == propertys.Length - 1)
                {
                    this._sqlBuilder.AppendFormat("{0}", dbParamName);
                }
                else
                {
                    this._sqlBuilder.AppendFormat("{0}", dbParamName + ",");
                }

                var fieldName = property.Name;
                var accessor = refInfo.GetAccessor(fieldName);
                if (accessor == null) continue;

                var val = accessor.Get(t);
                if (val == null)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        val = string.Empty;
                    }
                }
                else
                {
                    if (property.PropertyType == typeof(DateTime) && (DateTime)val == DateTime.MinValue)
                    {
                        val = DateTime.Now;
                    }
                }

                this.DbParams.Add(dbParamName, val);
            }

            this._sqlBuilder.AppendFormat("{0}", ")");
            return this;
        }

        public ExpressionToSql<T> Join<T2>(Expression<Func<T, T2, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser(expression, blnWithNoLock);
        }

        public ExpressionToSql<T> Join<T2, T3>(Expression<Func<T2, T3, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser2(expression, blnWithNoLock: blnWithNoLock);
        }

        public ExpressionToSql<T> LeftJoin<T2>(Expression<Func<T, T2, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser(expression, blnWithNoLock, "left ");
        }

        public ExpressionToSql<T> LeftJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser2(expression, blnWithNoLock, "left ");
        }

        public ExpressionToSql<T> Max(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this.Clear();
            Expression2SqlProvider.Max(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> Min(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this.Clear();
            Expression2SqlProvider.Min(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> OrderBy(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this._sqlBuilder += "\norder by ";
            Expression2SqlProvider.OrderBy(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> ThenByDesc(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this._sqlBuilder += ",";
            Expression2SqlProvider.ThenByDesc(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> ThenBy(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this._sqlBuilder += ", ";
            Expression2SqlProvider.ThenBy(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> OrderByDesc(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this._sqlBuilder += "\norder by ";
            Expression2SqlProvider.OrderByDesc(expression.Body, this._sqlBuilder);
            this._sqlBuilder += " desc ";
            return this;
        }

        public ExpressionToSql<T> RightJoin<T2>(Expression<Func<T, T2, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser(expression, blnWithNoLock, "right ");
        }

        public ExpressionToSql<T> RightJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, bool blnWithNoLock = false)
        {
            return JoinParser2(expression, blnWithNoLock, "right ");
        }

        public ExpressionToSql<T> Select(Expression<Func<T, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2>(Expression<Func<T, T2, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3>(Expression<Func<T, T2, T3, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Select<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression = null, bool blnWithNoLock = false)
        {
            return SelectParser(expression, expression == null ? null : expression.Body, blnWithNoLock, typeof(T));
        }

        public ExpressionToSql<T> Sum(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this.Clear();
            Expression2SqlProvider.Sum(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> Update(Expression<Func<object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            this.Clear();
            this._sqlBuilder.IsSingleTable = true;
            this._sqlBuilder.AppendFormat("update {0} set ", this._mainTableName);
            Expression2SqlProvider.Update(expression.Body, this._sqlBuilder);
            return this;
        }

        public ExpressionToSql<T> Update(T t)
        {
            if (t == null)
            {
                throw new ArgumentNullException("t", "Value cannot be null");
            }

            this.Clear();
            var refInfo = ReflectionHelper.GetInfo<T>();
            var propertys = refInfo.Properties;
            this._sqlBuilder.IsSingleTable = true;
            this._sqlBuilder.AppendFormat("update {0} set ", this._mainTableName);
            bool blnPrimaryKey = false;
            string strWhere = string.Empty;
            string strParamName;
            for (int i = 0; i < propertys.Length; i++)
            {
                strParamName = this._sqlBuilder.SqlParser.DbParamPrefix + "param" + i;
                PropertyInfo property = propertys[i];
                var fieldAttr = refInfo.GetFieldAttr(property);
                if (fieldAttr != null && fieldAttr.IsPrimaryKey)
                {
                    blnPrimaryKey = true;
                    strWhere = string.Format(" where {0}={1}", property.Name, strParamName);
                    continue;
                }
                if (fieldAttr != null && fieldAttr.UpdateIgnore)
                {
                    continue;
                }
                if (i == propertys.Length - 1)
                {
                    this._sqlBuilder.AppendFormat(" {0} ", property.Name + "=" + strParamName);
                }
                else
                {
                    this._sqlBuilder.AppendFormat(" {0} ", property.Name + "=" + strParamName);
                }
            }

            if (blnPrimaryKey == false)
            {
                throw new ArgumentNullException("PrimaryKey", "not PrimaryKey");
            }
            this._sqlBuilder.AppendFormat(" {0} ", strWhere);

            for (int i = 0; i < propertys.Length; i++)
            {
                var property = propertys[i];
                var fieldAttr = refInfo.GetFieldAttr(property);
                //if (fieldAttr != null && fieldAttr.IsPrimaryKey)
                //{
                //    continue;
                //}
                if (fieldAttr != null && fieldAttr.UpdateIgnore)
                {
                    continue;
                }
                strParamName = this._sqlBuilder.SqlParser.DbParamPrefix + "param" + i;
                var fieldName = property.Name;
                var accessor = refInfo.GetAccessor(fieldName);
                if (accessor == null) continue;

                var val = accessor.Get(t);
                if (val == null)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        val = string.Empty;
                    }
                }
                else
                {
                    if (property.PropertyType == typeof(DateTime) && (DateTime)val == DateTime.MinValue)
                    {
                        val = DateTime.Now;
                    }
                }

                this.DbParams.Add(strParamName, val);
            }

            return this;
        }

        public ExpressionToSql<T> Where(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            if (expression.Body != null && expression.Body.NodeType == ExpressionType.Constant)
            {
                throw new ArgumentException("Cannot be parse expression", "expression");
            }

            this._sqlBuilder += "\nwhere";
            Expression2SqlProvider.Where(expression.Body, this._sqlBuilder);
            return this;
        }

        private ExpressionToSql<T> JoinParser<T2>(Expression<Func<T, T2, bool>> expression, bool blnWithNoLock = false, string leftOrRightJoin = "")
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            string joinTableName = typeof(T2).Name;
            this._sqlBuilder.SetTableAlias(joinTableName);
            string strAlias = this._sqlBuilder.QueueEnglishWords.Dequeue();
            this._sqlBuilder.JoinTables.Add(strAlias, joinTableName);
            if (_sqlBuilder.JoinTables.Count(t => t.Value.Equals(joinTableName)) > 1)
            {
                this._sqlBuilder.AppendFormat("\n{0}join {1} ", leftOrRightJoin, joinTableName + " " + strAlias);
            }
            else
            {
                this._sqlBuilder.AppendFormat("\n{0}join {1} ", leftOrRightJoin, joinTableName + " " + this._sqlBuilder.GetTableAlias(joinTableName));
            }
            if (blnWithNoLock)
            {
                this._sqlBuilder.AppendFormat(" {0} ", AsNoLock());
            }
            this._sqlBuilder.AppendFormat(" {0} ", "on");
            Expression2SqlProvider.Join(expression.Body, this._sqlBuilder);
            return this;
        }

        private ExpressionToSql<T> JoinParser2<T2, T3>(Expression<Func<T2, T3, bool>> expression, bool blnWithNoLock = false, string leftOrRightJoin = "")
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            string joinTableName = typeof(T3).Name;
            this._sqlBuilder.SetTableAlias(joinTableName);
            string strAlias = this._sqlBuilder.QueueEnglishWords.Dequeue();
            this._sqlBuilder.JoinTables.Add(strAlias, joinTableName);
            if (_sqlBuilder.JoinTables.Count(t => t.Value.Equals(joinTableName)) > 1)
            {
                this._sqlBuilder.AppendFormat("\n{0}join {1} on", leftOrRightJoin, joinTableName + " " + strAlias);
            }
            else
            {
                this._sqlBuilder.AppendFormat("\n{0}join {1} on", leftOrRightJoin,
                    joinTableName + " " + this._sqlBuilder.GetTableAlias(joinTableName));
            }
            if (blnWithNoLock)
            {
                this._sqlBuilder.AppendFormat(" {0} ", AsNoLock());
            }
            Expression2SqlProvider.Join(expression.Body, this._sqlBuilder);
            return this;
        }

        private ExpressionToSql<T> SelectParser(Expression expression, Expression expressionBody, bool blnAsNoLock = false, params Type[] ary)
        {
            this.Clear();
            this._sqlBuilder.IsSingleTable = false;

            if (expressionBody != null && expressionBody.Type == typeof(T))
            {
                throw new ArgumentException("cannot be parse expression", "expression");
            }

            foreach (var item in ary)
            {
                string tableName = item.Name;
                this._sqlBuilder.SetTableAlias(tableName);
            }

            string sql = "select {0}\nfrom " + this._mainTableName + " " + this._sqlBuilder.GetTableAlias(this._mainTableName);
            if (blnAsNoLock)
            {
                sql += AsNoLock();
            }
            if (expression == null)
            {
                this._sqlBuilder.AppendFormat(sql, "*");
            }
            else
            {
                Expression2SqlProvider.Select(expressionBody, this._sqlBuilder);
                this._sqlBuilder.AppendFormat(sql, this._sqlBuilder.SelectFieldsStr);
            }

            return this;
        }

        public ExpressionToSql<T> Take(int intCount)
        {
            if (intCount == 0)
            {
                throw new Exception("Take 方法输入数字有误");
            }
            int intStart = _sqlBuilder.Sql.IndexOf("select", StringComparison.Ordinal);
            _sqlBuilder.Insert(intStart + 6, " top " + intCount + " ");
            return this;
        }

        /// <summary>AsNoLock 适用于SqlServer
        ///
        /// </summary>
        /// <returns></returns>
        private string AsNoLock()
        {
            if (this.SqlType == SqlType.SqlServer)
            {
                return " with(nolock) ";
            }
            return "";
        }

        public ExpressionToSql<T> First(Expression<Func<T, object>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression", "Value cannot be null");
            }

            Expression2SqlProvider.First(expression.Body, this._sqlBuilder);
            return this;
        }

        /// <summary>执行查询
        ///
        /// </summary>
        /// <returns>返回实体对象</returns>
        public List<T> ToList2()
        {
            if (dbHelper != null)
            {
                dbHelper.CreateCommand(this.Sql);
                foreach (var item in this.DbParams)
                {
                    dbHelper.AddParameter(item.Key, item.Value);
                }
                return MappingUntilTool.DataReaderToList<T>(typeof(T), dbHelper.ExecuteReader(), "*");
            }
            return null;
        }

        /// <summary>执行查询
        ///
        /// </summary>
        /// <returns>返回实体对象</returns>

        public List<T> ToList()
        {
            if (dbHelper != null)
            {
                dbHelper.CreateCommand(this.Sql);
                foreach (var item in this.DbParams)
                {
                    dbHelper.AddParameter(item.Key, item.Value);
                }
                return Mapping.ReaderToObjectList<T>((DbDataReader)dbHelper.ExecuteReader());
            }
            return null;
        }

        /// <summary>执行查询
        ///
        /// </summary>
        /// <returns>返回第一行第一列值</returns>
        public long ToLong()
        {
            if (dbHelper != null)
            {
                dbHelper.CreateCommand(this.Sql);
                foreach (var item in this.DbParams)
                {
                    dbHelper.AddParameter(item.Key, item.Value);
                }
                return long.Parse(dbHelper.ExecuteScalar());
            }
            return 0;
        }

        /// <summary>执行增删改
        ///
        /// </summary>
        /// <returns>执行成功返回True，否则返回失败</returns>
        public bool ToBool()
        {
            if (dbHelper != null)
            {
                dbHelper.CreateCommand(this.Sql);
                foreach (var item in this.DbParams)
                {
                    dbHelper.AddParameter(item.Key, item.Value);
                }
                return dbHelper.ExecuteNonQuery();
            }
            return false;
        }

        /// <summary>执行查询
        ///
        /// </summary>
        /// <returns>返回表</returns>
        public DataTable ToDataTable()
        {
            if (dbHelper != null)
            {
                dbHelper.CreateCommand(this.Sql);
                foreach (var item in this.DbParams)
                {
                    dbHelper.AddParameter(item.Key, item.Value);
                }
                return dbHelper.ExecuteQuery();
            }
            return null;
        }
    }
}