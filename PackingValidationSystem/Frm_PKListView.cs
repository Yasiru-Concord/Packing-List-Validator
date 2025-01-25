using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class Frm_PKListView : Form
    {
        private string pkListNo; // To store the passed PkListNo
        private SqlConnection con; // To manage the connection

        public Frm_PKListView(string pkListNo)
        {
            InitializeComponent();
            this.pkListNo = pkListNo;

            // Initialize and open the SQL connection
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            try
            {
                con.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to the database: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Connect the Load event
            this.Load += Frm_PKListView_Load;


            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.ForeColor = Color.FromArgb(69, 14, 123);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            dataGridView1.Refresh();

        }

        private void Frm_PKListView_Load(object sender, EventArgs e)
        {
            LoadPackingListDetails();
        }

        private void LoadPackingListDetails()
        {
            try
            {
                // Query to fetch details for the passed PkListNo
                string query = @"
                SELECT [PkListNo]
                      ,[FactoryCode] AS FactoryName
                      ,[CustomerId]
                      ,[Season]
                      ,[ActualDeliveryDate]
                      ,[PODeliveryDate]
                      ,[PONo]
                      ,[Status]
                FROM [dbo].[PackingList]
                WHERE [PkListNo] = @PkListNo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PkListNo", pkListNo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Bind the results to the DataGridView
                dataGridView1.DataSource = dt;

                dataGridView1.Columns["CustomerId"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading details: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Ensure the connection is closed when the form is closed
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (con != null && con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }
    }
}
