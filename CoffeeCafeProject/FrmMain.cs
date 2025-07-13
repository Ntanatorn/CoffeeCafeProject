using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMain : Form
    {
        float[] menuPrice = new float[10];
        int memberId = 0;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void btMenu_Click(object sender, EventArgs e)
        {


            FrmMenu frmMenu = new FrmMenu();
            frmMenu.FormClosed += (s, args) =>
            {
                this.Show();       // แสดง FrmMain กลับมา
                resetForm();       // โหลดหน้าจอ FrmMain ใหม่
            };

            frmMenu.ShowDialog();
        }

        private void btMember_Click(object sender, EventArgs e)
        {
            FrmMember frmMember = new FrmMember();
            frmMember.ShowDialog();
        }

        private void resetForm()
        {
            memberId = 0;
            // ไม่เลือก radio button
            rdMenberNo.Checked = false;
            rdMemberYes.Checked = false;

            // ล้างและปิดการใช้งานช่องเบอร์โทร
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false;

            // ตั้งชื่อสมาชิกเป็นข้อความ placeholder
            tbMemberName.Text = "(ชื่อสมาชิก)";

            // คะแนนและยอดชำระเริ่มต้น
            lbMemberScore.Text = "0";
            lbOrderPay.Text = "0.00";

            // ล้างรายการสั่งซื้อ
            lvOrderMenu.Items.Clear();
            lvOrderMenu.Columns.Clear();
            lvOrderMenu.FullRowSelect = true;
            lvOrderMenu.View = View.Details;
            lvOrderMenu.Columns.Add("ชื่อเมนู", 150, HorizontalAlignment.Left);
            lvOrderMenu.Columns.Add("ราคา", 80, HorizontalAlignment.Left);

            // โหลดเมนูจากฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string strSQL = "SELECT menuName, menuPrice, menuImage FROM [coffee_cafe_db].[dbo].[menu_tb]";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        PictureBox[] pbMenuImage = { pbMenu1, pbMenu2, pbMenu3, pbMenu4, pbMenu5, pbMenu6, pbMenu7, pbMenu8, pbMenu9, pbMenu10 };
                        Button[] btMenuName = { btMenu1, btMenu2, btMenu3, btMenu4, btMenu5, btMenu6, btMenu7, btMenu8, btMenu9, btMenu10 };

                        // ล้างค่าทั้งหมดก่อน
                        for (int i = 0; i < 10; i++)
                        {
                            btMenuName[i].Text = "Menu";
                            pbMenuImage[i].Image = Properties.Resources.menu;
                        }

                        // โหลดข้อมูลใหม่
                        int count = Math.Min(dataTable.Rows.Count, 10);

                        for (int i = 0; i < count; i++)
                        {
                            btMenuName[i].Text = dataTable.Rows[i]["menuName"].ToString();
                            float.TryParse(dataTable.Rows[i]["menuPrice"].ToString(), out menuPrice[i]);

                            if (dataTable.Rows[i]["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataTable.Rows[i]["menuImage"];
                                using (var ms = new MemoryStream(imgByte))
                                {
                                    pbMenuImage[i].Image = Image.FromStream(ms);
                                }
                            }
                            else
                            {
                                pbMenuImage[i].Image = Properties.Resources.menu;
                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือติดต่อ IT: " + ex.Message);
                }
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            resetForm();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            resetForm();
        }

        private void rdMenberNo_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false;
            tbMemberName.Text = "(ชื่อสมาชิก)";
            lbMemberScore.Text = "0";
            memberId = 0;
        }

        private void rdMemberYes_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = true;
            tbMemberName.Text = "(ชื่อสมาชิก)";
            lbMemberScore.Text = "0";
        }
        private void RecalculateScore()
        {
            if (tbMemberName.Text != "(ชื่อสมาชิก)")
            {
                int countMenu = lvOrderMenu.Items.Count;
                int newScore = countMenu; // สมมติ 1 เมนู = 1 แต้ม
                lbMemberScore.Text = newScore.ToString();
            }
            else
            {
                lbMemberScore.Text = "0";
            }
        }
        private void tbMemberPhone_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        string strSQL = "SELECT memberId, memberPhone, memberName, memberScore FROM member_tb WHERE memberPhone = @memberPhone";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@memberPhone", SqlDbType.VarChar, 50).Value = tbMemberPhone.Text;

                            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand))
                            {
                                DataTable dataTable = new DataTable();
                                dataAdapter.Fill(dataTable);

                                if (dataTable.Rows.Count == 1)
                                {
                                    tbMemberName.Text = dataTable.Rows[0]["memberName"].ToString();
                                    lbMemberScore.Text = dataTable.Rows[0]["memberScore"].ToString();
                                    memberId = int.Parse(dataTable.Rows[0]["memberId"].ToString());

                                    RecalculateScore();  // เรียกฟังก์ชันตรงนี้หลังโหลดสมาชิกเสร็จ
                                }
                                else
                                {
                                    MessageBox.Show("เบอร์โทรนี้ไม่มีในระบบ กรุณากรอกเบอร์โทรใหม่ให้ถูกต้อง", "ข้อมูลไม่พบ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void AddMenuToOrder(int index)
        {
            Button[] menuButtons = { btMenu1, btMenu2, btMenu3, btMenu4, btMenu5, btMenu6, btMenu7, btMenu8, btMenu9, btMenu10 };

            if (menuButtons[index].Text != "Menu")
            {
                // เพิ่มเมนูลงใน ListView
                ListViewItem item = new ListViewItem(menuButtons[index].Text);
                item.SubItems.Add(menuPrice[index].ToString("0.00"));
                lvOrderMenu.Items.Add(item);

                // เพิ่มแต้มเฉพาะเมื่อเป็นสมาชิก
                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    if (int.TryParse(lbMemberScore.Text, out int score))
                    {
                        score += 1;
                        lbMemberScore.Text = score.ToString();
                    }
                }

                // เพิ่มยอดรวม
                if (float.TryParse(lbOrderPay.Text, out float total))
                {
                    total += menuPrice[index];
                    lbOrderPay.Text = total.ToString("0.00");
                }
            }
        }

        private void btMenu1_Click(object sender, EventArgs e) => AddMenuToOrder(0);
        private void btMenu2_Click(object sender, EventArgs e) => AddMenuToOrder(1);
        private void btMenu3_Click(object sender, EventArgs e) => AddMenuToOrder(2);
        private void btMenu4_Click(object sender, EventArgs e) => AddMenuToOrder(3);
        private void btMenu5_Click(object sender, EventArgs e) => AddMenuToOrder(4);
        private void btMenu6_Click(object sender, EventArgs e) => AddMenuToOrder(5);
        private void btMenu7_Click(object sender, EventArgs e) => AddMenuToOrder(6);
        private void btMenu8_Click(object sender, EventArgs e) => AddMenuToOrder(7);
        private void btMenu9_Click(object sender, EventArgs e) => AddMenuToOrder(8);
        private void btMenu10_Click(object sender, EventArgs e) => AddMenuToOrder(9);

        private void btSave_Click(object sender, EventArgs e)
        {
            // ตรวจสอบก่อนว่ารวมเป็นมีค่า 0.00 หรือเปล่า
            if (lbOrderPay.Text == "0.00")
            {
                MessageBox.Show("เลือกเมนูที่จะสั่งด้วย....!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (rdMemberYes.Checked != true && rdMenberNo.Checked != true)
            {
                MessageBox.Show("เลือกสถานะการเป็นสมาชิกด้วย!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (rdMemberYes.Checked == true && tbMemberName.Text == "(ชื่อสมาชิก)")
            {
                MessageBox.Show("กรุณาค้นหาสมาชิกด้วย!", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                    // กำหนด memberId ให้เป็น 0 หากไม่ได้เป็นสมาชิก (เพื่อไม่ให้ใส่ null)
                    int memberIdToSave = rdMemberYes.Checked ? memberId : 0;

                    // INSERT into order_tb and get orderId
                    string strSql1 = @"INSERT INTO order_tb (memberId, orderPay, createAt, updateAt)
                               VALUES (@memberId, @orderPay, @createAt, @updateAt);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int orderId;

                    using (SqlCommand sqlCommand = new SqlCommand(strSql1, sqlConnection, sqlTransaction))
                    {
                        sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberIdToSave;
                        sqlCommand.Parameters.Add("@orderPay", SqlDbType.Float).Value = float.Parse(lbOrderPay.Text);
                        sqlCommand.Parameters.Add("@createAt", SqlDbType.Date).Value = DateTime.Now;
                        sqlCommand.Parameters.Add("@updateAt", SqlDbType.Date).Value = DateTime.Now;

                        orderId = (int)sqlCommand.ExecuteScalar();
                    }

                    // INSERT into order_detail_tb
                    foreach (ListViewItem item in lvOrderMenu.Items)
                    {
                        string strSql2 = @"INSERT INTO order_detail_tb (orderId, menuName, menuPrice)
                                   VALUES (@orderId, @menuName, @menuPrice)";

                        using (SqlCommand sqlCommand = new SqlCommand(strSql2, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.VarChar, 100).Value = item.SubItems[0].Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(item.SubItems[1].Text);

                            sqlCommand.ExecuteNonQuery();
                        }
                    }

                    // UPDATE member_tb if member
                    if (rdMemberYes.Checked)
                    {
                        string strSql3 = @"UPDATE member_tb SET memberScore = @memberScore WHERE memberId = @memberId";

                        using (SqlCommand sqlCommand = new SqlCommand(strSql3, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@memberScore", SqlDbType.Int).Value = int.Parse(lbMemberScore.Text);
                            sqlCommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;

                            sqlCommand.ExecuteNonQuery();
                        }
                    }

                    // COMMIT
                    sqlTransaction.Commit();

                    MessageBox.Show("บันทึกเรียบร้อยแล้ว <3 :)", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    resetForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือ ติดต่อ IT: " + ex.Message);
                }
            }
        }

        private void lvOrderMenu_ItemActivate(object sender, EventArgs e)
        {
            if (lvOrderMenu.SelectedItems.Count > 0)
            {
                var selectedItem = lvOrderMenu.SelectedItems[0]; // ประกาศ selectedItem ตรงนี้

                var result = MessageBox.Show($"ต้องการลบรายการ '{selectedItem.Text}' ออกจากรายการใช่หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // ลดจำนวนเงินรวม
                    if (float.TryParse(lbOrderPay.Text, out float total) &&
                        float.TryParse(selectedItem.SubItems[1].Text, out float price))
                    {
                        total -= price;
                        if (total < 0) total = 0; // ป้องกันยอดติดลบ
                        lbOrderPay.Text = total.ToString("0.00");
                    }

                    // ลดแต้มสมาชิก (ถ้าเป็นสมาชิก)
                    if (tbMemberName.Text != "(ชื่อสมาชิก)")
                    {
                        if (int.TryParse(lbMemberScore.Text, out int score))
                        {
                            score = Math.Max(0, score - 1); // ลดแต้มแต่ไม่ต่ำกว่า 0
                            lbMemberScore.Text = score.ToString();
                        }
                    }

                    // ลบรายการออก
                    lvOrderMenu.Items.Remove(selectedItem);
                }
            }
        }
    }
}
