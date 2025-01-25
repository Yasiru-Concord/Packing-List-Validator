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
    public partial class Frm_New_Factory : Form
    {
        SqlConnection con = new SqlConnection();
        public Frm_New_Factory()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;TrustServerCertificate=True;");
            con.Open();
            LoadData();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            try
            {
                String querySearchCode = "SELECT COUNT(*) FROM Factory WHERE FactoryCode=@txtFactoryCode";
                String querySearchName = "SELECT COUNT(*) FROM Factory WHERE FactoryName=@txtFactoryName";
                SqlCommand commandCode = new SqlCommand(querySearchCode, con);
                int countCode;
                int countName;
                {
                    commandCode.Parameters.AddWithValue("@txtFactoryCode", txtFactoryCode.Text);
                    countCode = (int)commandCode.ExecuteScalar();

                }
                SqlCommand commmandName = new SqlCommand(querySearchName, con);
                {
                    commmandName.Parameters.AddWithValue("@txtFactoryName", txtFactoryName.Text);
                    countName = (int)commmandName.ExecuteScalar();

                }
                if (countCode > 0)
                {
                    MessageBox.Show("Factory Code already available", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);


                }
                else if (countName > 0)
                {
                    MessageBox.Show("Factory Name already available", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (!string.IsNullOrWhiteSpace(txtFactoryCode.Text) && !string.IsNullOrWhiteSpace(txtFactoryName.Text))
                {

                    MessageBox.Show("Are you sure you want to Save?");
                    String queryCreate = "INSERT INTO Factory (FactoryCode,FactoryName) VALUES (@txtFactoryCode,@txtFactoryName)";
                    SqlCommand cmd = new SqlCommand(queryCreate, con);
                    {
                        cmd.Parameters.AddWithValue("@txtFactoryCode", txtFactoryCode.Text);
                        cmd.Parameters.AddWithValue("@txtFactoryName", txtFactoryName.Text);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Factory Create Successfully");
                        LoadData();
                        txtFactoryCode.Clear();
                        txtFactoryName.Clear();


                    }
                }
                else
                {
                    MessageBox.Show("Please fill the both fields", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", ex.Message);
            }
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

    
    }
}
