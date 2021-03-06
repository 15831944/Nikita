using Nikita.Base.Define;
using Nikita.Base.IDAL;
using Nikita.Core;

using Nikita.Platform.BugClose.Model;
using Nikita.WinForm.ExtendControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Nikita.Platform.BugClose
{
    /// <summary>说明:FrmBseProjectMasterDetailQuery
    /// 作者:Luhm
    /// 最后修改人:
    /// 最后修改时间:
    /// 创建时间:2016/6/4 18:09:29
    /// </summary>
    public partial class FrmBseProjectMasterDetailQuery : DockContentEx
    {
        #region 常量、变量

        /// <summary>主表操作类
        ///
        /// </summary>
        private IBseDAL<BseProject> m_BseProjectDAL;

        /// <summary>子表操作类
        ///
        /// </summary>
        private IBseDAL<BseProjectVersion> m_BseProjectVersionDAL;

        /// <summary>主表绑定集合
        ///
        /// </summary>
        private List<BseProject> m_lstBseProject;

        /// <summary>子表绑定集合
        ///
        /// </summary>
        private List<BseProjectVersion> m_lstBseProjectVersion;

        /// <summary>Master列表下拉框绑定数据源
        ///
        /// </summary>
        private DataSet m_dsMasterGridSource;

        /// <summary>Detail列表下拉框绑定数据源
        ///
        /// </summary>
        private DataSet m_dsDetailGridSource;

        #endregion 常量、变量

        #region 构造函数

        /// <summary>构造函数
        ///
        /// </summary>
        public FrmBseProjectMasterDetailQuery()
        {
            InitializeComponent();
            cboFilterType.SelectedIndex = 2;
            m_BseProjectDAL = GlobalHelp.GetResolve<IBseDAL<BseProject>>();
            m_BseProjectVersionDAL = GlobalHelp.GetResolve<IBseDAL<BseProjectVersion>>();
            m_lstBseProject = new List<BseProject>();
            DoInitData();
            DoInitMasterGridSource();
            DoInitDetailGridSource();

            #region 主表列表绑定字段

            this.gridmrzProjectID.AspectGetter = x => ((BseProject)x).ProjectID;
            this.gridmrzCategory.AspectGetter = delegate (object x) { return GetMasterCategory(((BseProject)x).Category); };
            this.gridmrzName.AspectGetter = x => ((BseProject)x).Name;
            this.gridmrzStatus.AspectGetter = x => ((BseProject)x).Status;
            this.gridmrzOnLevel.AspectGetter = delegate (object x) { return GetMasterOnLevel(((BseProject)x).OnLevel); };
            this.gridmrzRemark.AspectGetter = x => ((BseProject)x).Remark;
            this.gridmrzSort.AspectGetter = x => ((BseProject)x).Sort;
            this.gridmrzDeptId.AspectGetter = x => ((BseProject)x).DeptId;
            this.gridmrzCompanyID.AspectGetter = x => ((BseProject)x).CompanyID;
            this.gridmrzCreateDate.AspectGetter = x => ((BseProject)x).CreateDate;
            this.gridmrzCreateUser.AspectGetter = x => ((BseProject)x).CreateUser;
            this.gridmrzEditDate.AspectGetter = x => ((BseProject)x).EditDate;
            this.gridmrzEditUser.AspectGetter = x => ((BseProject)x).EditUser;

            #endregion 主表列表绑定字段

            #region 明细表列表绑定字段

            this.gridDetailmrzVersionID.AspectGetter = x => ((BseProjectVersion)x).VersionID;
            this.gridDetailmrzProjectID.AspectGetter = delegate (object x) { return GetDetailProjectID(((BseProjectVersion)x).ProjectID); };
            this.gridDetailmrzName.AspectGetter = x => ((BseProjectVersion)x).Name;
            this.gridDetailmrzStatus.AspectGetter = x => ((BseProjectVersion)x).Status;
            this.gridDetailmrzRemark.AspectGetter = x => ((BseProjectVersion)x).Remark;
            this.gridDetailmrzSort.AspectGetter = x => ((BseProjectVersion)x).Sort;
            this.gridDetailmrzDeptId.AspectGetter = x => ((BseProjectVersion)x).DeptId;
            this.gridDetailmrzCompanyID.AspectGetter = x => ((BseProjectVersion)x).CompanyID;
            this.gridDetailmrzCreateDate.AspectGetter = x => ((BseProjectVersion)x).CreateDate;
            this.gridDetailmrzCreateUser.AspectGetter = x => ((BseProjectVersion)x).CreateUser;
            this.gridDetailmrzEditDate.AspectGetter = x => ((BseProjectVersion)x).EditDate;
            this.gridDetailmrzEditUser.AspectGetter = x => ((BseProjectVersion)x).EditUser;

            #endregion 明细表列表绑定字段
        }

        #endregion 构造函数

        #region 基础事件

        /// <summary>查询
        ///
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void btnQuery_Click(object sender, EventArgs e)
        {
            DoQueryData();
        }

        /// <summary>执行命令
        ///
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Command_Click(object sender, EventArgs e)
        {
            ToolStripItem cmdItem = sender as ToolStripItem;
            if (cmdItem != null)
            {
                switch (cmdItem.Name.Trim())
                {
                    case "cmdRefres":
                        DoQueryData();
                        break;

                    case "cmdNew":
                        DoNew();
                        break;

                    case "cmdEdit":
                        DoEdit();
                        break;

                    case "cmdDelete":
                        DoDeleteOrCancel(EntityOperationType.删除);
                        break;

                    case "cmdCancel":
                        DoDeleteOrCancel(EntityOperationType.作废);
                        break;
                }
            }
        }

        /// <summary>分页事件
        ///
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Pager_PageChanged(object sender, EventArgs e)
        {
            DoQueryData();
        }

        /// <summary>主表选中事件
        ///
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void objListViewMaster_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (objListViewMaster.SelectedObjects.Count == 0)
            {
                return;
            }
            BseProject model = objListViewMaster.SelectedObjects[0] as BseProject;
            if (model != null)
            {
                m_lstBseProjectVersion = m_BseProjectVersionDAL.GetListArray("ProjectID=" + model.ProjectID + "");
                objListViewDetail.SetObjects(m_lstBseProjectVersion);
                objListViewDetail.Refresh();
            }
        }

        /// <summary>通用查询
        ///
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            this.objListViewMaster.TimedFilter(txtFilter.Text);
        }

        /// <summary>双击修改
        ///
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void objListViewMaster_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            Command_Click(cmdEdit, null);
        }

        #endregion 基础事件

        #region 基础方法

        /// <summary>执行查询
        ///
        /// </summary>
        private void DoQueryData()
        {
            try
            {
                btnQuery.Enabled = false;
                string strWhere = GetSearchSql();
                m_lstBseProject = m_BseProjectDAL.GetListArray("*", "ProjectID", "ASC", Pager.PageSize, Pager.PageIndex, strWhere);
                Pager.RecordCount = m_BseProjectDAL.CalcCount(strWhere);
                Pager.InitPageInfo();
                objListViewMaster.SetObjects(m_lstBseProject);
                if (m_lstBseProject.Count > 0)
                {
                    objListViewMaster.SelectedIndex = 0;
                }
            }
            finally
            {
                btnQuery.Enabled = true;
            }
        }

        /// <summary>根据查询条件构造查询语句
        ///
        /// </summary>
        /// <returns>查询条件</returns>
        private string GetSearchSql()
        {
            SearchCondition condition = new SearchCondition();
            condition.AddCondition("Name", this.txtQueryName.Text, SqlOperator.Like);
            return condition.BuildConditionSql().Replace("Where", "");
        }

        /// <summary>删除/作废
        ///
        /// </summary>
        /// <param name="operationType">操作类型</param>
        private void DoDeleteOrCancel(EntityOperationType operationType)
        {
            string strMsg = CheckSelect(operationType);
            if (strMsg != string.Empty)
            {
                MessageBox.Show(strMsg);
                return;
            }
            DialogResult diaComfirmResult = MessageBox.Show(string.Format("{0}主表信息，会连同明细一起{0}", operationType), @"警告",
                MessageBoxButtons.YesNo);
            if (diaComfirmResult != DialogResult.Yes)
            {
                return;
            }
            IList objList = objListViewMaster.SelectedObjects;
            IList lstSelectionIds = objList.Cast<BseProject>().Select(t => t.ProjectID).ToList();
            string strIds = lstSelectionIds.Cast<string>().Aggregate(string.Empty, (current, strId) => current + (strId + ","));
            strIds = strIds.TrimEnd(',');
            var blnReturn = operationType == EntityOperationType.删除
                ? m_BseProjectDAL.DeleteByCond("ProjectID in (" + strIds + ")")
                : m_BseProjectDAL.Update("Status =0", "ProjectID in (" + strIds + ")");
            if (blnReturn)
            {
                MessageBox.Show(string.Format("{0}成功", operationType));
                objListViewMaster.RemoveObjects(objList);
            }
            else
            {
                MessageBox.Show(string.Format("{0}失败", operationType));
            }
        }

        /// <summary>编辑
        ///
        /// </summary>
        private void DoEdit()
        {
            string strMsg = CheckSelect(EntityOperationType.修改);
            if (strMsg != string.Empty)
            {
                MessageBox.Show(strMsg);
                return;
            }
            BseProject model = objListViewMaster.SelectedObjects[0] as BseProject;
            if (model != null)
            {
                FrmBseProjectMasterDetailDialog frmDialog = new FrmBseProjectMasterDetailDialog(model, m_lstBseProject, m_lstBseProjectVersion, m_dsDetailGridSource);
                if (frmDialog.ShowDialog() == DialogResult.OK)
                {
                    m_lstBseProject = frmDialog.ListBseProject;
                    m_lstBseProjectVersion = frmDialog.ListBseProjectVersion;
                    if (m_lstBseProject != null)
                    {
                        objListViewMaster.SetObjects(m_lstBseProject);
                        objListViewMaster.Refresh();
                    }
                    if (m_lstBseProjectVersion != null)
                    {
                        objListViewDetail.SetObjects(m_lstBseProjectVersion);
                        objListViewDetail.Refresh();
                    }
                }
            }
        }

        /// <summary>新增
        ///
        /// </summary>
        private void DoNew()
        {
            FrmBseProjectMasterDetailDialog frmDialog = new FrmBseProjectMasterDetailDialog(null, m_lstBseProject, m_lstBseProjectVersion, m_dsDetailGridSource);
            if (frmDialog.ShowDialog() == DialogResult.OK)
            {
                m_lstBseProject = frmDialog.ListBseProject;
                m_lstBseProjectVersion = frmDialog.ListBseProjectVersion;
                if (m_lstBseProject != null)
                {
                    objListViewMaster.SetObjects(m_lstBseProject);
                    objListViewMaster.Refresh();
                }
                if (m_lstBseProjectVersion != null)
                {
                    objListViewDetail.SetObjects(m_lstBseProjectVersion);
                    objListViewDetail.Refresh();
                }
            }
        }

        /// <summary>检查选择
        ///
        /// </summary>
        /// <param name="operationType">操作说明</param>
        /// <returns>返回提示信息</returns>
        private string CheckSelect(EntityOperationType operationType)
        {
            string strMsg = string.Empty;
            if (objListViewMaster.SelectedObjects.Count == 0)
            {
                strMsg = string.Format("请选择要{0}的行数据", operationType);
            }
            return strMsg;
        }

        /// <summary>初始化绑定下拉框等
        ///
        /// </summary>
        private void DoInitData()
        {
        }

        /// <summary>初始主表数据源
        ///
        /// </summary>
        private void DoInitMasterGridSource()
        {
            const string strBindSql = "SELECT   Name,value FROM [BseDictionary] WHERE ParentID=19;SELECT   Name,value FROM [BseDictionary] WHERE ParentID=22;SELECT 'gridmrzCategory ','gridmrzOnLevel '";
            BindClass bindClass = new BindClass()
            {
                SqlType = SqlType.SqlServer,
                BindSql = strBindSql
            };
            m_dsMasterGridSource = BindSourceHelper.GetBindSourceDataSet(bindClass, GlobalHelp.Conn);
        }

        #endregion 基础方法

        /// <summary>初始化明细数据源
        ///
        /// </summary>
        private void DoInitDetailGridSource()
        {
            const string strBindSql = "SELECT ProjectID,Name From BseProject;SELECT 'gridDetailmrzProjectID '";
            BindClass bindClass = new BindClass()
            {
                SqlType = SqlType.SqlServer,
                BindSql = strBindSql
            };
            m_dsDetailGridSource = BindSourceHelper.GetBindSourceDataSet(bindClass, GlobalHelp.Conn);
        }

        #region 主表绑定方法

        private string GetMasterCategory(object objCategory)
        {
            return m_dsMasterGridSource.Tables["gridmrzCategory"].Select("Value = '" + objCategory + "'")[0]["Name"].ToString();
        }

        private string GetMasterOnLevel(object objOnLevel)
        {
            return m_dsMasterGridSource.Tables["gridmrzOnLevel"].Select("Value = '" + objOnLevel + "'")[0]["Name"].ToString();
        }

        #endregion 主表绑定方法

        #region 明细绑定方法

        private string GetDetailProjectID(object objProjectID)
        {
            return m_dsDetailGridSource.Tables["gridDetailmrzProjectID"].Select("ProjectID = '" + objProjectID + "'")[0]["Name"].ToString();
        }

        #endregion 明细绑定方法
    }
}