using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMember : Form
    {
        public FrmMember()
        {
            InitializeComponent();
        }

        private void FrmMember_Load(object sender, EventArgs e)
        {
            getAllMemberToListView();

            tbMemberId.Clear();
            tbMemberName.Clear();
            tbMemberPhone.Clear();

            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private void getAllMemberToListView()
        {
            // Connect String เพื่อติดต่อไปยังฐานข้อมูล
            string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    // ดึงข้อมูลจากตารางสมาชิก
                    string strSQL = "SELECT memberId, memberPhone, memberName, memberScore FROM [coffee_cafe_db].[dbo].[member_tb]";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // ตั้งค่า ListView
                        lvShowAllMember.Items.Clear();
                        lvShowAllMember.Columns.Clear();
                        lvShowAllMember.FullRowSelect = true;
                        lvShowAllMember.View = View.Details;

                        // เพิ่มหัวข้อคอลัมน์
                        lvShowAllMember.Columns.Add("รหัสสมาชิก", 80, HorizontalAlignment.Left);
                        lvShowAllMember.Columns.Add("เบอร์โทรศัพท์", 100, HorizontalAlignment.Left);
                        lvShowAllMember.Columns.Add("ชื่อสมาชิก", 140, HorizontalAlignment.Left);
                        lvShowAllMember.Columns.Add("คะแนนสะสม", 95, HorizontalAlignment.Right);

                        // วนลูปเพิ่มข้อมูลใน ListView
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(dataRow["memberId"].ToString());
                            item.SubItems.Add(dataRow["memberPhone"].ToString());
                            item.SubItems.Add(dataRow["memberName"].ToString());
                            item.SubItems.Add(dataRow["memberScore"].ToString());

                            lvShowAllMember.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือติดต่อ IT : " + ex.Message);
                }
            }
        }

    }
}
