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
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SmartPacker
{
    public partial class Frm_Styles : Form
    {

        SqlConnection con = new SqlConnection();
        public Frm_Styles()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;TrustServerCertificate=True;");
            //con = new SqlConnection(@"Data Source= 192.168.47.25;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;TrustServerCertificate=True;");
            con.Open();
            btnUpdate.Visible = false;
            grdExistStyles.ReadOnly = true;
            grdUpdate.ReadOnly = true;

            txtColourCode.Visible = false;
            txtColourName.Visible = false;
            txtEAN.Visible = false;
            txtSize.Visible = false;
            grdUpdate.Visible = false;
            btnAdd.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;


            grdStyleCode.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdStyleCode.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdStyleCode.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);
            grdStyleCode.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            grdStyleCode.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            grdStyleCode.Refresh();


            grdExistStyles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdExistStyles.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdExistStyles.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);
            grdExistStyles.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            grdExistStyles.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            grdExistStyles.Refresh();

            loadCmbCustomer();
            loadCmbSeason();
            grdExistStyles.Visible = false;


            LoadData();
            grdStyleCode.Columns["StyleNo"].HeaderText = "Style No";
        }

        private void grdStyleCode_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                object cellValue = grdStyleCode.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                LoadStyleData(cellValue);

            }
        }

        private void grdStyleCode_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
               
                object cellValue = grdStyleCode.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                LoadStyleData(cellValue);

            }
        }

        private void btnAddStyle_Click(object sender, EventArgs e)
        {
            Frm_New_Style frm = new Frm_New_Style();
            frm.Show();

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void LoadData()
        {
            String queryCount = "SELECT COUNT(*) FROM Style";
            String queryRead = "SELECT DISTINCT StyleNo FROM Style";
            DataTable dataTable = new DataTable();
            SqlCommand commandCode = new SqlCommand(queryCount, con);
            int styleCount;
            try
            {
                styleCount = (int)commandCode.ExecuteScalar();
                if (styleCount > 0)
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(queryRead, con);
                    adapter.Fill(dataTable);
                    grdStyleCode.DataSource = dataTable;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
        }


        private void LoadStyleData(object cellValue)
        {
           
            if (cellValue == null)
            {
                MessageBox.Show("Invalid StyleNo provided.");
                return;
            }

            string cellValueNo = cellValue.ToString();
            string queryLoad = "SELECT StyleNo, StyleName, ColourCode, ColourName, Size, EAN FROM Style WHERE StyleNo = @StyleNo";

            using (SqlCommand commandCode = new SqlCommand(queryLoad, con))
            {
                commandCode.Parameters.AddWithValue("@StyleNo", cellValueNo);

                try
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(commandCode);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable.Rows.Count > 0)
                    {
                        grdExistStyles.Visible = true;

                       
                        if (grdExistStyles.Columns.Count == 0)
                        {
                            grdExistStyles.Columns.Add("StyleNo", "StyleNo");
                            grdExistStyles.Columns.Add("StyleName", "StyleName");
                            grdExistStyles.Columns.Add("ColourCode", "ColourCode");
                            grdExistStyles.Columns.Add("ColourName", "ColourName");
                            grdExistStyles.Columns.Add("Size", "Size");
                            grdExistStyles.Columns.Add("EAN", "EAN");

                            DataGridViewButtonColumn deleteButton = new DataGridViewButtonColumn();
                            deleteButton.Name = "Delete";
                            deleteButton.HeaderText = "Delete";
                            deleteButton.Text = "Delete";
                            deleteButton.UseColumnTextForButtonValue = true;
                            grdExistStyles.Columns.Add(deleteButton);
                        }
                        grdExistStyles.CellClick -= GrdExistStyles_CellClick;
                        grdExistStyles.CellClick += GrdExistStyles_CellClick;

                      
                        grdExistStyles.Rows.Clear();

                        
                        foreach (DataRow row in dataTable.Rows)
                        {
                            
                            string[] colourCodes = row["ColourCode"].ToString().Split('/');
                            string[] colourNames = row["ColourName"].ToString().Split('/');
                            string[] sizes = row["Size"].ToString().Split('/');
                            string[] eans = row["EAN"].ToString().Split('/');

                           
                            int rowCount = Math.Min(Math.Min(colourCodes.Length, colourNames.Length), Math.Min(sizes.Length, eans.Length));

                            for (int i = 0; i < rowCount; i++)
                            {
                                grdExistStyles.Rows.Add(
                                    row["StyleNo"].ToString(),
                                    row["StyleName"].ToString(),
                                    colourCodes[i],
                                    colourNames[i],
                                    sizes[i],
                                    eans[i]
                                );
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No data found for the given StyleNo.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GrdExistStyles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtColourCode.Visible = true;
            txtColourName.Visible = true;
            txtEAN.Visible = true;
            txtSize.Visible = true;
            grdUpdate.Visible = true;
            btnAdd.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;


            if (e.RowIndex >= 0) 
            {
                
                if (grdExistStyles.Columns[e.ColumnIndex].Name == "Delete")
                {
                    var result = MessageBox.Show(
                        "Are you sure you want to delete this data set?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        grdExistStyles.Rows.RemoveAt(e.RowIndex);

                        DeleteRecordFromDatabase(txtColourCode.Text, txtColourName.Text, txtSize.Text, txtEAN.Text);
                        return; 
                    }
                }

               
                foreach (DataGridViewRow row in grdExistStyles.Rows)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }

                grdExistStyles.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;

               
                DataGridViewRow selectedRow = grdExistStyles.Rows[e.RowIndex];
                txtColourCode.Text = selectedRow.Cells["ColourCode"].Value?.ToString() ?? string.Empty;
                txtColourName.Text = selectedRow.Cells["ColourName"].Value?.ToString() ?? string.Empty;
                txtSize.Text = selectedRow.Cells["Size"].Value?.ToString() ?? string.Empty;
                txtEAN.Text = selectedRow.Cells["EAN"].Value?.ToString() ?? string.Empty;


            }
        }
        private void DeleteRecordFromDatabase(string code, string name, string size, string ean)
        {
            try
            {
               
                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(size) && !string.IsNullOrEmpty(ean))
                {
                    {

                        
                        string queryDelete = "DELETE FROM Style WHERE ColourCode = @code AND ColourName = @name AND Size=@size AND EAN=@ean";

                        using (SqlCommand command = new SqlCommand(queryDelete, con))
                        {
                            command.Parameters.AddWithValue("@code", code);
                            command.Parameters.AddWithValue("@name", name);
                            command.Parameters.AddWithValue("@size", size);
                            command.Parameters.AddWithValue("@ean", ean);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while deleting the record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {

        }
        private void loadCmbCustomer()
        {
            string queryCombo = "SELECT CusName FROM Customer";
            SqlCommand command = new SqlCommand(queryCombo, con);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            cmbCustomer.DisplayMember = "CusName";
            cmbCustomer.ValueMember = "CusName";      
            cmbCustomer.DataSource = dataTable;
            cmbCustomer.SelectedIndex = -1;

        }

        private void loadCmbSeason()
        {
            try
            {



                String queryCusId = "SELECT cus_id FROM Customer WHERE CusName = @CusName";
                SqlCommand command = new SqlCommand(queryCusId, con);
                command.Parameters.AddWithValue("@CusName", cmbCustomer.Text.Trim());
                object result = command.ExecuteScalar();
                int cus_id = Convert.ToInt32(result);

                String querySeason = "SELECT Season FROM CustomerSeasons WHERE cus_id = @cus_id";
                SqlCommand command2 = new SqlCommand(querySeason, con);
                command2.Parameters.AddWithValue("@cus_id", cus_id);

               
                SqlDataAdapter adapter = new SqlDataAdapter(command2);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                cmbSeason.DisplayMember = "Season";
                cmbSeason.ValueMember = "Season";
                cmbSeason.DataSource = dataTable;
                cmbSeason.SelectedIndex = -1;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadCmbSeason();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void cmbSeason_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
           
            if (cmbCustomer.SelectedItem != null && cmbSeason.SelectedItem != null) {
                string queryFetch = "SELECT cus_id FROM Customer WHERE CusName=@CusName";
                SqlCommand command = new SqlCommand(queryFetch, con);
                command.Parameters.AddWithValue("@CusName", cmbCustomer.Text);
                Object result = command.ExecuteScalar();
               
                int customerId = Convert.ToInt32(result);

                String querySeason = "SELECT Season FROM CustomerSeasons WHERE cus_id=@cus_id";
                SqlCommand command2 = new SqlCommand(querySeason, con);
                command.Parameters.AddWithValue("@cus_id", customerId);
                Object result2 = command.ExecuteScalar();
                String season = Convert.ToString(result2);

                string queryRead = "SELECT DISTINCT StyleNo FROM Style WHERE CusId=@CusId AND Season=@Season";
                SqlDataAdapter adapter = new SqlDataAdapter(queryRead, con);
                adapter.SelectCommand.Parameters.AddWithValue("@CusId", customerId);
                adapter.SelectCommand.Parameters.AddWithValue("@Season", cmbSeason.Text);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                grdStyleCode.DataSource = dataTable;

            }
            else if (cmbCustomer.SelectedItem != null && cmbSeason.SelectedItem == null)
            {
                string queryFetch = "SELECT cus_id FROM Customer WHERE CusName=@CusName";
                SqlCommand command = new SqlCommand(queryFetch, con);
                command.Parameters.AddWithValue("@CusName", cmbCustomer.Text);
                Object result = command.ExecuteScalar();
                int customerId = Convert.ToInt32(result);

                string queryRead = "SELECT DISTINCT StyleNo FROM Style WHERE CusId=@CusId";
                SqlDataAdapter adapter = new SqlDataAdapter(queryRead, con);
                adapter.SelectCommand.Parameters.AddWithValue("@CusId", customerId);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                grdStyleCode.DataSource = dataTable;

            }
            else if (cmbCustomer.SelectedItem == null && cmbSeason.SelectedItem == null)
            {
                MessageBox.Show("Please select customer and season before search", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else if(cmbCustomer.SelectedItem == null && cmbSeason.SelectedItem != null)
            {
                string queryRead = "SELECT DISTINCT StyleNo FROM Style WHERE Season=@Season";
                SqlDataAdapter adapter = new SqlDataAdapter(queryRead, con);
                adapter.SelectCommand.Parameters.AddWithValue("@Season", cmbSeason.Text);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                grdStyleCode.DataSource = dataTable;
            }


        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            btnUpdate.Visible = true;
            string size = txtSize.Text.Trim();
            string code = txtColourCode.Text.Trim();
            string name = txtColourName.Text.Trim();
            string ean = txtEAN.Text.Trim();

            if (grdUpdate.Columns.Count == 0)
            {
                grdUpdate.Columns.Add("ColourCode", "Colour Code");
                grdUpdate.Columns.Add("ColourName", "Colour Name");
                grdUpdate.Columns.Add("Size", "Size");
                grdUpdate.Columns.Add("EAN", "EAN");
            }
            grdUpdate.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdUpdate.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdUpdate.DefaultCellStyle.ForeColor = Color.FromArgb(69, 14, 123);
            grdUpdate.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            grdUpdate.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);

           
            if (!string.IsNullOrEmpty(size) &&
                !string.IsNullOrEmpty(code) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(ean))
            {
               
                DataGridViewRow highlightedRow = null;
                foreach (DataGridViewRow row in grdUpdate.Rows)
                {
                    if (row.DefaultCellStyle.BackColor == Color.Red)
                    {
                        highlightedRow = row;
                        break;
                    }
                }

                if (highlightedRow != null)
                {
                    
                    highlightedRow.Cells["ColourCode"].Value = code;
                    highlightedRow.Cells["ColourName"].Value = name;
                    highlightedRow.Cells["Size"].Value = size;
                    highlightedRow.Cells["EAN"].Value = ean;

                   
                       
                    highlightedRow.DefaultCellStyle.BackColor = Color.White;

                    MessageBox.Show("Style record updated successfully.");
                }
                else
                {
                    
                    grdUpdate.Rows.Add(code, name, size, ean);

                }


                txtColourCode.Focus();
            }
            else
            {
                MessageBox.Show("Please fill all fields before adding or updating a row.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow highlightedRow = null;

              
                foreach (DataGridViewRow row in grdExistStyles.Rows)
                {
                    if (row.DefaultCellStyle.BackColor == Color.Red)
                    {
                        highlightedRow = row;
                        break;
                    }
                }

                if (highlightedRow != null)
                {
                    string code = highlightedRow.Cells["ColourCode"].Value.ToString();
                    string name = highlightedRow.Cells["ColourName"].Value.ToString();
                    string size = highlightedRow.Cells["Size"].Value.ToString();
                    string ean = highlightedRow.Cells["EAN"].Value.ToString();

                  
                    string queryStyleNo = "SELECT StyleNo, StyleName, CusId, Season FROM Style WHERE ColourCode = @code AND ColourName = @name AND Size = @size AND EAN = @ean;";
                    SqlCommand command = new SqlCommand(queryStyleNo, con);
                    command.Parameters.AddWithValue("@code", code);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@size", size);
                    command.Parameters.AddWithValue("@ean", ean);

                    string styleNo = string.Empty;
                    string styleName = string.Empty;
                    string cusId = string.Empty;
                    string season = string.Empty;

                    
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            styleNo = reader["StyleNo"].ToString();
                            styleName = reader["StyleName"].ToString();
                            cusId = reader["CusId"].ToString();
                            season = reader["Season"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("No record found with the given StyleNo.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    } 

                  
                    string updateQuery = "UPDATE Style SET StyleName = @txtStyleName, ColourCode = @txtColourCode, ColourName = @txtColourName, Size = @txtSize, EAN = @txtEAN, CusId = @cmbCustomerId, Season = @txtSeason " +
                                         "WHERE StyleNo = @txtStyleNo AND ColourCode = @code AND ColourName = @name AND Size = @size AND EAN = @ean;";
                    SqlCommand cmd = new SqlCommand(updateQuery, con);

                    cmd.Parameters.AddWithValue("@txtStyleNo", styleNo);
                    cmd.Parameters.AddWithValue("@txtStyleName", styleName);
                    cmd.Parameters.AddWithValue("@txtColourCode", txtColourCode.Text);
                    cmd.Parameters.AddWithValue("@txtColourName", txtColourName.Text);
                    cmd.Parameters.AddWithValue("@txtSize", txtSize.Text);
                    cmd.Parameters.AddWithValue("@txtEAN", txtEAN.Text);
                    cmd.Parameters.AddWithValue("@txtSeason", season);
                    cmd.Parameters.AddWithValue("@cmbCustomerId", Convert.ToInt32(cusId));
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@size", size);
                    cmd.Parameters.AddWithValue("@ean", ean);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Style record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);


                        highlightedRow.Cells["ColourCode"].Value = txtColourCode.Text;
                        highlightedRow.Cells["ColourName"].Value = txtColourName.Text;
                        highlightedRow.Cells["Size"].Value = txtSize.Text;
                        highlightedRow.Cells["EAN"].Value = txtEAN.Text;


                        btnUpdate.Visible = false;
                       
                        txtColourCode.Clear();
                        
                        txtEAN.Clear();
                        txtSize.Clear();
                        txtColourName.Clear();
                        highlightedRow.DefaultCellStyle.BackColor = Color.White;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void grdExistStyles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtSize_TextChanged(object sender, EventArgs e)
        {
            foreach (char c in txtSize.Text)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    // Display an error message using a MessageBox or Label
                    MessageBox.Show("Only letters and digits are allowed.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Remove the invalid character
                    txtSize.Text = new string(txtSize.Text.Where(char.IsLetterOrDigit).ToArray());
                    txtSize.SelectionStart = txtSize.Text.Length; // Set cursor to the end
                    break;
                }
            }
        }

        private void txtColourCode_textChange(object sender, EventArgs e)
        {
            if (txtColourCode.Text.Contains(","))
            {
                txtColourCode.Text = txtColourCode.Text.Replace(",", "");
                txtColourCode.SelectionStart = txtColourCode.Text.Length;
                MessageBox.Show("Only letters and digits are allowed.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
        }

        private void txtEAN_TextChanged(object sender, EventArgs e)
        {

            foreach (char c in txtEAN.Text)
            {
                if (!char.IsDigit(c))
                {
                    MessageBox.Show("Only digits are allowed.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEAN.Clear();
                    return;
                }
            }


            if (txtEAN.Text.Length > 13)
            {
                MessageBox.Show("The EAN should not exceed 13 digits.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);


                txtEAN.Text = txtEAN.Text.Substring(0, 13);
                txtEAN.SelectionStart = txtEAN.Text.Length; // Set cursor to the end
            }
        }

    }
}
