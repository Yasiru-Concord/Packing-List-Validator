using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class NewCustomer : Form
    {
        SqlConnection con = new SqlConnection();
        DataTable dataTable = new DataTable();

        public NewCustomer()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            SetDataGridViewStyle();
            LoadData();
            dgvCustomer.CellClick += dgvCustomer_CellClick;
            txtSearch.TextChanged += txtSearch_TextChanged;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            Frm_Customer cus = new Frm_Customer();
            cus.ShowDialog();
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

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                try
                {
                    // Fill the DataGridView with customer data
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dataTable);
                    dgvCustomer.DataSource = dataTable;

                    //// Set header text for 'type' column
                    //dgvCustomer.Columns["type"].HeaderText = "Customer Type";

                    // Hide unnecessary columns
                    dgvCustomer.Columns["cus_id"].Visible = false;
 
                    dgvCustomer.Columns["type"].Visible = false;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
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
                    dgvCustomer.Columns["Type"].Visible = false;

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Get the search term from the textbox
            string searchTerm = txtSearch.Text.Trim();

            // Load customer data based on the search term
            LoadCustomerData(searchTerm);
        }

        private void dgvCustomer_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }
        private void dgvCustomer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure that the click is on a valid row (not on the header)
            if (e.RowIndex >= 0)
            {
                // Get the selected customer ID from the clicked row
                int customerId = Convert.ToInt32(dgvCustomer.Rows[e.RowIndex].Cells["cus_id"].Value);

                // Create a new instance of Frm_CusDetails and pass the customer ID to it
                Frm_CusDetails cusDetailsForm = new Frm_CusDetails(customerId);
                cusDetailsForm.ShowDialog();
            }
        }
    }
}
