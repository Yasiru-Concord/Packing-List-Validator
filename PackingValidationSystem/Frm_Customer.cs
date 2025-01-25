using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class Frm_Customer : Form
    {
        SqlConnection con = new SqlConnection();
        private int selectedCustomerId;

        // to set value to public customer id
        public int customer_id { get; set; }

        // data table for store customer data
        DataTable dataTable = new DataTable();
        private DataTable dtSeason = new DataTable();
        private bool isErrorMessageShown = false;


        public Frm_Customer()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            con.Open();

            dgvSeasons.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvSeasons.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvSeasons.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);
            dgvSeasons.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dgvSeasons.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dgvSeasons.Refresh();
        }

        private void Frm_Customer_Load(object sender, EventArgs e)
        {
            LoadData();
            dgvCustomer.CellClick += dgvCustomer_CellClick;
            dgvSeasons.CellClick += dgvSeasons_CellClick;
            txtSearch.TextChanged += txtSearch_TextChanged;
            SetDataGridViewStyle();

            // add columns to the season datatable
            dtSeason.Columns.Add("Destination", typeof(string));
            dtSeason.Columns.Add("Season", typeof(string));
            txtCusName.TextChanged += txtCusName_TextChanged;
            txtSeason.TextChanged += txtSeason_TextChanged;

        }

        private void SetDataGridViewStyle()
        {
            dgvCustomer.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvCustomer.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvCustomer.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);
            dgvCustomer.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dgvCustomer.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dgvCustomer.Refresh();
        }

        private void LoadData()
        {
            // SQL query to select all customers
            string query = "SELECT * FROM Customer";

            // Create a new DataTable to load fresh data
            DataTable dataTable = new DataTable();

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                try
                {
                    // Fill the DataTable with customer data
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);

                    // If there is an existing BindingSource, bind the DataTable to it
                    BindingSource bindingSource = new BindingSource();
                    bindingSource.DataSource = dataTable;
                    dgvCustomer.DataSource = bindingSource;

                    // Optionally, hide the unneeded columns
                    dgvCustomer.Columns["cus_id"].Visible = false;

                    // Set column header text
                    dgvCustomer.Columns["type"].HeaderText = "Customer Type";
                    //dgvCustomer.Columns["type"].Visible = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // season concatenation
            int rowCount = dtSeason.Rows.Count;
            string strSeason = "";
            if (rowCount > 0)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    strSeason = strSeason + dtSeason.Rows[i]["Season"].ToString() + "/ ";
                }
            }
            else
            {
                MessageBox.Show("Season is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate inputs before saving
            if (string.IsNullOrWhiteSpace(txtCusName.Text))
            {
                MessageBox.Show("Customer name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check for duplicate customer name
            if (IsDuplicateCustomerName(txtCusName.Text))
            {
                MessageBox.Show("A customer with this name already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!radiobtnInternal.Checked && !radiobtnExternal.Checked)
            {
                MessageBox.Show("Please select a customer type (Internal or External).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            // Confirm save operation
            DialogResult result = MessageBox.Show("Are you sure you want to save this data?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string cusName = txtCusName.Text;
                string type = radiobtnInternal.Checked ? "Internal" : "External";

                // SQL query to insert a new customer
                string insertCustomerQuery = "INSERT INTO Customer (CusName, Type) VALUES (@CusName, @Type); SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(insertCustomerQuery, con))
                {
                    cmd.Parameters.AddWithValue("@CusName", cusName);
                    cmd.Parameters.AddWithValue("@Type", type);

                    try
                    {
                        // Execute the query and get the inserted customer's ID
                        int newCustomerId = Convert.ToInt32(cmd.ExecuteScalar());

                        // Insert corresponding seasons into CustomerSeasons table
                        foreach (DataRow row in dtSeason.Rows)
                        {
                            string seasonValue = row["Season"].ToString();
                            string destinationValue = row["Destination"].ToString();

                            string insertSeasonQuery = "INSERT INTO CustomerSeasons (cus_id, Season, Destination) VALUES (@cus_id, @Season, @Destination)";

                            using (SqlCommand seasonCmd = new SqlCommand(insertSeasonQuery, con))
                            {
                                seasonCmd.Parameters.AddWithValue("@cus_id", newCustomerId);
                                seasonCmd.Parameters.AddWithValue("@Season", seasonValue);
                                seasonCmd.Parameters.AddWithValue("@Destination", destinationValue);

                                seasonCmd.ExecuteNonQuery(); // Insert the season into the CustomerSeasons table
                            }
                        }

                        MessageBox.Show($"{cusName} Customer Create Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData(); // Refresh the DataGridView
                        ClearFields();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error Saving Data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Save operation canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool IsDuplicateCustomerName(string customerName)
        {
            // SQL query to count customers with the same name
            string query = "SELECT COUNT(*) FROM Customer WHERE CusName = @CusName";

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@CusName", customerName);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the clicked row index is valid
            if (e.RowIndex >= 0 && e.RowIndex < dgvCustomer.Rows.Count)
            {
                foreach (DataGridViewRow row in dgvCustomer.Rows)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }

                DataGridViewRow selectedRow = dgvCustomer.Rows[e.RowIndex];

                // Highlight the selected row
                selectedRow.DefaultCellStyle.BackColor = Color.FromArgb(154,154,154);
                selectedCustomerId = Convert.ToInt32(selectedRow.Cells["cus_id"].Value);

                // Autofill customer information
                txtCusName.Text = selectedRow.Cells["CusName"].Value.ToString();
                string type = selectedRow.Cells["Type"].Value.ToString();
                if (type == "Internal")
                {
                    radiobtnInternal.Checked = true;
                }
                else
                {
                    radiobtnExternal.Checked = true;
                }

                // Load the corresponding seasons for this customer
                LoadCustomerSeasons();
            }
            else
            {
                MessageBox.Show("Invalid row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void LoadCustomerSeasons()
        {
            // Ensure dtSeason is initialized
            if (dtSeason == null)
            {
                dtSeason = new DataTable();
            }

            // Clear previous data in dtSeason
            dtSeason.Rows.Clear();

            // SQL query to fetch seasons for the selected customer
            string query = "SELECT * FROM CustomerSeasons WHERE cus_id = @CusId";

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CusId", selectedCustomerId);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Ensure data is returned
                    if (dt.Rows.Count > 0)
                    {
                        // Add the data fetched from the database to dtSeason
                        foreach (DataRow row in dt.Rows)
                        {
                            dtSeason.Rows.Add(row["Destination"], row["Season"]);
                        }

                        // Bind dgvSeasons to dtSeason
                        dgvSeasons.DataSource = dtSeason;

                        // Ensure Delete button column exists and is positioned correctly
                        if (!dgvSeasons.Columns.Contains("Delete"))
                        {
                            DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn
                            {
                                Name = "Delete",
                                HeaderText = "Action",
                                Text = "Delete",
                                UseColumnTextForButtonValue = true,
                                Width = 75
                            };

                            // Add the Delete column
                            dgvSeasons.Columns.Add(deleteButtonColumn);
                        }

                        // Ensure Delete column is positioned last
                        dgvSeasons.Columns["Delete"].DisplayIndex = dgvSeasons.Columns.Count - 1;
                    }
                    else
                    {
                        MessageBox.Show("No seasons found for the selected customer.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customer seasons: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Check if both Season and Destination fields have values
            if (!string.IsNullOrEmpty(txtSeason.Text) && !string.IsNullOrEmpty(txtDestination.Text))
            {
                bool rowUpdated = false;

                // Check if any row has red background color (indicating it's selected)
                foreach (DataGridViewRow row in dgvSeasons.Rows)
                {
                    if (row.DefaultCellStyle.BackColor == Color.FromArgb(154, 154, 154))
                    {
                        // Update the selected row with new values from the textboxes
                        row.Cells["Season"].Value = txtSeason.Text;
                        row.Cells["Destination"].Value = txtDestination.Text;

                        // Reset the background color of the updated row to white
                        row.DefaultCellStyle.BackColor = Color.White;

                        rowUpdated = true;
                        break; // Exit the loop once we update the row
                    }
                }

                // If no red row was found, add a new row to the DataGridView
                if (!rowUpdated)
                {
                    dtSeason.Rows.Add(txtDestination.Text, txtSeason.Text);

                    // Optionally update the database (if needed), or just refresh the DataGridView
                    dgvSeasons.DataSource = dtSeason;  // Bind the DataGridView to the updated DataTable
                }

                // Clear the textboxes for the next entry
                txtSeason.Clear();
                txtDestination.Clear();
            }
            else
            {
                MessageBox.Show("Please enter both Season and Destination.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Concatenate seasons
            int rowCount = dtSeason.Rows.Count;
            string strSeason = "";
            if (dtSeason.Rows.Count > 0)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    strSeason = strSeason + dtSeason.Rows[i]["Season"].ToString() + "/ ";
                }
            }
            else
            {
                MessageBox.Show("Season is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to update this data?", "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                string cusName = txtCusName.Text;
                string type = radiobtnInternal.Checked ? "Internal" : "External";

                // SQL query to update the selected customer
                string query = "UPDATE Customer SET CusName = @CusName, Type = @Type WHERE cus_id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CusName", cusName);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@Id", selectedCustomerId);

                    try
                    {
                        // Update customer information
                        cmd.ExecuteNonQuery();

                        // Delete old seasons from the CustomerSeasons table
                        string deleteSeasonsQuery = "DELETE FROM CustomerSeasons WHERE cus_id = @cus_id";
                        using (SqlCommand deleteCmd = new SqlCommand(deleteSeasonsQuery, con))
                        {
                            deleteCmd.Parameters.AddWithValue("@cus_id", selectedCustomerId);
                            deleteCmd.ExecuteNonQuery();
                        }

                        // Insert new seasons into CustomerSeasons table
                        foreach (DataRow row in dtSeason.Rows)
                        {
                            string seasonValue = row["Season"].ToString();
                            string destinationValue = row["Destination"].ToString();

                            string insertSeasonQuery = "INSERT INTO CustomerSeasons (cus_id, Season, Destination) VALUES (@cus_id, @Season, @Destination)";

                            using (SqlCommand seasonCmd = new SqlCommand(insertSeasonQuery, con))
                            {
                                seasonCmd.Parameters.AddWithValue("@cus_id", selectedCustomerId);
                                seasonCmd.Parameters.AddWithValue("@Season", seasonValue);
                                seasonCmd.Parameters.AddWithValue("@Destination", destinationValue);
                                seasonCmd.ExecuteNonQuery(); // Insert the new season into the CustomerSeasons table
                            }
                        }

                        MessageBox.Show($"{cusName} Customer Updated Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData(); // Refresh the DataGridView with updated data
                        ClearFields();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Update operation canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCustomerId == 0) // Check if a customer is selected
            {
                MessageBox.Show("Please select a customer to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this customer?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // SQL query to delete the selected customer
                string query = "DELETE FROM Customer WHERE cus_id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Id", selectedCustomerId);

                    try
                    {
                        cmd.ExecuteNonQuery();

                        // Delete the corresponding seasons from the CustomerSeasons table
                        string deleteSeasonsQuery = "DELETE FROM CustomerSeasons WHERE cus_id = @cus_id";
                        using (SqlCommand deleteCmd = new SqlCommand(deleteSeasonsQuery, con))
                        {
                            deleteCmd.Parameters.AddWithValue("@cus_id", selectedCustomerId);
                            deleteCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show($"Customer Deleted Successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData(); // Refresh the DataGridView
                        ClearFields();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Delete operation canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dgvSeasons_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure that the clicked row is valid (not a header row)
            if (e.RowIndex >= 0)
            {
                // Reset the background color of all rows to white
                foreach (DataGridViewRow row in dgvSeasons.Rows)
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                }

                // Highlight the selected row
                dgvSeasons.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(154, 154, 154);

                // Get the season and destination values from the selected row
                string season = dgvSeasons.Rows[e.RowIndex].Cells["Season"].Value?.ToString();
                string destination = dgvSeasons.Rows[e.RowIndex].Cells["Destination"].Value?.ToString();

                // Autofill the textboxes with the selected row data
                txtSeason.Text = season;
                txtDestination.Text = destination;

                // Check if the clicked column is the 'Delete' button column
                if (e.ColumnIndex == dgvSeasons.Columns["Delete"].Index)
                {
                    // Confirm the delete operation
                    DialogResult result = MessageBox.Show($"Are you sure you want to delete the season '{season}' for destination '{destination}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Remove the row from the DataGridView and the DataTable
                        dgvSeasons.Rows.RemoveAt(e.RowIndex);

                        // Remove the corresponding row from the dtSeason DataTable
                        var rowToDelete = dtSeason.AsEnumerable()
                                                  .FirstOrDefault(r => r["Season"].ToString() == season && r["Destination"].ToString() == destination);
                        if (rowToDelete != null)
                        {
                            dtSeason.Rows.Remove(rowToDelete);
                        }

                        MessageBox.Show("Season removed from the grid.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }


        private void ClearFields()
        {
            // Clear all textboxes
            txtCusName.Clear();
            txtSeason.Clear();
            txtDestination.Clear();

            // Uncheck the radio buttons
            radiobtnInternal.Checked = false;
            radiobtnExternal.Checked = false;

            // Clear the seasons DataGridView
            dtSeason.Clear();
            dgvSeasons.DataSource = null;
        }

        // For dgvCustomer_CellClick_1
        private void dgvCustomer_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
        }

        // For dgvCustomer_CellContentDoubleClick
        private void dgvCustomer_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        // For dgvSeasons_CellContentDoubleClick
        private void dgvSeasons_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void btnSeason_Click(object sender, EventArgs e)
        {
        }

        private void dgvSeasons_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Get the search term from the textbox
            string searchTerm = txtSearch.Text.Trim();

            // Load customer data based on the search term
            LoadCustomerData(searchTerm);
        }

        private void LoadCustomerData(string searchTerm = "")
        {
            try
            {
                string query = "SELECT [cus_id], [CusName] FROM [dbo].[Customer]";

                // If searchTerm is not empty, filter the results using LIKE
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE [CusName] LIKE @SearchTerm";
                }

                // Create a SqlDataAdapter with the query and connection
                SqlDataAdapter dataAdapter = new SqlDataAdapter(query, con);

                // If there is a search term, add it to the parameters for the query
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                }

                // Create a DataTable to hold the result
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                // Check if there are no rows in the result
                if (dataTable.Rows.Count == 0)
                {
                    if (dgvCustomer.DataSource != null)
                    {
                        dgvCustomer.DataSource = new DataTable(); // Clear the grid safely
                    }

                    // Show the message box only once
                    MessageBox.Show("No matching customer records found.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtSearch.Text = "";
                    return;
                }

                // Bind the result to the DataGridView
                dgvCustomer.DataSource = dataTable;

                // Optionally, hide the "cus_id" column if not needed for display
                dgvCustomer.Columns["cus_id"].Visible = false;
            }
            catch (Exception ex)
            {
                // Handle any errors that may occur
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                MessageBox.Show("Please enter a customer name to search.");
                return;
            }

            // Load customer data based on the search term
            LoadCustomerData(searchTerm);
        }

        private void txtCusName_TextChanged(object sender, EventArgs e)
        {
            // Get the input text
            string inputText = txtCusName.Text;

            // Remove numeric characters (keep only non-numeric characters)
            string nonNumericText = new string(inputText.Where(c => !char.IsDigit(c)).ToArray());

            // Check if the input contains any numeric characters
            if (inputText != nonNumericText)
            {
                // If the user entered numeric characters, update the text to only include non-numeric characters
                txtCusName.Text = nonNumericText;

                // Optionally, place the cursor at the end of the text
                txtCusName.SelectionStart = txtCusName.Text.Length;

                // Show the error message only once
                if (!isErrorMessageShown)
                {
                    MessageBox.Show("Customer name must contain only alphabetic characters and spaces.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isErrorMessageShown = true;
                }
            }
            else
            {
                // Reset the error message flag if no numeric characters are entered
                isErrorMessageShown = false;
            }
        }

        private void txtDestination_TextChanged(object sender, EventArgs e)
        {
            // Get the input text
            string inputText = txtDestination.Text;

            // Remove numeric characters (keep only non-numeric characters)
            string nonNumericText = new string(inputText.Where(c => !char.IsDigit(c)).ToArray());

            // Check if the input contains any numeric characters
            if (inputText != nonNumericText)
            {
                // If the user entered numeric characters, update the text to only include non-numeric characters
                txtDestination.Text = nonNumericText;

                // Optionally, place the cursor at the end of the text
                txtDestination.SelectionStart = txtDestination.Text.Length;

                // Show the error message only once
                if (!isErrorMessageShown)
                {
                    MessageBox.Show("Destination name must contain only alphabetic characters and spaces.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isErrorMessageShown = true;
                }
            }
            else
            {
                // Reset the error message flag if no numeric characters are entered
                isErrorMessageShown = false;
            }
        }

        private void txtSeason_TextChanged(object sender, EventArgs e)
        {
            // Get the input text
            string inputText = txtSeason.Text;

            // Remove numeric characters (keep only non-numeric characters)
            string nonNumericText = new string(inputText.Where(c => !char.IsDigit(c)).ToArray());

            // Check if the input contains any numeric characters
            if (inputText != nonNumericText)
            {
                // If the user entered numeric characters, update the text to only include non-numeric characters
                txtSeason.Text = nonNumericText;

                // Optionally, place the cursor at the end of the text
                txtSeason.SelectionStart = txtSeason.Text.Length;

                // Show the error message only once
                if (!isErrorMessageShown)
                {
                    MessageBox.Show("Season name must contain only alphabetic characters and spaces.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isErrorMessageShown = true;
                }
            }
            else
            {
                // Reset the error message flag if no numeric characters are entered
                isErrorMessageShown = false;
            }
        }
    }
}
