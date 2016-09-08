using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Nikita.WinForm.ExtendControl;
using Nikita.WinForm.ExtendControl.WinControls; 
using WeifenLuo.WinFormsUI.Docking;

namespace Nikita.Platform.BugClose
{
    public partial class FrmBugCloseMenu : DockContentEx
    { 
        public OutlookBar OutlookBarMenu
        {
            get
            {
                return OutLookBarMenu;
            }
        }

        public FrmBugCloseMenu()
        { 
            InitializeComponent(); 
            OutlookBarBand outlookMenu = new OutlookBarBand("��������")
            {
                SmallImageList = imageList1,
                LargeImageList = imageList1
            };
            outlookMenu.Items.Add(new OutlookBarItem("ȱ���ֵ����", 0));
            outlookMenu.Items.Add(new OutlookBarItem("������Ŀ", 0));
            outlookMenu.Items.Add(new OutlookBarItem("��Ŀ�汾", 0));
            outlookMenu.Items.Add(new OutlookBarItem("��Ŀģ��", 0));

            OutlookBarBand outlookMenu2 = new OutlookBarBand("Bug����")
            {
                SmallImageList = imageList1,
                LargeImageList = imageList1
            };

            outlookMenu2.Items.Add(new OutlookBarItem("�Bug", 1));
            outlookMenu2.Items.Add(new OutlookBarItem("����Bug", 2));
            outlookMenu2.Items.Add(new OutlookBarItem("�ҵĴ���", 3));
            outlookMenu2.Items.Add(new OutlookBarItem("�������", 4));
            outlookMenu2.Items.Add(new OutlookBarItem("�ҵķ���", 5));
            outlookMenu2.Items.Add(new OutlookBarItem("Bugͳ��", 5));
            OutlookBarBand outlookMenu3 = new OutlookBarBand("�������")
            {
                SmallImageList = imageList1,
                LargeImageList = imageList1
            };


            outlookMenu3.Items.Add(new OutlookBarItem("����������", 1));
            outlookMenu3.Items.Add(new OutlookBarItem("��������", 2));
            
            OutLookBarMenu.Bands.Add(outlookMenu);
            OutLookBarMenu.Bands.Add(outlookMenu2);
            OutLookBarMenu.Bands.Add(outlookMenu3);
        }

        void OnOutlookBarItemClicked(OutlookBarBand band, OutlookBarItem item)
        {
            string message = "Item : " + item.Text + " was clicked...";
            MessageBox.Show(message);
        }

         
    }
}