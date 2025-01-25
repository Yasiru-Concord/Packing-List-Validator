using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class frm_customer_details : Form
    {
        //to store customer id that comes from frm_customer
        public int customer_id;
        SqlConnection con = new SqlConnection();

        //datatable to store customer details
        DataTable dtCustomer = new DataTable();

        //string array for store seasons details
        string[] arrSeasons;

        //datatable to store seasons 
        DataTable dtSeason = new DataTable();

        public frm_customer_details(int customer_id)
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=192.168.47.25;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            this.customer_id = customer_id;
        }

        private void frm_customer_details_Load(object sender, EventArgs e)
        {
            label1.Text = customer_id.ToString();

            String selQuery = "SELECT * FROM Customer WHERE cus_id = @cus_id";
            using(SqlCommand cmd = new SqlCommand(selQuery, con))
            {
                cmd.Parameters.AddWithValue("@cus_id", customer_id);

                try
                {
                    using (SqlDataAdapter ad = new SqlDataAdapter(cmd))
                    {
                        ad.Fill(dtCustomer);
                    }
                    //get the all seasons to the one 

                    dgvCustomer.DataSource = dtCustomer;
                    string strSeasons = dgvCustomer.Rows[0].Cells["Season"].Value.ToString();
                    arrSeasons = strSeasons.Split(new string[] { "/" }, StringSplitOptions.None);

                    arrSeasons = arrSeasons.Select(s => s.Trim()).ToArray();

                    dtSeason.Columns.Add("Season", typeof(String));
                    for(int i=0; i <= arrSeasons.Length-1; i++)
                    {
                        dtSeason.Rows.Add(arrSeasons[i]);
                    }
                    dgvSeason.DataSource = dtSeason;

                    //changing font style of the grid views
                    dgvSeason.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 10);
                    dgvCustomer.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 10);
                    dgvSeason.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);

                    //hide the season column from dgvCustomer
                    dgvCustomer.Columns["Season"].Visible = false;
                    //changing the default column header names of the dgvCustomer
                    dgvCustomer.Columns["cus_id"].HeaderText = "Customer Id";
                    dgvCustomer.Columns["cusName"].HeaderText = "Name";

                    lblTitle.Text = dgvCustomer.Rows[0].Cells["cusName"].Value.ToString() + " Details";

                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error:- ", ex.Message);
                }
            }
        }
    }
}
