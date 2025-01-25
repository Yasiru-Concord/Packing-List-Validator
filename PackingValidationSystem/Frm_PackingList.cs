using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class Frm_PackingList : Form
    {
        SqlConnection con = new SqlConnection();

        public Frm_PackingList()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            con.Open();
            cmbCustomer.SelectedValue = -1;
            LoadCustomerNames();

            // Load Packing List Data
            LoadPackingListData();

            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        // Load Customer Names into ComboBox
        private void LoadCustomerNames()
        {
            try
            {
                string query = "SELECT cus_id, CusName FROM Customer";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbCustomer.DisplayMember = "CusName";
                cmbCustomer.ValueMember = "cus_id";
                cmbCustomer.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Load Seasons based on Customer Selection
        private void LoadSeasons(int cusId)
        {
            try
            {
                string query = "SELECT Season FROM [dbo].[CustomerSeasons] WHERE cus_id = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", cusId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    cmbSeason.DataSource = dt;
                    cmbSeason.DisplayMember = "Season";
                    cmbSeason.ValueMember = "Season";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadPackingListData()
        {
            try
            {
                // Get the selected customer id and season
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                string selectedSeason = cmbSeason.SelectedValue.ToString();

                // Query to get Packing List data filtered by Customer and Season
                string query = @"
            SELECT DISTINCT [PkListNo]
            FROM [dbo].[PackingList] 
            WHERE [CustomerId] = @CustomerId AND [Season] = @Season";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CustomerId", selectedCusId);
                cmd.Parameters.AddWithValue("@Season", selectedSeason);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Binding the DataTable to DataGridView
                dataGridView1.DataSource = dt;

                // Add Status column if it doesn't exist
                if (!dataGridView1.Columns.Contains("Status"))
                {
                    DataGridViewTextBoxColumn statusColumn = new DataGridViewTextBoxColumn
                    {
                        Name = "Status",
                        HeaderText = "Status"
                    };
                    dataGridView1.Columns.Add(statusColumn);
                }

                // Add Action column (a button column) if it doesn't exist
                if (!dataGridView1.Columns.Contains("Action"))
                {
                    DataGridViewButtonColumn actionColumn = new DataGridViewButtonColumn
                    {
                        Name = "Action",
                        HeaderText = "Action",
                        Text = "View Details",
                        UseColumnTextForButtonValue = true
                    };
                    dataGridView1.Columns.Add(actionColumn);
                }

                // Iterate through each PkListNo in the DataGridView and calculate the Status
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["PkListNo"].Value == null) continue;

                    string pkListNo = row.Cells["PkListNo"].Value.ToString();

                    // Query to get all statuses for the current PkListNo
                    string statusQuery = @"
                SELECT [Status]
                FROM [dbo].[PackingList]
                WHERE [PkListNo] = @PkListNo";

                    SqlCommand statusCmd = new SqlCommand(statusQuery, con);
                    statusCmd.Parameters.AddWithValue("@PkListNo", pkListNo);

                    SqlDataAdapter statusDa = new SqlDataAdapter(statusCmd);
                    DataTable statusDt = new DataTable();
                    statusDa.Fill(statusDt);

                    // Determine the aggregated status for the current PkListNo
                    bool allCompleted = true;
                    bool allNotStarted = true;

                    foreach (DataRow statusRow in statusDt.Rows)
                    {
                        string status = statusRow["Status"]?.ToString() ?? "";

                        if (status != "Completed") allCompleted = false;
                        if (status != "Not Yet Started") allNotStarted = false;
                    }

                    // Set the Status column value based on the aggregated statuses
                    if (allCompleted)
                    {
                        row.Cells["Status"].Value = "Scanned Completed";
                    }
                    else if (allNotStarted)
                    {
                        row.Cells["Status"].Value = "Not Yet Scanned";
                    }
                    else
                    {
                        row.Cells["Status"].Value = "Pending Scanning";
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log or display the error
                //MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Handle Customer ComboBox selection change
        private void cmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null)
            {
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                LoadSeasons(selectedCusId);

                // Load the Packing List data when a customer is selected
                LoadPackingListData();
            }
        }

        // Handle Season ComboBox selection change
        private void cmbSeason_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSeason.SelectedValue != null)
            {
                // Load the Packing List data when a season is selected
                LoadPackingListData();
            }
        }

        // Open the Packing List Details form when a button is clicked (example)
        private void button1_Click(object sender, EventArgs e)
        {
            Frm_PackingListDetails frm = new Frm_PackingListDetails();
            frm.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = textBox1.Text.Trim();
            LoadPackingListData(searchTerm);
        }

        private void LoadPackingListData(string searchTerm = "")
        {
            try
            {
                // Ensure a valid customer and season are selected
                if (cmbCustomer.SelectedValue == null || cmbSeason.SelectedValue == null)
                {
                    MessageBox.Show("Please select a customer and a season first.",
                                    "Missing Selection",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected customer ID and season
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                string selectedSeason = cmbSeason.SelectedValue.ToString();

                // SQL query to load data based on the search term
                string query = @"
            SELECT DISTINCT [PkListNo]
            FROM [dbo].[PackingList] 
            WHERE [CustomerId] = @CustomerId 
              AND [Season] = @Season
              AND [PkListNo] LIKE @SearchTerm";

                // Create a SqlCommand
                SqlCommand cmd = new SqlCommand(query, con);

                // Add parameters to prevent SQL injection
                cmd.Parameters.AddWithValue("@CustomerId", selectedCusId);
                cmd.Parameters.AddWithValue("@Season", selectedSeason);
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");

                // Execute the query and fetch data into a DataTable
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Bind the fetched data to the DataGridView
                dataGridView1.DataSource = dt;

                // Check if no records were found
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No matching records found for the entered PkListNo.",
                                    "Search Result",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }

                // Ensure the Status column exists in the DataGridView
                if (!dataGridView1.Columns.Contains("Status"))
                {
                    DataGridViewTextBoxColumn statusColumn = new DataGridViewTextBoxColumn
                    {
                        Name = "Status",
                        HeaderText = "Status"
                    };
                    dataGridView1.Columns.Add(statusColumn);
                }

                // Ensure the Action button column exists in the DataGridView
                if (!dataGridView1.Columns.Contains("Action"))
                {
                    DataGridViewButtonColumn actionColumn = new DataGridViewButtonColumn
                    {
                        Name = "Action",
                        HeaderText = "Action",
                        Text = "View Details",
                        UseColumnTextForButtonValue = true
                    };
                    dataGridView1.Columns.Add(actionColumn);
                }

                // Iterate through each row in the DataGridView to calculate the status
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["PkListNo"].Value == null) continue;

                    string pkListNo = row.Cells["PkListNo"].Value.ToString();

                    // Query to get all statuses for the current PkListNo
                    string statusQuery = @"
                SELECT [Status]
                FROM [dbo].[PackingList]
                WHERE [PkListNo] = @PkListNo";

                    SqlCommand statusCmd = new SqlCommand(statusQuery, con);
                    statusCmd.Parameters.AddWithValue("@PkListNo", pkListNo);

                    SqlDataAdapter statusDa = new SqlDataAdapter(statusCmd);
                    DataTable statusDt = new DataTable();
                    statusDa.Fill(statusDt);

                    // Determine the aggregated status for the current PkListNo
                    bool allCompleted = true;
                    bool allNotStarted = true;

                    foreach (DataRow statusRow in statusDt.Rows)
                    {
                        string status = statusRow["Status"]?.ToString() ?? "";

                        if (status != "Completed") allCompleted = false;
                        if (status != "Not Yet Started") allNotStarted = false;
                    }

                    // Set the Status column value based on the aggregated statuses
                    if (allCompleted)
                    {
                        row.Cells["Status"].Value = "Scanned Completed";
                    }
                    else if (allNotStarted)
                    {
                        row.Cells["Status"].Value = "Not Yet Scanned";
                    }
                    else
                    {
                        row.Cells["Status"].Value = "Pending Scanning";
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or display the error message
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
          
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure a valid row is clicked
            if (e.RowIndex >= 0)
            {
                // Check if the clicked column is "Action"
                if (dataGridView1.Columns[e.ColumnIndex].Name == "Action")
                {
                    string pkListNo = dataGridView1.Rows[e.RowIndex].Cells["PkListNo"].Value?.ToString();
                    if (!string.IsNullOrEmpty(pkListNo))
                    {
                        // Open Frm_PKListView form when Action column is clicked
                        Frm_PKListView detailsForm = new Frm_PKListView(pkListNo);
                        detailsForm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Invalid PkListNo selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Check if the clicked column is "PkListNo"
                else if (dataGridView1.Columns[e.ColumnIndex].Name == "PkListNo")
                {
                    string pkListNo = dataGridView1.Rows[e.RowIndex].Cells["PkListNo"].Value?.ToString();
                    if (!string.IsNullOrEmpty(pkListNo))
                    {
                        // Open Frm_ScanPkList form when PkListNo column is clicked
                        Frm_ScanPkList frmScan = new Frm_ScanPkList(pkListNo);
                        frmScan.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Invalid PkListNo selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            string searchTerm = textBox1.Text.Trim();
            LoadPackingListData(searchTerm);
        }
    }
}
