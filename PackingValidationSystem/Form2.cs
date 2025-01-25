using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SmartPacker
{
    public partial class Frm_New_Style : Form
    {
        SqlConnection con = new SqlConnection();
        public Frm_New_Style()
        {
            InitializeComponent();
            //con = new SqlConnection(@"Data Source=192.168.47.25;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;TrustServerCertificate=True;");
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;TrustServerCertificate=True;");
            con.Open();
            LoadComboBox();
            LoadSeasonComboBox();
            InitializeGrid();
            btnUpdate.Visible = false;
            grdSizes.ReadOnly = true;
            grdNewStyle.ReadOnly = true;

            grdSizes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdSizes.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdSizes.DefaultCellStyle.ForeColor = Color.FromArgb(69, 14, 123);
            grdSizes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            grdSizes.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            grdSizes.Refresh();

            grdNewStyle.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdNewStyle.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdNewStyle.DefaultCellStyle.ForeColor = Color.FromArgb(69, 14, 123);
            grdNewStyle.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            grdNewStyle.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            grdNewStyle.Refresh();



        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void txtStyleNo_TextChanged(object sender, EventArgs e)
        {

        }



        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtStyleNo.Text) &&
                    !string.IsNullOrWhiteSpace(txtStyleName.Text) &&
                    !string.IsNullOrWhiteSpace(cmbCustomerName.Text))
                {
                    foreach (DataGridViewRow row in grdSizes.Rows)
                    {
                        if (row.Cells[0].Value != null && row.Cells[1].Value != null &&
                            row.Cells[2].Value != null && row.Cells[3].Value != null)
                        {
                            string colourCode = row.Cells[0].Value.ToString();
                            string colourName = row.Cells[1].Value.ToString();
                            string size = row.Cells[2].Value.ToString();
                            string ean = row.Cells[3].Value.ToString();

                            // Insert data into the database for each row
                            string queryCreate = "INSERT INTO Style (StyleNo,StyleName,ColourCode,ColourName,Size,EAN,CusId,Season)" +
                                " VALUES (@txtStyleNo,@txtStyleName,@txtColourCode,@txtColourName,@txtSize,@txtEAN,@cmbCustomerId,@txtSeason)";
                            SqlCommand cmd = new SqlCommand(queryCreate, con);

                            cmd.Parameters.AddWithValue("@txtStyleNo", txtStyleNo.Text);
                            cmd.Parameters.AddWithValue("@txtStyleName", txtStyleName.Text);
                            cmd.Parameters.AddWithValue("@txtColourCode", colourCode);
                            cmd.Parameters.AddWithValue("@txtColourName", colourName);
                            cmd.Parameters.AddWithValue("@txtSize", size);
                            cmd.Parameters.AddWithValue("@txtEAN", ean);
                            cmd.Parameters.AddWithValue("@cmbCustomerId", (int)cmbCustomerName.SelectedValue);
                            cmd.Parameters.AddWithValue("@txtSeason", cmbSeason.Text);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Style Enter Successfully");
                    LoadDataToGrid();

                    // Clear inputs
                    //txtStyleNo.Clear();
                    //txtStyleName.Clear();
                    txtColourCode.Clear();
                    txtColourName.Clear();
                    txtSize.Clear();
                    txtEAN.Clear();
                    grdSizes.Rows.Clear(); // Clear the grid after saving
                }
                else
                {
                    MessageBox.Show("Please fill all the fields", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewRow highlightedRow = null;
                foreach (DataGridViewRow row in grdNewStyle.Rows)
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

                    // Reset row background color after update

                    string updateQuery = "UPDATE Style SET StyleName = @txtStyleName, ColourCode = @txtColourCode, ColourName = @txtColourName, Size = @txtSize, EAN = @txtEAN, CusId = @cmbCustomerId, Season = @txtSeason " +
                        "WHERE StyleNo = @txtStyleNo AND ColourCode = @code AND ColourName=@name AND Size = @size AND EAN = @ean";
                    SqlCommand cmd = new SqlCommand(updateQuery, con);

                    cmd.Parameters.AddWithValue("@txtStyleNo", txtStyleNo.Text);
                    cmd.Parameters.AddWithValue("@txtStyleName", txtStyleName.Text);
                    cmd.Parameters.AddWithValue("@txtColourCode", txtColourCode.Text);
                    cmd.Parameters.AddWithValue("@txtColourName", txtColourName.Text);
                    cmd.Parameters.AddWithValue("@txtSize", txtSize.Text);
                    cmd.Parameters.AddWithValue("@txtEAN", txtEAN.Text);
                    cmd.Parameters.AddWithValue("@txtSeason", cmbSeason.Text);
                    cmd.Parameters.AddWithValue("@cmbCustomerId", (int)cmbCustomerName.SelectedValue);
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.Parameters.AddWithValue("@name", name);
                    cmd.Parameters.AddWithValue("@size", size);
                    cmd.Parameters.AddWithValue("@ean", ean);

                    int rowsAffected = cmd.ExecuteNonQuery();



                    if (rowsAffected > 0)
                    {
                        MessageBox.Show(" Style record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        highlightedRow.Cells["ColourCode"].Value = txtColourCode.Text;
                        highlightedRow.Cells["ColourName"].Value = txtColourName.Text;
                        highlightedRow.Cells["Size"].Value = txtSize.Text;
                        highlightedRow.Cells["EAN"].Value = txtEAN.Text;

                        btnUpdate.Visible = false;
                        btnSave.Visible = true;

                        txtStyleName.Clear();
                        txtColourCode.Clear();
                        txtStyleName.Clear();
                        txtStyleNo.Clear();
                        txtEAN.Clear();
                        txtSize.Clear();

                        highlightedRow.DefaultCellStyle.BackColor = Color.White;


                    }
                    else
                    {
                        MessageBox.Show("No record found with the given StyleNo.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Helper function to collect column data
        private string CollectColumnData(DataGridView grid, int columnIndex)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue; // Skip new row placeholder

                string value = row.Cells[columnIndex].Value?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    stringBuilder.Append(value + "/");
                }
            }

            // Remove the last '/' delimiter
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--; // Trim the trailing '/'
            }

            return stringBuilder.ToString();
        }
        private void LoadDataToGrid()
        {
            try
            {
                string query = "SELECT StyleNo, StyleName, ColourCode, ColourName, Size, EAN FROM Style WHERE StyleNo = @styleNo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@styleNo", txtStyleNo.Text);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    // Ensure the grid has columns
                    if (grdNewStyle.Columns.Count == 0)
                    {
                        grdNewStyle.Columns.Add("StyleNo", "Style NO");
                        grdNewStyle.Columns.Add("StyleName", "Style Name");
                        grdNewStyle.Columns.Add("ColourCode", "ColourCode");
                        grdNewStyle.Columns.Add("ColourName", "ColourName");
                        grdNewStyle.Columns.Add("Size", "Size");
                        grdNewStyle.Columns.Add("EAN", "EAN");

                        DataGridViewButtonColumn deleteButton = new DataGridViewButtonColumn();
                        deleteButton.Name = "Delete";
                        deleteButton.HeaderText = "Delete";
                        deleteButton.Text = "Delete";
                        deleteButton.UseColumnTextForButtonValue = true; // Show "Delete" as the button text
                        grdNewStyle.Columns.Add(deleteButton);


                    }

                    // Clear existing rows
                    grdNewStyle.Rows.Clear();

                    // Add rows directly from the DataTable
                    foreach (DataRow row in dataTable.Rows)
                    {
                        grdNewStyle.Rows.Add(row["StyleNo"].ToString(),
                                             row["StyleName"].ToString(),
                                             row["ColourCode"].ToString(),
                                             row["ColourName"].ToString(),
                                             row["Size"].ToString(),
                                             row["EAN"].ToString());
                    }
                    grdNewStyle.CellClick += grdNewStyle_CellClick;
                }
                else
                {
                    MessageBox.Show("No data found for the given StyleNo.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void GrdNewStyle_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            //if (e.RowIndex >= 0 && grdNewStyle.Columns[e.ColumnIndex].Name == "Delete")
            //{
            //    //        string code = grdNewStyle.Rows[e.RowIndex].Cells["ColourCode"].Value.ToString();
            //    string name = grdNewStyle.Rows[e.RowIndex].Cells["ColourName"].Value.ToString();
            //    string size = grdNewStyle.Rows[e.RowIndex].Cells["Size"].Value.ToString();
            //    string ean = grdNewStyle.Rows[e.RowIndex].Cells["EAN"].Value.ToString();

            //    DialogResult result = MessageBox.Show($"Are you sure you want to delete this record ?",
            //                                          "Confirm Deletion",
            //                                          MessageBoxButtons.YesNo,
            //                                          MessageBoxIcon.Warning);

            //    if (result == DialogResult.Yes)
            //    {

            //        DeleteRowFromDatabase(code, name, size, ean);


            //        grdNewStyle.Rows.RemoveAt(e.RowIndex);
            //    }
            //}
        }
        private void DeleteRowFromDatabase(string code, string name, string size, string ean)
        {
            try
            {
                string deleteQuery = "DELETE FROM Style WHERE ColourCode = @code AND ColourName = @name AND Size = @size AND EAN = @ean";
                SqlCommand cmd = new SqlCommand(deleteQuery, con);
                cmd.Parameters.AddWithValue("@code", code);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@size", size);
                cmd.Parameters.AddWithValue("@ean", ean);


                int rowsAffected = cmd.ExecuteNonQuery();


                if (rowsAffected > 0)
                {
                    MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No record found with the given values.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void cmbCustomerName_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSeasonComboBox();
        }

        private void LoadComboBox()
        {
            string queryCombo = "SELECT cus_id,CusName FROM Customer";
            SqlCommand command = new SqlCommand(queryCombo, con);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            cmbCustomerName.DataSource = dataTable;
            cmbCustomerName.DisplayMember = "CusName";
            cmbCustomerName.ValueMember = "cus_id";
        }

        private void LoadSeasonComboBox()
        {
            try
            {
                // Query to fetch the Season data for the selected customer
                String queryCusId = "SELECT cus_id FROM Customer WHERE CusName = @CusName";
                SqlCommand command = new SqlCommand(queryCusId, con);
                command.Parameters.AddWithValue("@CusName", cmbCustomerName.Text.Trim());
                object result = command.ExecuteScalar();
                int cus_id = Convert.ToInt32(result);

                String querySeason = "SELECT Season FROM CustomerSeasons WHERE cus_id = @cus_id";
                SqlCommand command2 = new SqlCommand(querySeason, con);
                command2.Parameters.AddWithValue("@cus_id", cus_id);

                // Execute the query and fetch data
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

        private void grdNewStyle_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //if (e.RowIndex >= 0)
            //{
            //    btnSave.Visible = false;
            //    btnUpdate.Visible = true;
            //    foreach (DataGridViewRow row in grdNewStyle.Rows)
            //    {
            //        row.DefaultCellStyle.BackColor = Color.White;
            //    }
            //    grdNewStyle.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;

            //    DataGridViewRow selectedRow = grdNewStyle.Rows[e.RowIndex];

            //    txtColourCode.Text = selectedRow.Cells["ColourCode"].Value?.ToString() ?? string.Empty;
            //    txtStyleNo.Text = selectedRow.Cells["StyleNo"].Value?.ToString() ?? string.Empty;
            //    txtStyleName.Text = selectedRow.Cells["StyleName"].Value?.ToString() ?? string.Empty;
            //    txtColourName.Text = selectedRow.Cells["ColourName"].Value?.ToString() ?? string.Empty;
            //    txtSize.Text = selectedRow.Cells["Size"].Value?.ToString() ?? string.Empty;
            //    txtEAN.Text = selectedRow.Cells["EAN"].Value?.ToString() ?? string.Empty;

            //    if (e.ColumnIndex == grdNewStyle.Columns["Delete"].Index && e.RowIndex >= 0)
            //    {
            //        DialogResult result = MessageBox.Show("Are you sure you want to delete this row?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //        if (result == DialogResult.Yes)
            //        {
            //            grdNewStyle.Rows.RemoveAt(e.RowIndex); // Remove the selected row
            //        }

            //    }

            //}
        }

      private void grdNewStyle_CellClick(object sender, DataGridViewCellEventArgs e)
{
    if (e.RowIndex >= 0)
    {
        btnSave.Visible = false;
        btnUpdate.Visible = true;

        // Reset all rows' colors
        foreach (DataGridViewRow row in grdNewStyle.Rows)
        {
            row.DefaultCellStyle.BackColor = Color.White;
        }

        // Highlight the selected row
        if (e.RowIndex < grdNewStyle.Rows.Count)
        {
            grdNewStyle.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;
        }

        DataGridViewRow selectedRow = grdNewStyle.Rows[e.RowIndex];

        // Populate text boxes with row data
        txtColourCode.Text = selectedRow.Cells["ColourCode"].Value?.ToString() ?? string.Empty;
        txtStyleNo.Text = selectedRow.Cells["StyleNo"].Value?.ToString() ?? string.Empty;
        txtStyleName.Text = selectedRow.Cells["StyleName"].Value?.ToString() ?? string.Empty;
        txtColourName.Text = selectedRow.Cells["ColourName"].Value?.ToString() ?? string.Empty;
        txtSize.Text = selectedRow.Cells["Size"].Value?.ToString() ?? string.Empty;
        txtEAN.Text = selectedRow.Cells["EAN"].Value?.ToString() ?? string.Empty;

        // Handle delete button click
        if (e.ColumnIndex == grdNewStyle.Columns["Delete"].Index)
        {

                    string code = grdNewStyle.Rows[e.RowIndex].Cells["ColourCode"].Value.ToString();
                    string name = grdNewStyle.Rows[e.RowIndex].Cells["ColourName"].Value.ToString();
                    string size = grdNewStyle.Rows[e.RowIndex].Cells["Size"].Value.ToString();
                    string ean = grdNewStyle.Rows[e.RowIndex].Cells["EAN"].Value.ToString();
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this row?", 
                                                  "Confirm Delete", 
                                                  MessageBoxButtons.YesNo, 
                                                  MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Ensure the index is valid before removing the row
                if (e.RowIndex >= 0 && e.RowIndex < grdNewStyle.Rows.Count)
                {
                    grdNewStyle.Rows.RemoveAt(e.RowIndex); // Remove the selected row

                                DeleteRowFromDatabase(code, name, size, ean);
                }
            }
        }
    }
}

        private void loadGrid()
        {
            string queryShow = "SELECT Style.StyleNo,Style.StyleName,Style.ColourCode,Style.ColourName,Style.Size,Style.EAN,Style.Season," +
                "Customer.CusName AS CustomerName FROM Style INNER JOIN Customer ON Style.CusId = Customer.cus_id WHERE Style.StyleNo=@styleNo";
            SqlCommand showData = new SqlCommand(queryShow, con);
            showData.Parameters.AddWithValue("@styleNo", txtStyleNo.Text);
            SqlDataAdapter adapter = new SqlDataAdapter(showData);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            grdNewStyle.DataSource = dataTable;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                // Highlight the selected row
                foreach (DataGridViewRow row in grdSizes.Rows)
                {
                    // Reset row background color
                    row.DefaultCellStyle.BackColor = Color.White;
                }

                // Set the clicked row's background color to red
                grdSizes.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;

                // Fill text boxes with values from the clicked row
                DataGridViewRow selectedRow = grdSizes.Rows[e.RowIndex];
                txtColourCode.Text = selectedRow.Cells["ColourCode"].Value?.ToString() ?? string.Empty;
                txtColourName.Text = selectedRow.Cells["ColourName"].Value?.ToString() ?? string.Empty;
                txtSize.Text = selectedRow.Cells["Size"].Value?.ToString() ?? string.Empty;
                txtEAN.Text = selectedRow.Cells["EAN"].Value?.ToString() ?? string.Empty;
            }

            if (e.ColumnIndex == grdSizes.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                // Confirm deletion
                DialogResult result = MessageBox.Show("Are you sure you want to delete this row?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    grdSizes.Rows.RemoveAt(e.RowIndex); // Remove the selected row
                }
            }
        }
        private void InitializeGrid()
        {
            // Add columns to the DataGridView if they don't already exist
            if (grdSizes.Columns.Count == 0)
            {
                grdSizes.Columns.Add("ColourCode", "Colour Code");
                grdSizes.Columns.Add("ColourName", "Colour Name");
                grdSizes.Columns.Add("Size", "Size");
                grdSizes.Columns.Add("EAN", "EAN");

                // Add the Delete button column
                DataGridViewButtonColumn deleteButton = new DataGridViewButtonColumn();
                deleteButton.Name = "Delete";
                deleteButton.HeaderText = "Delete";
                deleteButton.Text = "Delete";
                deleteButton.UseColumnTextForButtonValue = true; // Show "Delete" as the button text
                grdSizes.Columns.Add(deleteButton);
            }
            grdSizes.CellClick += GrdSizes_CellClick;
        }
        private void GrdSizes_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0 && grdSizes.Columns[e.ColumnIndex].Name == "Delete")
            {
                // Show a message box
                var result = MessageBox.Show(
                    "Are you sure you want to delete this data set?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                // If the user confirms, remove the row
                if (result == DialogResult.Yes)
                {
                    grdSizes.Rows.RemoveAt(e.RowIndex);
                }
            }


            if (e.RowIndex >= 0)
            {
                // Reset background color for all rows
                foreach (DataGridViewRow row in grdSizes.Rows)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }

                // Highlight the selected row
                grdSizes.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Red;

                // Populate text boxes with the selected row's values
                DataGridViewRow selectedRow = grdSizes.Rows[e.RowIndex];
                txtColourCode.Text = selectedRow.Cells["ColourCode"].Value?.ToString() ?? string.Empty;
                txtColourName.Text = selectedRow.Cells["ColourName"].Value?.ToString() ?? string.Empty;
                txtSize.Text = selectedRow.Cells["Size"].Value?.ToString() ?? string.Empty;
                txtEAN.Text = selectedRow.Cells["EAN"].Value?.ToString() ?? string.Empty;
            }


        }

        private void btnAdd_Click(object sender, EventArgs e)
        {

            string size = txtSize.Text.Trim();
            string code = txtColourCode.Text.Trim();
            string name = txtColourName.Text.Trim();
            string ean = txtEAN.Text.Trim();

            // Ensure all fields are filled
            if (!string.IsNullOrEmpty(size) &&
                !string.IsNullOrEmpty(code) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(ean))
            {
                // Check if a row is highlighted (red background)
                DataGridViewRow highlightedRow = null;
                foreach (DataGridViewRow row in grdSizes.Rows)
                {
                    if (row.DefaultCellStyle.BackColor == Color.Red)
                    {
                        highlightedRow = row;
                        break;
                    }
                }

                if (highlightedRow != null)
                {
                    // Update the highlighted row
                    highlightedRow.Cells["ColourCode"].Value = code;
                    highlightedRow.Cells["ColourName"].Value = name;
                    highlightedRow.Cells["Size"].Value = size;
                    highlightedRow.Cells["EAN"].Value = ean;

                    // Reset row background color after update
                    highlightedRow.DefaultCellStyle.BackColor = Color.White;

                    // Provide feedback to the user
                    MessageBox.Show("Row updated successfully.");
                }
                else
                {
                    // If no row is highlighted, add a new row
                    grdSizes.Rows.Add(code, name, size, ean);

                }


                txtSize.Clear();

                txtColourCode.Focus();
            }
            else
            {
                MessageBox.Show("Please fill all fields before adding or updating a row.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbSeason_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            string size = txtSize.Text.Trim();
            string code = txtColourCode.Text.Trim();
            string name = txtColourName.Text.Trim();
            string ean = txtEAN.Text.Trim();

            // Ensure all fields are filled
            if (!string.IsNullOrEmpty(size) &&
                !string.IsNullOrEmpty(code) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(ean))
            {
                // Check if the record already exists in the grid
                bool isDuplicate = false;

                foreach (DataGridViewRow row in grdSizes.Rows)
                {
                    if (row.IsNewRow) continue; // Skip the new row

                    string existingCode = row.Cells["ColourCode"].Value?.ToString();
                    string existingName = row.Cells["ColourName"].Value?.ToString();
                    string existingSize = row.Cells["Size"].Value?.ToString();
                    string existingEAN = row.Cells["EAN"].Value?.ToString();

                    // Compare values for duplication
                    if (existingCode == code &&
                        existingName == name &&
                        existingSize == size &&
                        existingEAN == ean)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (isDuplicate)
                {
                    MessageBox.Show("This record already exists in the grid.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    // Check if a row is highlighted (red background)
                    DataGridViewRow highlightedRow = null;
                    foreach (DataGridViewRow row in grdSizes.Rows)
                    {
                        if (row.IsNewRow) continue;

                        if (row.DefaultCellStyle.BackColor == Color.Red)
                        {
                            highlightedRow = row;
                            break;
                        }
                    }

                    if (highlightedRow != null)
                    {
                        // Update the highlighted row
                        highlightedRow.Cells["ColourCode"].Value = code;
                        highlightedRow.Cells["ColourName"].Value = name;
                        highlightedRow.Cells["Size"].Value = size;
                        highlightedRow.Cells["EAN"].Value = ean;

                        // Reset row background color after update
                        highlightedRow.DefaultCellStyle.BackColor = Color.White;

                        // Provide feedback to the user
                        MessageBox.Show("Style record updated successfully.");
                    }
                    else
                    {
                        // Add a new row
                        grdSizes.Rows.Add(code, name, size, ean);
                        MessageBox.Show("New style record added successfully.");
                    }
                }

                txtColourCode.Focus();
            }
            else
            {
                MessageBox.Show("Please fill all fields before adding or updating a row.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void grdSizes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

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


        private void txtColourCode_textChange(object sender, EventArgs e)
        {
            // Your event-handling code here
        }

    }

}