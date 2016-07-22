using System.Collections.Generic;
using System;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using Nikita.WinForm.ExtendControl;
using System.Reflection;
using Nikita.Assist.CodeMaker.CodeMakerDemoForm;
//using Nikita.Assist.CodeMaker.CodeMakerDemoForm;
using Nikita.Core;
using Nikita.DataAccess4DBHelper;

namespace Nikita.Assist.CodeMaker
{
    public partial class FrmNestQuery
    {
        #region ����������
        /// <summary>��ʾ�ؼ�
        /// 
        /// </summary> 
        MasterControl _masterDetail;
        #endregion

        #region ���캯��
        public FrmNestQuery()
        {
            InitializeComponent();
        }
        #endregion

        #region �����¼�
        public void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        #endregion

        #region ��������

        /// <summary>��������
        /// 
        /// </summary>
        public void LoadData()
        {
            Clear();
            CreateMasterDetailView();
        }

        /// <summary>���
        /// 
        /// </summary>
        public void Clear()
        {
            panelView.Controls.Clear();
            _masterDetail = null;
            Refresh();
        }

        /// <summary>�������ӹ�ϵ
        /// 
        /// </summary>
        public void CreateMasterDetailView()
        {
            var oDataSet = GetData();
            _masterDetail = new MasterControl(oDataSet, ControlType.Middle);
            panelView.Controls.Add(_masterDetail);
        }

        /// <summary> ��ȡ����Դ
        /// 
        /// </summary>
        /// <returns>DataSet</returns>
        public DataSet GetData()
        {
            IDbHelper dbHelper = GlobalHelpDemoForm.GetDataAccessHelperDemo();
            string strWhere = GetSearchSql();
            string strSql = "SELECT * INTO #TempTable FROM  dbo.Sys_Departments  WHERE  " + strWhere + "" +
                                       "  SELECT  * FROM   #TempTable " +
                                       "  SELECT  b.DepID,a.* FROM   dbo.Sys_Users a " +
                                       "  INNER JOIN  #TempTable b ON a.KeyId =b.UserID" +

              //+ 
                //"  SELECT   b.UserID ,c.* FROM  Sys_Users  a" +
                //"  INNER JOIN dbo.Sys_UserRoles b ON a.KeyId=b.UserID" +
                //"  INNER JOIN dbo.Sys_Roles c ON c.KeyId=b.RoleID WHERE  UserID=15 "

                     "  Drop Table  #TempTable "
;
            dbHelper.CreateCommand(strSql);
            DataSet oDataSet = dbHelper.ExecuteQueryDataSet();
            for (int i = 0; i < oDataSet.Tables.Count; i++)
            {
                oDataSet.Tables[i].TableName = "T" + (i + 1);
            }
            //���Ƕ�Ӧ��ϵ��ʱ����������Ψһ
            oDataSet.Relations.Add("1", oDataSet.Tables["T1"].Columns["KeyId"], oDataSet.Tables["T2"].Columns["DepID"]);
            //oDataSet.Relations.Add("2", oDataSet.Tables["T2"].Columns["KeyId"], oDataSet.Tables["T3"].Columns["UserId"]);
            return oDataSet;
        }

        /// <summary>���ݲ�ѯ���������ѯ���
        ///
        /// </summary>
        /// <returns>��ѯ����</returns>
        private string GetSearchSql()
        {
            SearchCondition condition = new SearchCondition();
            condition.AddCondition("UserName", this.txtDepartmentName.Text, SqlOperator.Like);
            return condition.BuildConditionSql().Replace("Where", "");
        }

        #endregion
    }
}
