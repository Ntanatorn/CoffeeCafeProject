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
        private int memberId;
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
            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string query = "SELECT memberId, memberPhone, memberName FROM member_tb";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        lvShowAllMember.Items.Clear();
                        lvShowAllMember.Columns.Clear();

                        lvShowAllMember.View = View.Details;
                        lvShowAllMember.FullRowSelect = true;

                        lvShowAllMember.Columns.Add("รหัสสมาชิก", 80, HorizontalAlignment.Left);
                        lvShowAllMember.Columns.Add("เบอร์โทรสมาชิก", 120, HorizontalAlignment.Left);
                        lvShowAllMember.Columns.Add("ชื่อสมาชิก", 150, HorizontalAlignment.Left);

                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["memberId"].ToString());
                            item.SubItems.Add(reader["memberPhone"].ToString());
                            item.SubItems.Add(reader["memberName"].ToString());

                            lvShowAllMember.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void tbMemberPhone_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
            if (!char.IsControl(e.KeyChar) && tb.Text.Length >= 10) e.Handled = true;
        }
        private void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void btSave_Click(object sender, EventArgs e)
        {
            if (tbMemberPhone.Text.Length == 0)
            {
                showWarningMSG("กรุณาป้อนเบอร์โทรสมาชิกด้วย...");
                return;
            }
            else if (tbMemberName.Text.Length == 0)
            {
                showWarningMSG("กรุณาป้อนชื่อสมาชิกด้วย...");
                return;
            }

            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                    string strSql = "INSERT INTO member_tb (memberPhone, memberName, memberScore) VALUES (@memberPhone, @memberName, @memberScore)";

                    using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                    {
                        sqlCommand.Parameters.Add("@memberPhone", SqlDbType.NVarChar, 50).Value = tbMemberPhone.Text;
                        sqlCommand.Parameters.Add("@memberName", SqlDbType.NVarChar, 100).Value = tbMemberName.Text;
                        sqlCommand.Parameters.Add("@memberScore", SqlDbType.Int).Value = 0;

                        sqlCommand.ExecuteNonQuery();
                        sqlTransaction.Commit();

                        MessageBox.Show("บันทึกสมาชิกเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        getAllMemberToListView();

                        tbMemberPhone.Clear();
                        tbMemberName.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือ ติดต่อ IT: " + ex.Message);
                }
            }
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (tbMemberPhone.Text.Length == 0)
            {
                showWarningMSG("กรุณาป้อนเบอร์โทรสมาชิกด้วย...");
                return;
            }
            else if (tbMemberName.Text.Length == 0)
            {
                showWarningMSG("กรุณาป้อนชื่อสมาชิกด้วย...");
                return;
            }
            else if (string.IsNullOrEmpty(tbMemberId.Text) || !int.TryParse(tbMemberId.Text, out int memberId))
            {
                showWarningMSG("กรุณาเลือกรายการสมาชิกที่ต้องการแก้ไข");
                return;
            }

            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                    string strSql = "UPDATE member_tb SET " +
                                    "memberPhone = @memberPhone, " +
                                    "memberName = @memberName " +
                                    "WHERE memberId = @memberId";

                    using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                    {
                        sqlCommand.Parameters.Add("@memberPhone", SqlDbType.NVarChar, 50).Value = tbMemberPhone.Text;
                        sqlCommand.Parameters.Add("@memberName", SqlDbType.NVarChar, 100).Value = tbMemberName.Text;
                        sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = int.Parse(tbMemberId.Text);

                        sqlCommand.ExecuteNonQuery();
                        sqlTransaction.Commit();

                        MessageBox.Show("อัปเดตข้อมูลสมาชิกเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        getAllMemberToListView();

                        tbMemberId.Clear();
                        tbMemberPhone.Clear();
                        tbMemberName.Clear();

                        btSave.Enabled = true;
                        btUpdate.Enabled = false;
                        btDelete.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือ ติดต่อ IT: " + ex.Message);
                }
            }
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbMemberId.Text))
            {
                MessageBox.Show("กรุณาเลือกรายการสมาชิกก่อนลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(tbMemberId.Text, out int memberId))
            {
                MessageBox.Show("รหัสสมาชิกไม่ถูกต้อง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("คุณแน่ใจที่จะลบสมาชิกนี้หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

                using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSql = "DELETE FROM member_tb WHERE memberId = @memberId";

                        using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("ลบข้อมูลสมาชิกเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMemberToListView();

                            tbMemberId.Clear();
                            tbMemberPhone.Clear();
                            tbMemberName.Clear();

                            btSave.Enabled = true;
                            btUpdate.Enabled = false;
                            btDelete.Enabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }
            }
        }
        private void lvShowAllMember_ItemActivate(object sender, EventArgs e)
        {
            if (lvShowAllMember.SelectedItems.Count > 0)
            {
                int memberId = int.Parse(lvShowAllMember.SelectedItems[0].SubItems[0].Text);

                LoadMemberDataToTextBox(memberId);

                btSave.Enabled = false;
                btUpdate.Enabled = true;
                btDelete.Enabled = true;
            }
            else
            {
                MessageBox.Show("กรุณาเลือกรายการสมาชิกที่ต้องการ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadMemberDataToTextBox(int memberId)
        {
            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string query = "SELECT * FROM member_tb WHERE memberId = @memberId";

                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@memberId", memberId);

                        SqlDataReader reader = sqlCommand.ExecuteReader();
                        if (reader.Read())
                        {
                            tbMemberId.Text = reader["memberId"].ToString();
                            tbMemberPhone.Text = reader["memberPhone"].ToString();
                            tbMemberName.Text = reader["memberName"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            getAllMemberToListView();

            tbMemberId.Clear();
            tbMemberName.Clear();
            tbMemberPhone.Clear();

            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
