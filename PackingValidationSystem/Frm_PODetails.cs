using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class Frm_PODetails : Form
    {
        SqlConnection con = new SqlConnection();

        public Frm_PODetails()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            con.Open();
            LoadCustomerNames();  // Load customers when form loads
            InitializeDataGridView();  // Initialize DataGridView columns
            LoadYears();  // Load years into cmbYear
        }

        // Initialize DataGridView columns and styles
        private void InitializeDataGridView()
        {
            dgvViewPO.Columns.Add("PONo", "PONo");
            dgvViewPO.Columns.Add("StyleNo", "StyleNo");
            dgvViewPO.Columns.Add("StyleName", "StyleName");
            dgvViewPO.Columns.Add("Season", "Season");
            dgvViewPO.Columns.Add("OrderType", "OrderType");
            dgvViewPO.Columns.Add("ShipMood", "ShipMood");
            dgvViewPO.Columns.Add("Tolerance", "Tolerance(%)");
            dgvViewPO.Columns.Add("DeliveryDate", "DeliveryDate");
            dgvViewPO.Columns.Add("CreatedPODate", "CreatedPODate");

            DataGridViewButtonColumn viewButtonColumn = new DataGridViewButtonColumn
            {
                Name = "ViewButton",
                Text = "View",
                UseColumnTextForButtonValue = true,
                DefaultCellStyle = { BackColor = ColorTranslator.FromHtml("#f0ab00") }
            };

            dgvViewPO.Columns.Add(viewButtonColumn);

            Font gridFont = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvViewPO.DefaultCellStyle.Font = gridFont;
            dgvViewPO.ColumnHeadersDefaultCellStyle.Font = gridFont;

            dgvViewPO.CellContentClick += dgvViewPO_CellContentClick;
        }

        // Load Customer Names into ComboBox
        private void LoadCustomerNames()
        {
            try
            {
                string query = "SELECT cus_id, CusName FROM [Customer]";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbCustomer.DataSource = dt;
                cmbCustomer.DisplayMember = "CusName";
                cmbCustomer.ValueMember = "cus_id";
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

                cmbSeason.DataSource = dt;
                cmbSeason.DisplayMember = "Season";
                cmbSeason.ValueMember = "Season";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Load years dynamically starting from the current year
        private void LoadYears()
        {
            try
            {
                int currentYear = DateTime.Now.Year;
                DataTable dt = new DataTable();
                dt.Columns.Add("Year", typeof(int));

                for (int year = currentYear; year <= currentYear + 50; year++)
                {
                    dt.Rows.Add(year);
                }

                cmbYear.DataSource = dt;
                cmbYear.DisplayMember = "Year";
                cmbYear.ValueMember = "Year";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Method to load PO details for the selected customer, season, and year
        private void LoadPODetails(int cusId, string season, int year)
        {
            try
            {
                dgvViewPO.Rows.Clear();

                string query = @"
                    SELECT DISTINCT
                        PO.Season, 
                        Style.StyleNo,
                        Style.StyleName,
                        PO.PONo, 
                        PO.OrderType, 
                        PO.ShipMethod,
                        PO.Tolerance,
                        PO.DeliveryDate,
                        PO.Date
                    FROM 
                        [PO] AS PO
                    INNER JOIN 
                        [PODetails] AS PD ON PO.PONo = PD.PONumber
                    INNER JOIN 
                        [Style] AS Style ON PD.StyleId = Style.StyleNo
                    WHERE 
                        PO.CustomerId = @CusId AND 
                        PO.Season = @Season AND
                        YEAR(PO.Date) = @Year;";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CusId", cusId);
                cmd.Parameters.AddWithValue("@Season", season);
                cmd.Parameters.AddWithValue("@Year", year);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    dgvViewPO.Rows.Add(
                        row["PONo"],
                        row["StyleNo"],
                        row["StyleName"],
                        row["Season"],
                        row["OrderType"],
                        row["ShipMethod"],
                        row["Tolerance"],
                        row["DeliveryDate"],
                        row["Date"]
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Event handlers
        private void cmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null)
            {
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                LoadSeasons(selectedCusId);
            }
        }
        // Method to load PO details for the selected customer and season
        private void LoadPODetails(int cusId, string season)
        {
            try
            {
                // Clear existing rows in the DataGridView
                dgvViewPO.Rows.Clear();

                // Query to fetch PO details for the selected customer and season
                string query = @"
                  SELECT DISTINCT
    PO.Season, 
    Style.StyleNo,
    Style.StyleName,
    PO.PONo, 
    PO.OrderType, 
    PO.ShipMethod,
    PO.Tolerance,
    PO.DeliveryDate
FROM 
    [PO] AS PO
INNER JOIN 
    [PODetails] AS PD ON PO.PONo = PD.PONumber
INNER JOIN 
    [Style] AS Style ON PD.StyleId = Style.StyleNo
WHERE 
    PO.CustomerId = @CusId AND 
    PO.Season = @Season;
";  // Filter by both CustomerId and Season

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CusId", cusId);
                cmd.Parameters.AddWithValue("@Season", season);  // Pass the selected season as parameter

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Populate DataGridView with the PO details
                foreach (DataRow row in dt.Rows)
                {
                    dgvViewPO.Rows.Add(
                        row["PONo"],
                        row["StyleNo"],
                        row["StyleName"],
                        row["Season"],
                        row["OrderType"],
                        row["ShipMethod"],
                        row["Tolerance"],
                        row["DeliveryDate"],
                        "N/A"  // Set default value for CreatedPkList column
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void cmbSeason_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if both Customer and Season are selected
            if (cmbCustomer.SelectedValue != null && cmbSeason.SelectedValue != null)
            {
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                string selectedSeason = cmbSeason.SelectedValue.ToString();
       /*         LoadPODetails(selectedCusId, selectedSeason); */ // Load PO details for the selected customer and season
            }
        }

        private void cmbYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null && cmbSeason.SelectedValue != null && cmbYear.SelectedValue != null)
            {
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                string selectedSeason = cmbSeason.SelectedValue.ToString();
                int selectedYear = Convert.ToInt32(cmbYear.SelectedValue);

                LoadPODetails(selectedCusId, selectedSeason, selectedYear);
            }
        }

        private void dgvViewPO_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == dgvViewPO.Columns["ViewButton"].Index)
            {
                string poNo = dgvViewPO.Rows[e.RowIndex].Cells["PONo"].Value.ToString();
                MessageBox.Show($"View details for PO No: {poNo}");

                Frm_PO frmPO = new Frm_PO(poNo);
                frmPO.ShowDialog();
            }
        }
    }
}
