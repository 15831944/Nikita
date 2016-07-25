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
using Nikita.Base.Define;
using Nikita.Core.WinForm;
using Nikita.DataAccess4DBHelper;

namespace Nikita.Assist.CodeMaker
{
    public partial class FrmNestQueryPreview
    {
        #region ����������
        /// <summary>��ʾ�ؼ�
        /// 
        /// </summary> 
        MasterControl _masterDetail;

        /// <summary>��ʾ�ؼ�
        /// 
        /// </summary> 
        DataSet m_dataSet;


        /// <summary>��ѯ����
        /// 
        /// </summary> 
        public DataTable QueryDataTable { get; private set; }

        #endregion

        #region ���캯��
        public FrmNestQueryPreview(DataSet dataSet)
        {
            InitializeComponent();
            m_dataSet = dataSet;
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

            DataTable dtColumns = new DataTable();
            dtColumns.Columns.Add("Name");
            dtColumns.Columns.Add("DataType");
            for (int i = 0; i < m_dataSet.Tables[0].Columns.Count; i++)
            {
                DataRow dr = dtColumns.NewRow();
                dr["Name"] = m_dataSet.Tables[0].Columns[i].ToString();
                dr["DataType"] = m_dataSet.Tables[0].Columns[i].DataType.Name;
                dtColumns.Rows.Add(dr);
            }
            CheckedListBoxHelper.BindCheckedListBox(lstQueryFileds, dtColumns, "Name", "Name");
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
            if (m_dataSet.Tables.Count == 3)
            {
                _masterDetail = new MasterControl(m_dataSet, ControlType.OutSide);
            }
            else if (m_dataSet.Tables.Count == 2)
            {
                _masterDetail = new MasterControl(m_dataSet, ControlType.Middle);
            }
            panelView.Controls.Add(_masterDetail);
        }

        #endregion

        #region �����¼�
        private void btnSet_Click(object sender, EventArgs e)
        {
            NestQueryParameter parameter = new NestQueryParameter();
       
            #region strQueryColumns
            string strQueryColumns = string.Empty;
            foreach (var item in lstQueryFileds.CheckedItems)
            {
                var dr = item as DataRowView;
                if (dr != null)
                    strQueryColumns = strQueryColumns + dr.Row["Name"] + ",";
            }
            strQueryColumns = strQueryColumns.TrimEnd(',');
            #endregion

            parameter.QueryColumns = strQueryColumns;
            parameter.CodeGenType = CodeGenType.WinFromNestQuery;
            FrmControlSetting frmSetting = new FrmControlSetting(parameter);
            if (frmSetting.ShowDialog() == DialogResult.OK)
            {
                QueryDataTable = frmSetting.QueryDataTable;
                this.DialogResult = DialogResult.OK;
            }
        }
        #endregion
    }
}
