using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class Frm_PackingListDetails : Form
    {
        SqlConnection con = new SqlConnection();

        public Frm_PackingListDetails()
        {
            InitializeComponent();
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            con.Open();

            dataGridView1.Columns.Add("PONO", "PONO");

            // Add a "Delete" button column
            DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn
            {
                Name = "DeleteColumn",
                HeaderText = "Action",
                Text = "Delete",
                UseColumnTextForButtonValue = true // Display the text "Delete" on the button
            };

            dataGridView1.Columns.Add(deleteButtonColumn);

            LoadFactory();
            LoadCustomerNames();

            dataGridView1.CellContentClick += DataGridView1_CellContentClick;

            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);
            dataGridView1.Refresh();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the clicked cell is in the "Delete" button column
            if (dataGridView1.Columns[e.ColumnIndex].Name == "DeleteColumn" && e.RowIndex >= 0)
            {
                // Confirm the delete action with the user
                DialogResult result = MessageBox.Show("Are you sure you want to delete this row from the grid?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Remove the row from the DataGridView
                        dataGridView1.Rows.RemoveAt(e.RowIndex);

                        // Optionally, you can display a success message
                        MessageBox.Show("Row removed from the grid.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error while removing the row: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        // Load Factory Names into ComboBox
        private void LoadFactory()
        {
            try
            {
                string query = "SELECT FactoryCode,FactoryName FROM [Factory]";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbFactory.DataSource = dt;
                cmbFactory.DisplayMember = "FactoryName";
                cmbFactory.ValueMember = "FactoryCode";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Factory: " + ex.Message);
            }
        }

        


        // Load Customer Names into ComboBox
        private void LoadCustomerNames()
        {
            try
            {
                //string query = "SELECT cus_id, CusName FROM [Production].[dbo].[Customer]";
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
                // Corrected query to fetch the 'Season' from the 'CustomerSeasons' table based on cus_id
                string query = "SELECT Season FROM [dbo].[CustomerSeasons] WHERE cus_id = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", cusId);

                // Create a SqlDataAdapter to fill the DataTable with the result of the query
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Check if there are any rows returned
                if (dt.Rows.Count > 0)
                {
                    // Set the ComboBox data source to the Season column in the DataTable
                    cmbSeason.DataSource = dt;
                    cmbSeason.DisplayMember = "Season";  // Display the Season column in the ComboBox
                    cmbSeason.ValueMember = "Season";    // Set the value of each item to the Season column
                }
                else
                {
                    //MessageBox.Show("No seasons found for the selected customer.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void cmbCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null)
            {
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);

                // Load Seasons for the selected customer
                LoadSeasons(selectedCusId);

                // Load PONo for the selected customer
                LoadPOs(selectedCusId);
            }
        }

        // Load PONo based on the selected CustomerId
        private void LoadPOs(int customerId)
        {
            try
            {
                string query = "SELECT DISTINCT PONo FROM [PO] WHERE CustomerId = @CustomerId";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Clear the existing items in cmbPO
                cmbPO.Items.Clear();

                // Add PONo values to the ComboBox
                foreach (DataRow row in dt.Rows)
                {
                    cmbPO.Items.Add(row["PONo"].ToString());
                }

                // Optionally, you can set a default selection if needed
                if (cmbPO.Items.Count > 0)
                {
                    cmbPO.SelectedIndex = 0;  // Set the first item as selected
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PO numbers: " + ex.Message);
            }
        }
        // Load StyleId and StyleName based on the selected PONo
        private void LoadStyleDetails(string poNo)
        {
            try
            {
                string query = @"
            SELECT pd.StyleId, s.StyleName
            FROM PODetails pd
            INNER JOIN Style s ON pd.StyleId = s.StyleNo
            WHERE pd.PONumber = @PONo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // If there are results, populate the textboxes
                if (dt.Rows.Count > 0)
                {
                    // For the first result (assuming one StyleId per PO)
                    txtStyle.Text = dt.Rows[0]["StyleId"].ToString();
                    txtStyleName.Text = dt.Rows[0]["StyleName"].ToString();
                }
                else
                {
                    // If no styles found for the selected PONo, clear the textboxes
                    txtStyle.Clear();
                    txtStyleName.Clear();
                    MessageBox.Show("No style details found for the selected PO number.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading style details: " + ex.Message);
            }
        }

        // Load color details based on the selected PONo
        private void LoadColorDetails(string poNo)
        {
            try
            {
                string query = @"
            SELECT DISTINCT pd.ColourCode, pd.ColourName
            FROM PODetails pd
            INNER JOIN Style s ON pd.StyleId = s.StyleNo
            WHERE pd.PONumber = @PONo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Clear the existing items in ListBox1
                listBox1.Items.Clear();

                // If there are color details, populate the ListBox
                foreach (DataRow row in dt.Rows)
                {
                    string colour = $"{row["ColourCode"]} - {row["ColourName"]}";
                    listBox1.Items.Add(colour);
                }

                // Optionally, show a message if no colors are found
                if (listBox1.Items.Count == 0)
                {
                    MessageBox.Show("No color details found for the selected PO number.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading color details: " + ex.Message);
            }
        }

        private void cmbPO_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPO.SelectedItem != null)
            {
                string selectedPONo = cmbPO.SelectedItem.ToString(); // Get the selected PONo

                // Load the corresponding color details for the selected PONo
                LoadColorDetails(selectedPONo);

                // Optionally, you can also load other details like StyleId, StyleName
                LoadStyleDetails(selectedPONo);

                // Add the selected PONo to the DataGridView if it's not already present
                bool isAlreadyAdded = false;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["PONO"].Value != null && row.Cells["PONO"].Value.ToString() == selectedPONo)
                    {
                        isAlreadyAdded = true;
                        break;
                    }
                }
                if (!isAlreadyAdded)
                {
                    dataGridView1.Rows.Add(selectedPONo);
                }
                else
                {
                    MessageBox.Show("This PONO is already added to the list.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Gather data from the controls
                string pkListNo = txtPklist.Text.Trim();
                string factoryCode = cmbFactory.SelectedValue?.ToString();
                int customerId = Convert.ToInt32(cmbCustomer.SelectedValue);
                string season = cmbSeason.SelectedItem != null ? ((DataRowView)cmbSeason.SelectedItem)["Season"].ToString() : "";

                DateTime actualDeliveryDate = dateTimePicker1.Value;
                DateTime poDeliveryDate = dateTimePicker2.Value;

                // Validate inputs
                if (string.IsNullOrEmpty(pkListNo))
                {
                    MessageBox.Show("Packing List Number (PkListNo) is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(factoryCode))
                {
                    MessageBox.Show("Factory Code is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(season))
                {
                    MessageBox.Show("Season is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (customerId == 0)
                {
                    MessageBox.Show("Customer is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (actualDeliveryDate < DateTime.Now)
                {
                    MessageBox.Show("Actual Delivery Date cannot be in the past.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //if (poDeliveryDate > DateTime.Now)
                //{
                //    MessageBox.Show("PO Delivery Date cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}

                // Ask user for confirmation before saving
                DialogResult result = MessageBox.Show("Are you sure you want to save the packing list?",
                                                      "Confirm Save",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Loop through each row in DataGridView to insert PONo records
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["PONO"].Value != null)
                        {
                            string poNo = row.Cells["PONO"].Value.ToString();

                            // Construct the SQL Insert query for each PONo
                            string query = @"
                    INSERT INTO PackingList (PkListNo, FactoryCode, CustomerId, Season, ActualDeliveryDate, PODeliveryDate, PONo, Status)
                    VALUES (@PkListNo, @FactoryCode, @CustomerId, @Season, @ActualDeliveryDate, @PODeliveryDate, @PONo, @Status)";

                            // Create SQL Command with parameters
                            SqlCommand cmd = new SqlCommand(query, con);
                            cmd.Parameters.AddWithValue("@PkListNo", pkListNo);
                            cmd.Parameters.AddWithValue("@FactoryCode", factoryCode);
                            cmd.Parameters.AddWithValue("@CustomerId", customerId);
                            cmd.Parameters.AddWithValue("@Season", season); // Use the string value of the Season
                            cmd.Parameters.AddWithValue("@ActualDeliveryDate", actualDeliveryDate);
                            cmd.Parameters.AddWithValue("@PODeliveryDate", poDeliveryDate);
                            cmd.Parameters.AddWithValue("@PONo", poNo);
                            cmd.Parameters.AddWithValue("@Status", "Not Yet Started");

                            // Execute the query for each row
                            int resultDb = cmd.ExecuteNonQuery();

                            if (resultDb <= 0)
                            {
                                // If no rows were affected for this PO, show an error message and exit
                                MessageBox.Show($"Failed to save PONo: {poNo}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return; // Exit if there's an error saving any record
                            }
                        }
                    }

                    // If all rows are successfully inserted, show a success message
                    MessageBox.Show("Packing list saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Optionally, clear fields after saving
                    ClearFields();
                }
                else
                {
                    // Show a message if the user canceled the save operation
                    MessageBox.Show("Save operation canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Show an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void ClearFields()
        {
            // Clear all textboxes
            txtPklist.Clear();
            txtStyle.Clear();
            txtStyleName.Clear();

            // Reset combo boxes
            cmbFactory.SelectedIndex = -1;  // Or set to a default item if necessary
            cmbCustomer.SelectedIndex = -1;  // Or set to a default item if necessary
            cmbSeason.SelectedIndex = -1;  // Or set to a default item if necessary
            cmbPO.SelectedIndex = -1;  // Or set to a default item if necessary

            // Reset date pickers to the current date (or set to a default date)
            dateTimePicker1.Value = DateTime.Now;
            dateTimePicker2.Value = DateTime.Now;

            // Clear the ListBox
            listBox1.Items.Clear();

            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Ask for confirmation before clearing the fields
            DialogResult result = MessageBox.Show("Are you sure you want to cancel the data? Any unsaved data will be lost.",
                                                  "Confirm Cancel",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Clear the fields if the user confirms
                ClearFields();
            }
            else
            {
                // Optionally, show a message or do nothing if the user cancels
                MessageBox.Show("Operation canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
    }
}
