using PackingValidationSystem;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Windows.Forms;
namespace SmartPacker
{
    public partial class Frm_Factory : Form

    {
        SqlConnection con = new SqlConnection();

        public Frm_Factory()
        {
            InitializeComponent();
            //con.ConnectionString = @"Data Source=(localdb)\Local;Initial Catalog=marker_making_demo;Integrated Security=True;";
            //con = new SqlConnection(@"Data Source=192.168.47.25;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;TrustServerCertificate=True;");
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;TrustServerCertificate=True;");
            con.Open();
            LoadData();
            btnDelete.Visible = false;
            btnUpdate.Visible = false;
            grdFactory.Columns["FactoryCode"].HeaderText = "Factory Code";
            grdFactory.Columns["FactoryName"].HeaderText = "Factory Name";
            btnDelete.Visible = false;
            btnUpdate.Visible = false;

            grdFactory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdFactory.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grdFactory.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);
            grdFactory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            grdFactory.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            grdFactory.Refresh();

        }

        private void LoadData()
        {
            String queryRead = "SELECT * FROM Factory";
            DataTable dataTable = new DataTable();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(queryRead, con);
                adapter.Fill(dataTable);
                grdFactory.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Frm_New_Factory new_Factory = new Frm_New_Factory();
            new_Factory.Show();
        }
        private void txtFactoryCode_TextChanged(object sender, EventArgs e)
        {
        }
        private void txtFactoryName_TextChanged(object sender, EventArgs e)
        {
        }
        private void grdFactory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            btnDelete.Visible = true;
            btnUpdate.Visible = true;
            txtFactoryCode.Visible = true;
            txtFactoryName.Visible = true;
        }

        private void frmFactory_Load(object sender, EventArgs e)
        {

        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Are you sure you want to delete?");
            String queryDelete = "DELETE FROM Factory WHERE FactoryCode=@factoryCode";
            SqlCommand cmd = new SqlCommand(queryDelete, con);
            cmd.Parameters.AddWithValue("@factoryCode", txtFactoryCode.Text);
            cmd.ExecuteNonQuery();
            MessageBox.Show("Factory Delete Successfully");
            txtFactoryCode.Clear();
            txtFactoryName.Clear();
            LoadData();

        }

        private void grdFactory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            groupBox1.Text = "Edit Factory";
            btnDelete.Visible = true;
            btnUpdate.Visible = true;
            txtFactoryCode.ReadOnly = true;
            txtFactoryCode.Text = grdFactory.Rows[e.RowIndex].Cells["FactoryCode"].Value.ToString();
            txtFactoryName.Text = grdFactory.Rows[e.RowIndex].Cells["FactoryName"].Value.ToString();
            btnDelete.Visible = true;
            btnUpdate.Visible = true;
            

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {

            //String querySearchCode = "SELECT COUNT(*) FROM factory_table WHERE factory_code=@txtFactoryCode";
            String querySearchName = "SELECT COUNT(*) FROM Factory WHERE FactoryName=@txtFactoryName";
            //SqlCommand commandCode = new SqlCommand(querySearchCode, con);
            //int countCode;
            int countName;
            //{
            //    commandCode.Parameters.AddWithValue("@txtFactoryCode", txtFactoryCode.Text);
            //    countCode = (int)commandCode.ExecuteScalar();

            //}
            SqlCommand commmandName = new SqlCommand(querySearchName, con);
            {
                commmandName.Parameters.AddWithValue("@txtFactoryName", txtFactoryName.Text);
                countName = (int)commmandName.ExecuteScalar();

            }
            //if (countCode > 0)
            //{
            //    MessageBox.Show("Factory Code already available", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);


            //}
            if (countName > 0)
            {
                MessageBox.Show("Factory Name already available", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (!string.IsNullOrWhiteSpace(txtFactoryName.Text) && countName == 0)
            {

                MessageBox.Show("Are you sure you want to update?");
                String queryUpdateCode = "UPDATE Factory SET FactoryName=@factoryName WHERE FactoryCode = @factoryCode";
                SqlCommand cmdUpdateCode = new SqlCommand(queryUpdateCode, con);
                cmdUpdateCode.Parameters.AddWithValue("@factoryCode", txtFactoryCode.Text);
                cmdUpdateCode.Parameters.AddWithValue("@factoryName", txtFactoryName.Text);
                cmdUpdateCode.ExecuteNonQuery();
                MessageBox.Show("Factory Updated Successfully");
                LoadData();
                txtFactoryCode.Clear();
                txtFactoryName.Clear();
            }
            else
            {
                MessageBox.Show("Please fill the both fields", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }

        }

        private void lblFactoryCode_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pctrRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }
    }


}
