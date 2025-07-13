using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMenu : Form

    {
        byte[] menuImage;
        private int menuId;
        public FrmMenu()
        {
            InitializeComponent();
        }

        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }
        private void getAllMenuToListView()
        {
            //Connect String เพื่อติดต่อไปยังฐานข้อมูล
           // string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            //สร้าง Connection ไปยังฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string strSQL = "SELECT menuId, menuName ,menuPrice ,menuImage FROM[coffee_cafe_db].[dbo].[menu_tb]";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        lvShowAllMenu.Items.Clear();
                        lvShowAllMenu.Columns.Clear();
                        lvShowAllMenu.FullRowSelect = true;
                        lvShowAllMenu.View = View.Details;

                        if (lvShowAllMenu.SmallImageList == null)
                        {
                            lvShowAllMenu.SmallImageList = new ImageList();
                            lvShowAllMenu.SmallImageList.ImageSize = new Size(50, 50);
                            lvShowAllMenu.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }
                        lvShowAllMenu.SmallImageList.Images.Clear();

                        lvShowAllMenu.Columns.Add("รูปเมนู", 100, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("รหัสเมนู", 80, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ชื่อเมนู", 135, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ราคาเมนู", 75, HorizontalAlignment.Right);

                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem();
                            Image menuImage = null;

                            if (dataRow["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["menuImage"];
                                menuImage = convertByteArrayToImage(imgByte);

                                // เพิ่มรูปเข้า ImageList และเก็บ index
                                lvShowAllMenu.SmallImageList.Images.Add(menuImage);
                                item.ImageIndex = lvShowAllMenu.SmallImageList.Images.Count - 1;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }

                            // ต้องใส่ค่าแรกใน constructor ด้วย
                            item.SubItems.Add(dataRow["menuId"].ToString());
                            item.SubItems.Add(dataRow["menuName"].ToString());
                            item.SubItems.Add(dataRow["menuPrice"].ToString());

                            lvShowAllMenu.Items.Add(item);
                        }

                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือติดต่อ IT : " + ex.Message);
                }
            }
        }
        private void FrmMenu_Load(object sender, EventArgs e)
        {
            getAllMenuToListView();
            menuImage = null;
            pbMenuImage.Image = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();

            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
        }

        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void btSelectMenuImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.Filter = "Image Files (*.jpg;*.png;)|*.jpg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pbMenuImage.Image = Image.FromFile(openFileDialog.FileName);

                if (pbMenuImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Png);
                }
            }
        }
        private void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        private void btSave_Click(object sender, EventArgs e)
        {
            // ตรวจสอบว่ามีการเลือกรูปหรือไม่
            if (menuImage == null)
            {
                showWarningMSG("เลือกรูปเมนูด้วย...");
            }
            // ตรวจสอบว่ากรอกชื่อเมนูหรือไม่
            else if (tbMenuName.Text.Length == 0)
            {
                showWarningMSG("ป้อนชื่อเมนูด้วย...");
            }
            // ตรวจสอบว่ากรอกราคาเมนูหรือไม่
            else if (tbMenuPrice.Text.Length == 0)
            {
                showWarningMSG("ป้อนราคาเมนูด้วย...");
            }
            else
            {
                // Connect String สำหรับเชื่อมต่อฐานข้อมูล
                //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

                using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        //ก่อนจะกดบันทึกให้ตรวจสอบก่อนว่ามีเมนู ครบ10 รึยัง ถ้าครบแล้ว ต้องลบออกสัก1อันก่อน
                        string countSQL = "SELECT COUNT(*) FROM menu_tb";
                        using (SqlCommand countCommand = new SqlCommand(countSQL, sqlConnection))
                        {
                            int rowCount = (int)countCommand.ExecuteScalar();
                            if (rowCount >= 10)  // ใช้ >= 10 เพื่อป้องกันกรณีเกิน 10 ด้วย
                            {
                                showWarningMSG("มีเมนูได้ทั้งหมดแค่ 10 เมนูเท่านั้น ถ้าต้องการจะเพิ่ม ต้องลบของเก่าก่อน");
                                return;
                            }
                        }

                        // เริ่ม Transaction
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
                        // คำสั่ง INSERT ข้อมูล
                        string strSql = "INSERT INTO menu_tb " +
                                        "(menuName, menuPrice, menuImage) " +
                                        "VALUES (@menuName, @menuPrice, @menuImage)";

                        using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                        {
                            // กำหนดค่าให้กับ Parameter ต่างๆ
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            sqlCommand.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;

                            // สั่งให้ SQL ทำงาน
                            sqlCommand.ExecuteNonQuery();

                            // ยืนยันการทำธุรกรรม
                            sqlTransaction.Commit();

                            // แจ้งเตือนผู้ใช้
                            MessageBox.Show("บันทึกเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // อัปเดตรายการใน ListView และล้างค่าบนหน้าจอ
                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void tbMenuPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // เช็คว่าอักขระที่กด ไม่ใช่ปุ่มควบคุม (เช่น Backspace)
            // และไม่ใช่ตัวเลข
            // และกรณีที่กดจุดทศนิยม (.) ให้ตรวจสอบว่ามีจุดอยู่แล้วหรือยัง ถ้ามีแล้วไม่ให้พิมพ์ซ้ำ
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.' || ((TextBox)sender).Text.Contains(e.KeyChar.ToString())))
            {
                e.Handled = true;  // ถ้าเข้าเงื่อนไขนี้ ไม่อนุญาตให้พิมพ์ตัวอักษรนั้น
            }
        }

        private void lvShowAllMenu_ItemActivate(object sender, EventArgs e)
        {
            if (lvShowAllMenu.SelectedItems.Count > 0)
            {
                int menuId = int.Parse(lvShowAllMenu.SelectedItems[0].SubItems[1].Text);

                // ดึงข้อมูลเมนูตาม menuId มาแสดงใน TextBox ต่างๆ
                LoadMenuDataToTextBox(menuId);

                // เปิดปุ่ม Update และ Delete
                btSave.Enabled = false;
                btUpdate.Enabled = true;
                btDelete.Enabled = true;
            }
            else
            {
                MessageBox.Show("กรุณาเลือกรายการสินค้าที่ต้องการ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void LoadMenuDataToTextBox(int menuId)
        {
            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();
                    string query = "SELECT * FROM menu_tb WHERE menuId = @menuId";

                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@menuId", menuId);

                        SqlDataReader reader = sqlCommand.ExecuteReader();
                        if (reader.Read())
                        {
                            tbMenuId.Text = reader["menuId"].ToString();
                            tbMenuName.Text = reader["menuName"].ToString();
                            tbMenuPrice.Text = reader["menuPrice"].ToString();

                            if (reader["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])reader["menuImage"];
                                menuImage = imgByte; // เก็บค่าไว้ใช้ตอน Save หรือ Update
                                pbMenuImage.Image = convertByteArrayToImage(imgByte);
                            }
                            else
                            {
                                menuImage = null;
                                pbMenuImage.Image = null;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (menuImage == null)
            {
                showWarningMSG("เลือกรูปเมนูด้วย...");
            }
            else if (tbMenuName.Text.Length == 0)
            {
                showWarningMSG("ป้อนชื่อเมนูด้วย...");
            }
            else if (tbMenuPrice.Text.Length == 0)
            {
                showWarningMSG("ป้อนราคาเมนูด้วย...");
            }
            else
            {
                //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

                using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSql = "UPDATE menu_tb SET " +
                                        "menuName = @menuName, " +
                                        "menuPrice = @menuPrice, " +
                                        "menuImage = @menuImage " +  // ตัดคอมมาออก
                                        "WHERE menuId = @menuId";

                        using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            sqlCommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            sqlCommand.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;
                            sqlCommand.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("อัปเดตข้อมูลเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่ หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            //ลบข้อมูล ที่ถูกเลือก โดยมีการถามก่อนลบ
            if (string.IsNullOrEmpty(tbMenuId.Text))
            {
                MessageBox.Show("กรุณาเลือกรายการเมนูก่อนลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int menuId;
            if (!int.TryParse(tbMenuId.Text, out menuId))
            {
                MessageBox.Show("รหัสเมนูไม่ถูกต้อง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirmResult = MessageBox.Show("คุณแน่ใจที่จะลบเมนูนี้หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

                using (SqlConnection sqlConnection = new SqlConnection(ShereResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSql = "DELETE FROM menu_tb WHERE menuId = @menuId";

                        using (SqlCommand sqlCommand = new SqlCommand(strSql, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@menuId", SqlDbType.Int).Value = menuId;

                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("ลบข้อมูลเมนูเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // รีเซ็ตฟอร์มและโหลดข้อมูลใหม่
                            getAllMenuToListView();
                            menuImage = null;
                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();

                            // ปิดปุ่ม Update และ Delete
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

        private void btCancel_Click(object sender, EventArgs e)
        {
            //ยกเลิกการทำงานต่าง และ เคลียทุกอย่างออกจาก toolbox
            pbMenuImage.Image = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();

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
