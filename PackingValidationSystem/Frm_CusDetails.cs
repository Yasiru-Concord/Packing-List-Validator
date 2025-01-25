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

namespace PackingValidationSystem
{
    public partial class Frm_CusDetails : Form
    {
        private int customerId;

        public Frm_CusDetails(int cusId)
        {
            InitializeComponent();
            customerId = cusId;
            LoadCustomerDetails();

            dgvDestinationList.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvDestinationList.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvDestinationList.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);
            dgvDestinationList.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dgvDestinationList.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dgvDestinationList.Refresh();
        }

        private void LoadCustomerDetails()
        {
            try
            {
                // Create connection to the database
                using (SqlConnection con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;"))
                {
                    // SQL query to get customer details from the Customer table
                    string customerQuery = "SELECT [cus_id], [CusName], [Type] FROM [dbo].[Customer] WHERE [cus_id] = @CustomerId";
                    SqlDataAdapter customerAdapter = new SqlDataAdapter(customerQuery, con);
                    customerAdapter.SelectCommand.Parameters.AddWithValue("@CustomerId", customerId);

                    DataTable customerTable = new DataTable();
                    customerAdapter.Fill(customerTable);

                    if (customerTable.Rows.Count > 0)
                    {
                        // Display customer details
                        txtCusName.Text = customerTable.Rows[0]["CusName"].ToString();
                        txtType.Text = customerTable.Rows[0]["Type"].ToString();
                    }

                    // SQL query to get customer seasons from the CustomerSeasons table
                    string seasonQuery = "SELECT [Destination],[Season] FROM [dbo].[CustomerSeasons] WHERE [cus_id] = @CustomerId";
                    SqlDataAdapter seasonAdapter = new SqlDataAdapter(seasonQuery, con);
                    seasonAdapter.SelectCommand.Parameters.AddWithValue("@CustomerId", customerId);

                    DataTable seasonTable = new DataTable();
                    seasonAdapter.Fill(seasonTable);

                    // Bind data to the DataGridView (dgvDestinationList)
                    dgvDestinationList.DataSource = seasonTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
