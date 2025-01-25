using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PackingValidationSystem
{
    public partial class Frm_PO : Form
    {
        SqlConnection con = new SqlConnection();
        private bool isErrorMessageShown = false;

        public Frm_PO(string poNo)
        {
            InitializeComponent();
            // New connection
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            con.Open();

            txtPO.Text = poNo;
            LoadFactoryNames();
            LoadCustomerNames();
            cmbCustomer.SelectedValue = -1;

            // Set default label to "Add PO" when opening the form
            label1.Text = "Add PO";  // Default behavior is Add PO

            btnSave.Visible = true;
            btnView.Visible = true;
            btnEdit.Visible = false;
            btnDelete.Visible = false;


            // Initialize DataGridView columns
            dataGridView1.Columns.Add("ColourCode", "Colour Code");
            dataGridView1.Columns.Add("ColourName", "Colour Name");
            dataGridView1.Columns.Add("Size", "Size");
            dataGridView1.Columns.Add("Quantity", "Quantity");
            dataGridView1.Columns.Add("Tolearnce", "Tolearnce");
            dataGridView1.Columns.Add("StyleId", "StyleId");
            dataGridView1.Columns.Add("EAN", "EAN");

            // Set font size, font family, and font style (bold) for DataGridView cells and headers
            Font gridFont = new Font("Segoe UI", 9, FontStyle.Bold);

            dataGridView1.CellClick += dataGridView1_CellClick;
            // Apply font to all cells
            dataGridView1.DefaultCellStyle.Font = gridFont;

            cmbSize.SelectedIndexChanged += cmbSize_SelectedIndexChanged;

            // Apply font to column headers
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = gridFont;

            dataGridView1.Columns["StyleId"].Visible = false;

            cmbStyle.SelectedValue = -1;


            // Load ShipMethod, OrderType, and DeliveryDate if PO number is pre-filled
            if (!string.IsNullOrEmpty(poNo))
            {
                LoadShipMethodByPONo(poNo);
                LoadOrderTypeByPONo(poNo);
                LoadDeliveryDateByPONo(poNo);
                LoadToleranceByPONo(poNo);
                LoadSeasonByPONo(poNo);
                LoadCustomerByPONo(poNo);
                LoadFactoryByPONo(poNo);
                LoadStylesByPONo(poNo);


                // Load the PODetails for the given PONo
                LoadPODetailsByPONo(poNo);

            }

            // If a PONo is passed (indicating we are editing), change the label to "Edit PO"
            if (poNo != null)
            {
                label1.Text = $"Edit PO - {poNo}";  // Show "Edit PO - [PONo]" when editing
                btnSave.Visible = false;
                btnView.Visible = false;
                btnEdit.Visible = true;
                btnDelete.Visible = true;


                // Create a new button column for deleting
                DataGridViewButtonColumn deleteButtonColumn = new DataGridViewButtonColumn();
                deleteButtonColumn.Name = "DeleteButton"; // Name of the column (optional)
                deleteButtonColumn.HeaderText = "Delete";  // Header text for the column
                deleteButtonColumn.Text = "Delete";        // Text displayed on the button
                deleteButtonColumn.UseColumnTextForButtonValue = true; // Display the text on the button

                // Add the delete button column to the DataGridView
                dataGridView1.Columns.Add(deleteButtonColumn);
            }

        }

        // Method to load ShipMethod based on PONo
        private void LoadShipMethodByPONo(string poNo)
        {
            try
            {
                // Query to fetch ShipMethod based on PONo
                string query = "SELECT ShipMethod FROM PO WHERE PONo = @PONo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Set the ShipMethod in ComboBox
                    cmbShpMethod.SelectedItem = result.ToString();
                }
                else
                {
                    MessageBox.Show("PO number not found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadStylesByPONo(string poNo)
        {
            try
            {
                // Query to fetch StyleId from PODetails based on PONo
                string queryStyleIds = "SELECT DISTINCT StyleId FROM PODetails WHERE PONumber = @PONo";
                SqlCommand cmdStyleIds = new SqlCommand(queryStyleIds, con);
                cmdStyleIds.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and retrieve StyleIds
                SqlDataAdapter daStyleIds = new SqlDataAdapter(cmdStyleIds);
                DataTable dtStyleIds = new DataTable();
                daStyleIds.Fill(dtStyleIds);

                // Check if there are results
                if (dtStyleIds.Rows.Count > 0)
                {
                    // Prepare a list to hold all the StyleIds
                    List<string> styleIds = new List<string>();

                    foreach (DataRow row in dtStyleIds.Rows)
                    {
                        styleIds.Add(row["StyleId"].ToString());
                    }

                    // Use the StyleIds to fetch the corresponding StyleNames
                    string queryStyleNames = @"
                SELECT DISTINCT StyleName 
                FROM Style 
                WHERE StyleNo IN (" + string.Join(",", styleIds.Select(id => $"'{id}'")) + ")";
                    SqlCommand cmdStyleNames = new SqlCommand(queryStyleNames, con);

                    // Execute the query and retrieve StyleNames
                    SqlDataAdapter daStyleNames = new SqlDataAdapter(cmdStyleNames);
                    DataTable dtStyleNames = new DataTable();
                    daStyleNames.Fill(dtStyleNames);

                    // Bind the StyleNames to the ComboBox
                    cmbStyle.DataSource = dtStyleNames;
                    cmbStyle.DisplayMember = "StyleName";
                    cmbStyle.ValueMember = "StyleName";
                }
                else
                {
                    MessageBox.Show("No styles found for the selected PO number.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        // Method to load OrderType based on PONo
        private void LoadOrderTypeByPONo(string poNo)
        {
            try
            {
                // Query to fetch OrderType based on PONo
                string query = "SELECT OrderType FROM PO WHERE PONo = @PONo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Set the OrderType in ComboBox (if needed)
                    cmbOrder.SelectedItem = result.ToString();
                }
                else
                {
                    MessageBox.Show("PO number not found for OrderType.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Method to load DeliveryDate based on PONo
        private void LoadDeliveryDateByPONo(string poNo)
        {
            try
            {
                // Query to fetch DeliveryDate based on PONo
                string query = "SELECT DeliveryDate FROM PO WHERE PONo = @PONo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Set the DeliveryDate in DateTimePicker
                    dateTimePicker1.Value = Convert.ToDateTime(result);
                }
                else
                {
                    MessageBox.Show("PO number not found for DeliveryDate.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Method to load Tolerance based on PONo
        private void LoadToleranceByPONo(string poNo)
        {
            try
            {
                // Query to fetch Tolerance based on PONo
                string query = "SELECT Tolerance FROM PO WHERE PONo = @PONo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Set the Tolerance value in the txtTolerance TextBox
                    txtTolerance.Text = result.ToString();
                }
                else
                {
                    // If the PO number is not found or Tolerance is not available
                    MessageBox.Show("PO number not found for Tolerance.");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Method to load Season based on PONo
        private void LoadSeasonByPONo(string poNo)
        {
            try
            {
                // Query to fetch Season based on PONo
                string query = "SELECT Season FROM PO WHERE PONo = @PONo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Set the Season value in cmbSeason ComboBox
                    cmbSeason.SelectedItem = result.ToString();
                }
                else
                {
                    // Handle if the PO number is not found or Season is not available
                    MessageBox.Show("PO number not found for Season.");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Method to load Customer based on PONo
        private void LoadCustomerByPONo(string poNo)
        {
            try
            {
                // Query to fetch CustomerName based on PONo
                string query = "SELECT c.CusName " +
                               "FROM PO p " +
                               "INNER JOIN Customer c ON p.CustomerId = c.cus_id " +
                               "WHERE p.PONo = @PONo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Set the Customer value in cmbCustomer ComboBox
                    cmbCustomer.Text = result.ToString();
                }
                else
                {
                    // Handle if PO number does not return a customer
                    MessageBox.Show("PO number not found for Customer.");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Method to load Factory based on PONo
        private void LoadFactoryByPONo(string poNo)
        {
            try
            {
                // Query to fetch FactoryName based on PONo
                string query = "SELECT f.FactoryName " +
                               "FROM PO p " +
                               "INNER JOIN Factory f ON p.AssignFacId = f.FactoryCode " +
                               "WHERE p.PONo = @PONo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    // Set the Factory value in cmbFactory ComboBox
                    cmbFactory.Text = result.ToString();
                }
                else
                {
                    // Handle if PO number does not return a factory
                    MessageBox.Show("PO number not found for Factory.");
                }
            }
            catch (Exception ex)
            {
                // Handle any errors
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        // Method to load PODetails based on PONo
        private void LoadPODetailsByPONo(string poNo)
        {
            try
            {
                // Query to fetch PODetails based on PONo
                string query = @"
            SELECT 
                PD.ColourCode,
                PD.ColourName,
                PD.Size,
                PD.Quantity,
                PD.Tolerance,
                PD.StyleId,
                PD.EAN
            FROM 
                PODetails PD
            INNER JOIN 
                PO P ON PD.PONumber = P.PONo
            WHERE 
                P.PONo = @PONo";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                // Execute the query and fill the DataGridView
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Clear existing rows in the DataGridView before adding new data
                dataGridView1.Rows.Clear();

                // Populate DataGridView with PODetails
                foreach (DataRow row in dt.Rows)
                {
                    dataGridView1.Rows.Add(
                        row["ColourCode"],
                        row["ColourName"],
                        row["Size"],
                        row["Quantity"],
                        row["Tolerance"],
                        row["StyleId"],
                        row["EAN"]
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        // Event handler for when the PO number (txtPO) changes or focus leaves
        private void txtPO_Leave(object sender, EventArgs e)
        {
            string poNo = txtPO.Text.Trim();

            // Ensure PO number is entered before attempting to load data
            if (!string.IsNullOrEmpty(poNo))
            {
                LoadShipMethodByPONo(poNo);
                LoadOrderTypeByPONo(poNo);
                LoadDeliveryDateByPONo(poNo);
                LoadToleranceByPONo(poNo);
                LoadSeasonByPONo(poNo);
                LoadCustomerByPONo(poNo);
                LoadFactoryByPONo(poNo);
                LoadStylesByPONo(poNo);


                // Load the PODetails for the given PONo
                LoadPODetailsByPONo(poNo);
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

        // Load Styles based on Customer Selection
        // Load Styles based on Customer Selection and Season
        private void LoadStyles(int cusId, string season)
        {
            try
            {
                // Modify the query to select StyleName where CusId and Season match
                string query = "SELECT DISTINCT StyleName FROM [Style] WHERE CusId = @CusId AND Season = @Season";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CusId", cusId);
                cmd.Parameters.AddWithValue("@Season", season);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                dt.Clear();
                cmbStyle.SelectedValue = "";

                da.Fill(dt);

                // Bind the data to the ComboBox
                cmbStyle.DataSource = dt;
                cmbStyle.DisplayMember = "StyleName";
                cmbStyle.ValueMember = "StyleName";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadFactoryNames()
        {
            try
            {
                // SQL query to select FactoryCode and FactoryName from Factory table
                string query = "SELECT FactoryCode, FactoryName FROM Factory";

                // Create a SqlDataAdapter to fetch data from the database
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Set the ComboBox properties to display FactoryName and use FactoryCode as the value
                cmbFactory.DisplayMember = "FactoryName";  // This is what will be shown in the ComboBox
                cmbFactory.ValueMember = "FactoryCode";   // This is the value that will be used (selected value)
                cmbFactory.DataSource = dt;  // Bind the data to the ComboBox
            }
            catch (Exception ex)
            {
                // Show error message if any exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        // Load Seasons based on Customer Selection
        //private void LoadSeasons(int cusId)
        //{
        //    try
        //    {
        //        //string query = "SELECT Season FROM [Customer] WHERE Id = @Id";
        //        string query = "SELECT Season FROM [Customer] WHERE cus_id = @Id";

        //        SqlCommand cmd = new SqlCommand(query, con);
        //        cmd.Parameters.AddWithValue("@Id", cusId);

        //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //        DataTable dt = new DataTable();
        //        da.Fill(dt);



        //        cmbSeason.DataSource = dt;
        //        cmbSeason.DisplayMember = "Season";
        //        cmbSeason.ValueMember = "Season";
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error: " + ex.Message);
        //    }
        //}

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

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }



        // Load Colour Codes based on StyleName and Customer
        private void LoadColourCodes(int cusId, string styleName)
        {
            try
            {
                string query = "SELECT DISTINCT ColourCode FROM [Style] WHERE CusId = @CusId AND StyleName = @StyleName";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CusId", cusId);
                cmd.Parameters.AddWithValue("@StyleName", styleName);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbColour.DataSource = dt;
                cmbColour.DisplayMember = "ColourCode";
                cmbColour.ValueMember = "ColourCode";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Load Colour Name based on selected ColourCode
        private void LoadColourName(int cusId, string styleName, string colourCode)
        {
            try
            {
                string query = "SELECT ColourName FROM [Style] WHERE CusId = @CusId AND StyleName = @StyleName AND ColourCode = @ColourCode";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CusId", cusId);
                cmd.Parameters.AddWithValue("@StyleName", styleName);
                cmd.Parameters.AddWithValue("@ColourCode", colourCode);

                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    txtColourName.Text = result.ToString();
                }
                else
                {
                    txtColourName.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Load Sizes based on StyleName and Customer
        private void LoadSizes(int cusId, string styleName)
        {
            try
            {
                string query = "SELECT DISTINCT Size FROM [Style] WHERE CusId = @CusId AND StyleName = @StyleName";

                SqlCommand cmd = new SqlCommand(query, con);
                //MessageBox.Show("selected customer:- " + cusId);
                cmd.Parameters.AddWithValue("@CusId", cusId);
                cmd.Parameters.AddWithValue("@StyleName", styleName);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbSize.DataSource = dt;
                cmbSize.DisplayMember = "Size";
                cmbSize.ValueMember = "Size";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // Load EAN based on selected StyleName, ColourCode, and Size
        private void LoadEAN(string styleName, string colourCode, string size)
        {
            try
            {
                // Query to fetch the EAN based on the selected parameters
                string query = "SELECT EAN FROM [dbo].[Style] WHERE StyleName = @StyleName AND ColourCode = @ColourCode AND Size = @Size";

                // Create SqlCommand object
                SqlCommand cmd = new SqlCommand(query, con);


                cmd.Parameters.AddWithValue("@StyleName", styleName);
                cmd.Parameters.AddWithValue("@ColourCode", colourCode);
                cmd.Parameters.AddWithValue("@Size", size);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                // If result is not null, display the EAN in the txtEAN text box
                if (result != null)
                {
                    txtEAN.Text = result.ToString();
                }
                else
                {
                    // If no result, clear the EAN text box
                    txtEAN.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                // Display any error that occurs
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void cmbSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbStyle.SelectedValue != null && cmbColour.SelectedValue != null && cmbSize.SelectedValue != null)
            {
                // Get the selected values
                string styleName = cmbStyle.SelectedValue.ToString();
                string colourCode = cmbColour.SelectedValue.ToString();
                string size = cmbSize.SelectedValue.ToString();

                // Call LoadEAN with the selected values
                LoadEAN(styleName, colourCode, size);
            }
        }

        // Event for Customer ComboBox selection change
        private void cmbCustomer_SelectedIndexChanged_1(object sender, EventArgs e)
        {

            if (cmbCustomer.SelectedValue != null)
            {
                //MessageBox.Show(cmbCustomer.SelectedValue.ToString());
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                ////MessageBox.Show(selectedCusId.ToString());
                //LoadStyles(selectedCusId);
                LoadSeasons(selectedCusId);
            }
        }

        // Event for Style ComboBox selection change
        private void cmbStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null && cmbStyle.SelectedValue != null)
            {
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                string selectedStyleName = cmbStyle.SelectedValue.ToString();
                LoadColourCodes(selectedCusId, selectedStyleName);
                LoadSizes(selectedCusId, selectedStyleName);
            }
        }

        // Event for Colour ComboBox selection change
        private void cmbColour_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue != null && cmbStyle.SelectedValue != null && cmbColour.SelectedValue != null)
            {
                int selectedCusId = Convert.ToInt32(cmbCustomer.SelectedValue);
                string selectedStyleName = cmbStyle.SelectedValue.ToString();
                string selectedColourCode = cmbColour.SelectedValue.ToString();
                LoadColourName(selectedCusId, selectedStyleName, selectedColourCode);
            }
        }

        private string GetStyleNoByStyleName(string styleName)
        {
            string styleNo = string.Empty;

            try
            {
                // Query to fetch StyleNo based on StyleName
                string query = "SELECT StyleNo FROM [Style] WHERE StyleName = @StyleName";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@StyleName", styleName);

                // Execute the query and get the result
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    styleNo = result.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return styleNo;
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Ensure that all fields are filled in before proceeding
            if (string.IsNullOrWhiteSpace(txtQty.Text))
            {
                MessageBox.Show("Please enter a quantity.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbStyle.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a style.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbColour.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a colour.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtColourName.Text))
            {
                MessageBox.Show("Please enter a colour name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cmbSize.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a size.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtValue.Text))
            {
                MessageBox.Show("Please enter a tolerance value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEAN.Text))
            {
                MessageBox.Show("Please enter an EAN value.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Retrieve the StyleName from cmbStyle
            string selectedStyleName = cmbStyle.SelectedValue?.ToString();

            // Retrieve StyleNo corresponding to StyleName from the database
            string selectedStyleNo = GetStyleNoByStyleName(selectedStyleName);

            if (!string.IsNullOrEmpty(selectedStyleNo))
            {
                // Find the red-colored row (the row that is highlighted with red background)
                DataGridViewRow redRow = null;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.DefaultCellStyle.BackColor == Color.FromArgb(154, 154, 154))
                    {
                        redRow = row;
                        break; // Exit loop once we find the red row
                    }
                }

                if (redRow != null)
                {
                    // Update the selected red row with the new values
                    redRow.Cells["ColourCode"].Value = cmbColour.SelectedValue.ToString(); // Colour Code
                    redRow.Cells["ColourName"].Value = txtColourName.Text;                    // Colour Name
                    redRow.Cells["Size"].Value = cmbSize.SelectedValue.ToString();           // Size
                    redRow.Cells["Quantity"].Value = txtQty.Text;                              // Quantity
                    redRow.Cells["Tolearnce"].Value = txtValue.Text;                          // Tolerance
                    redRow.Cells["StyleId"].Value = selectedStyleNo;
                    redRow.Cells["EAN"].Value = txtEAN.Text;  // StyleNo
                                                              // StyleName

                    // Reset the background color of the row to the default (white)
                    redRow.DefaultCellStyle.BackColor = Color.White;  // Change color to white
                    txtStyleId.Text = "";
                }
                else
                {
                    // If no red row is found, add a new row instead
                    dataGridView1.Rows.Add(
                        cmbColour.SelectedValue?.ToString(),  // Colour Code
                        txtColourName.Text,                   // Colour Name
                        cmbSize.SelectedValue?.ToString(),    // Size
                        txtQty.Text,
                        txtValue.Text,                        // Tolerance
                        selectedStyleNo,
                        txtEAN.Text // StyleNo
                    );
                }

                // Clear the form controls after adding/updating the row
                cmbColour.SelectedIndex = -1;
                txtColourName.Clear();
                cmbSize.SelectedIndex = -1;
                txtQty.Clear();
                txtValue.Clear();
                txtEAN.Clear();
            }
            else
            {
                MessageBox.Show("Style number could not be found.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string poNo = txtPO.Text.Trim();

                // Step 1: Validate mandatory fields for PO table
                if (string.IsNullOrEmpty(poNo))
                {
                    MessageBox.Show("PO Number cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (cmbCustomer.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a Customer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (cmbShpMethod.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a Shipping Method.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (cmbSeason.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a Season.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (cmbOrder.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select an Order Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (string.IsNullOrEmpty(txtTolerance.Text))
                {
                    MessageBox.Show("Tolerance cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (cmbFactory.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a Factory.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Step 2: Check if the PO number already exists
                if (DoesPONumberExist(poNo))
                {
                    MessageBox.Show("Error: PO number already exists. Data cannot be saved.", "Duplicate PO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // Exit the method without saving data
                }

                // Confirm save operation
                DialogResult result1 = MessageBox.Show("Are you sure you want to save this data?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result1 == DialogResult.Yes)
                {
                    // Step 3: Proceed with saving the data
                    int customerId = Convert.ToInt32(cmbCustomer.SelectedValue);
                    string shipMethod = cmbShpMethod.SelectedItem.ToString();
                    string season = cmbSeason.SelectedValue?.ToString();
                    string orderType = cmbOrder.SelectedItem.ToString();
                    string tolerance = txtTolerance.Text;

                    // Get the FactoryCode corresponding to the selected FactoryName
                    string assignFacId = cmbFactory.SelectedValue?.ToString(); // This will return the FactoryCode

                    // Get the DeliveryDate from the DateTimePicker control
                    DateTime deliveryDate = dateTimePicker1.Value;

                    // SQL query to insert data into PO table
                    string query = @"
INSERT INTO PO (PONo, CustomerId, ShipMethod, Season, OrderType, Tolerance, DeliveryDate, AssignFacId, Date)
VALUES (@PONo, @CustomerId, @ShipMethod, @Season, @OrderType, @Tolerance, @DeliveryDate, @AssignFacId, @Date);";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@PONo", poNo);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    cmd.Parameters.AddWithValue("@ShipMethod", shipMethod);
                    cmd.Parameters.AddWithValue("@Season", season ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrderType", orderType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tolerance", string.IsNullOrWhiteSpace(tolerance) ? (object)DBNull.Value : Convert.ToDecimal(tolerance));
                    cmd.Parameters.AddWithValue("@DeliveryDate", deliveryDate);
                    cmd.Parameters.AddWithValue("@AssignFacId", assignFacId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);

                    // Execute the query
                    cmd.ExecuteNonQuery(); // This inserts into the PO table

                    // Process each row in the DataGridView to save PODetails
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue; // Skip the new row

                        // Retrieve values from the DataGridView row
                        string styleId = row.Cells["StyleId"].Value?.ToString();
                        string colourCode = row.Cells["ColourCode"].Value?.ToString();
                        string colourName = row.Cells["ColourName"].Value?.ToString();
                        string size = row.Cells["Size"].Value?.ToString();
                        string EAN = row.Cells["EAN"].Value?.ToString();
                        int qty = 0;

                        // Parse the Tolerance field into decimal (or null if invalid)
                        decimal? rowTolerance = null;
                        if (row.Cells["Tolearnce"].Value != null && decimal.TryParse(row.Cells["Tolearnce"].Value.ToString(), out decimal parsedTolerance))
                        {
                            rowTolerance = parsedTolerance;
                        }

                        // Validate and parse the quantity
                        if (row.Cells["Quantity"].Value != null && int.TryParse(row.Cells["Quantity"].Value.ToString(), out qty))
                        {
                            // Ensure necessary fields are not null or empty
                            if (!string.IsNullOrWhiteSpace(colourCode) && !string.IsNullOrWhiteSpace(colourName) &&
                                !string.IsNullOrWhiteSpace(size) && qty > 0)
                            {
                                // Prepare the SQL query to insert data into PODetails, including the Tolerance
                                string insertPODetailsQuery = @"
INSERT INTO PODetails (PONumber, StyleId, ColourCode, ColourName, Size, Quantity, Tolerance, EAN)
VALUES (@PONumber, @StyleId, @ColourCode, @ColourName, @Size, @Quantity, @Tolerance, @EAN);";

                                // Create SQL command for PODetails
                                SqlCommand podCmd = new SqlCommand(insertPODetailsQuery, con);

                                // Add parameters with the proper values
                                podCmd.Parameters.AddWithValue("@PONumber", poNo);
                                podCmd.Parameters.AddWithValue("@StyleId", styleId); // Use styleId from the row
                                podCmd.Parameters.AddWithValue("@ColourCode", colourCode ?? (object)DBNull.Value);
                                podCmd.Parameters.AddWithValue("@ColourName", colourName ?? (object)DBNull.Value);
                                podCmd.Parameters.AddWithValue("@Size", size ?? (object)DBNull.Value);
                                podCmd.Parameters.AddWithValue("@Quantity", qty);
                                podCmd.Parameters.AddWithValue("@Tolerance", rowTolerance ?? (object)DBNull.Value); // Add Tolerance value here
                                podCmd.Parameters.AddWithValue("@EAN", EAN);
                                // Execute the command
                                podCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                MessageBox.Show("Please ensure that all fields (ColourCode, ColourName, Size, Quantity) are filled in correctly.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid quantity value in one of the rows.");
                        }
                    }

                    MessageBox.Show("PO Entered successfully.");

                    // Step 4: Clear all fields after successful save
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Save operation canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            txtPO.Clear();
            cmbCustomer.SelectedIndex = -1;
            cmbShpMethod.SelectedIndex = -1;
            cmbSeason.SelectedIndex = -1;
            cmbOrder.SelectedIndex = -1;
            txtTolerance.Clear();
            cmbFactory.SelectedIndex = -1;
            dateTimePicker1.Value = DateTime.Now; // Set to current date (or any default date)

            // Optionally, clear the DataGridView
            dataGridView1.Rows.Clear();
        }

        // Helper function to check if the PO number already exists
        private bool DoesPONumberExist(string poNo)
        {
            try
            {
                string query = "SELECT COUNT(1) FROM [PO] WHERE PONo = @PONo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNo);

                int count = (int)cmd.ExecuteScalar(); // Get the count of rows with the same PONo

                return count > 0; // If count > 0, the PO number exists
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking PO number: " + ex.Message);
                return false;
            }
        }


        // Helper function to get StyleId based on StyleName
        private int GetStyleId(string styleName)
        {
            try
            {
                string query = "SELECT StyleNo FROM [Style] WHERE StyleName = @StyleName";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@StyleName", styleName);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                else
                {
                    MessageBox.Show("Style not found.");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return 0;
            }
        }

        private void btnView_Click(object sender, EventArgs e)
        {
            Frm_PODetails frm = new Frm_PODetails();
            frm.ShowDialog();
        }

        private void CalculateValue()
        {
            // Check if both txtQty and txtTolerance are numeric values
            if (decimal.TryParse(txtQty.Text, out decimal qty) && decimal.TryParse(txtTolerance.Text, out decimal tolerance))
            {
                // Calculate value: (Qty * Tolerance / 100) + Qty
                decimal calculatedValue = (qty * tolerance / 100) + qty;

                // Set the result in the txtValue TextBox
                txtValue.Text = calculatedValue.ToString(); // Formatting to 2 decimal places
            }
            else
            {
                // If the input is invalid, clear the txtValue
                txtValue.Clear();
            }
        }

        private void txtQty_TextChanged(object sender, EventArgs e)
        {
            // Only show the error message if it's not already shown
            if (string.IsNullOrEmpty(txtTolerance.Text) && !isErrorMessageShown)
            {
                // Display an error message
                MessageBox.Show("Please add the Tolerance before adding the Quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isErrorMessageShown = true;

                txtTolerance.Focus();
                txtQty.Clear();
            }
            else
            {
                // Reset the flag once tolerance is filled
                isErrorMessageShown = false;
            }

            // Check if the text contains only numeric characters
            string inputText = txtQty.Text;

            // Remove non-numeric characters in real-time
            string numericText = new string(inputText.Where(char.IsDigit).ToArray());

            if (inputText != numericText)
            {
                // If the user entered non-numeric characters, update the text
                txtQty.Text = numericText;

                // Optionally, place the cursor at the end of the text
                txtQty.SelectionStart = txtQty.Text.Length;

                // Show the error message only once
                if (!isErrorMessageShown)
                {
                    MessageBox.Show("Please enter only numeric characters.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isErrorMessageShown = true;
                }
            }
            else
            {
                // Reset error message flag if the input is numeric
                isErrorMessageShown = false;
            }


            CalculateValue();
        }

        private void txtPO_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure the user clicked on a valid row (e.RowIndex >= 0 ensures it's a data row, not the header)
            if (e.RowIndex >= 0)
            {
                // If the clicked cell is from the delete button column
                if (dataGridView1.Columns[e.ColumnIndex].Name == "DeleteButton")
                {
                    // Confirm the deletion
                    DialogResult result = MessageBox.Show("Are you sure you want to delete this row?", "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    // If the user clicks Yes, delete the row
                    if (result == DialogResult.Yes)
                    {
                        // Remove the row from the DataGridView
                        dataGridView1.Rows.RemoveAt(e.RowIndex);
                    }
                }
                else
                {
                    // Auto-fill the form controls based on the clicked row values (same as before)
                    DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];

                    if (selectedRow.Cells["ColourCode"].Value != null)
                    {
                        cmbColour.SelectedValue = selectedRow.Cells["ColourCode"].Value.ToString(); // Set ColourCode in ComboBox
                    }

                    if (selectedRow.Cells["ColourName"].Value != null)
                    {
                        txtColourName.Text = selectedRow.Cells["ColourName"].Value.ToString(); // Set ColourName in TextBox
                    }

                    if (selectedRow.Cells["Size"].Value != null)
                    {
                        cmbSize.SelectedValue = selectedRow.Cells["Size"].Value.ToString(); // Set Size in ComboBox
                    }

                    if (selectedRow.Cells["Quantity"].Value != null)
                    {
                        txtQty.Text = selectedRow.Cells["Quantity"].Value.ToString(); // Set Quantity in TextBox
                    }

                    if (selectedRow.Cells["StyleId"].Value != null)
                    {
                        txtStyleId.Text = selectedRow.Cells["StyleId"].Value.ToString(); // Set StyleId in TextBox
                    }

                    if (selectedRow.Cells["EAN"].Value != null)
                    {
                        txtEAN.Text = selectedRow.Cells["EAN"].Value.ToString(); // Set StyleId in TextBox
                    }

                    // Change the background color of the selected row to red
                    selectedRow.DefaultCellStyle.BackColor = Color.FromArgb(
    154, 154, 154);

                    // Optionally reset the background color for other rows
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Index != e.RowIndex) // Skip the selected row
                        {
                            row.DefaultCellStyle.BackColor = Color.White; // Or any default color you prefer
                        }
                    }
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                string poNo = txtPO.Text.Trim();

                // Step 1: Check if the PO number exists
                if (!DoesPONumberExist(poNo))
                {
                    MessageBox.Show("Error: PO number does not exist. Cannot update.", "PO Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // Exit the method if PO does not exist
                }
                // Confirm save operation
                DialogResult result = MessageBox.Show("Are you sure you want to Edit this PO?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Step 2: Proceed with updating the PO data (update PO header)
                    int customerId = Convert.ToInt32(cmbCustomer.SelectedValue);
                    string shipMethod = cmbShpMethod.SelectedItem.ToString();
                    string season = cmbSeason.SelectedValue?.ToString();
                    string orderType = cmbOrder.SelectedItem.ToString();
                    string tolerance = txtTolerance.Text;

                    // Get the FactoryCode corresponding to the selected FactoryName
                    string assignFacId = cmbFactory.SelectedValue?.ToString(); // This will return the FactoryCode

                    // Get the DeliveryDate from the DateTimePicker control
                    DateTime deliveryDate = dateTimePicker1.Value;

                    // SQL query to update data in the PO table
                    string updatePOQuery = @"
UPDATE PO 
SET CustomerId = @CustomerId, ShipMethod = @ShipMethod, Season = @Season, OrderType = @OrderType, 
    Tolerance = @Tolerance, DeliveryDate = @DeliveryDate, AssignFacId = @AssignFacId
WHERE PONo = @PONo;";

                    SqlCommand cmd = new SqlCommand(updatePOQuery, con);
                    cmd.Parameters.AddWithValue("@PONo", poNo);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    cmd.Parameters.AddWithValue("@ShipMethod", shipMethod);
                    cmd.Parameters.AddWithValue("@Season", season ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrderType", orderType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tolerance", tolerance ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeliveryDate", deliveryDate);
                    cmd.Parameters.AddWithValue("@AssignFacId", assignFacId ?? (object)DBNull.Value);

                    // Execute the query to update the PO header
                    cmd.ExecuteNonQuery();

                    // Step 3: Delete existing PODetails records for the given PONo
                    string deletePODetailsQuery = "DELETE FROM PODetails WHERE PONumber = @PONumber;";
                    SqlCommand deleteCmd = new SqlCommand(deletePODetailsQuery, con);
                    deleteCmd.Parameters.AddWithValue("@PONumber", poNo);

                    // Execute the query to delete existing PODetails
                    deleteCmd.ExecuteNonQuery();

                    // Step 4: Insert new PODetails based on the DataGridView rows
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.IsNewRow) continue; // Skip the new row

                        // Retrieve values from the DataGridView row
                        string styleId = row.Cells["StyleId"].Value?.ToString();
                        string colourCode = row.Cells["ColourCode"].Value?.ToString();
                        string colourName = row.Cells["ColourName"].Value?.ToString();
                        string size = row.Cells["Size"].Value?.ToString();
                        int qty = 0;
                        string rowTolerance = row.Cells["Tolearnce"].Value?.ToString(); // Get Tolerance value from the DataGridView row
                        string EAN = row.Cells["EAN"].Value?.ToString();
                        // Validate and parse the quantity
                        if (row.Cells["Quantity"].Value != null && int.TryParse(row.Cells["Quantity"].Value.ToString(), out qty))
                        {
                            // Ensure necessary fields are not null or empty
                            if (!string.IsNullOrWhiteSpace(colourCode) && !string.IsNullOrWhiteSpace(colourName) &&
                                !string.IsNullOrWhiteSpace(size) && qty > 0)
                            {
                                // Prepare the SQL query to insert new PODetails
                                string insertPODetailsQuery = @"
INSERT INTO PODetails (PONumber, StyleId, ColourCode, ColourName, Size, Quantity, Tolerance,EAN)
VALUES (@PONumber, @StyleId, @ColourCode, @ColourName, @Size, @Quantity, @Tolerance,@EAN);";

                                // Create SQL command for PODetails
                                SqlCommand podCmd = new SqlCommand(insertPODetailsQuery, con);

                                // Add parameters with the proper values
                                podCmd.Parameters.AddWithValue("@PONumber", poNo);
                                podCmd.Parameters.AddWithValue("@StyleId", styleId); // Use styleId from the row
                                podCmd.Parameters.AddWithValue("@ColourCode", colourCode ?? (object)DBNull.Value);
                                podCmd.Parameters.AddWithValue("@ColourName", colourName ?? (object)DBNull.Value);
                                podCmd.Parameters.AddWithValue("@Size", size ?? (object)DBNull.Value);
                                podCmd.Parameters.AddWithValue("@Quantity", qty);
                                podCmd.Parameters.AddWithValue("@Tolerance", rowTolerance ?? (object)DBNull.Value); // Add Tolerance value here
                                podCmd.Parameters.AddWithValue("@EAN", EAN); // Add
                                // Execute the command to insert new PODetails
                                podCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                MessageBox.Show("Please ensure that all fields (ColourCode, ColourName, Size, Quantity) are filled in correctly.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid quantity value in one of the rows.");
                        }
                    }

                    MessageBox.Show("PO Updated Successfully.");
                }

                else
                {
                    MessageBox.Show("Save operation canceled.", "Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Prompt the user for confirmation before deleting
            DialogResult result = MessageBox.Show("Are you sure you want to delete this PO?",
                                                  "Delete Confirmation",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);

            // If the user clicked Yes, proceed with deletion
            if (result == DialogResult.Yes)
            {
                try
                {
                    string poNo = txtPO.Text.Trim();  // Assuming txtPO is the textbox with the PO Number to delete

                    // Ensure the PO number is not empty
                    if (string.IsNullOrWhiteSpace(poNo))
                    {
                        MessageBox.Show("PO Number cannot be empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Open the database connection if it isn't already open
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }

                    // Step 1: Delete related records in the PODetails table
                    string deletePODetailsQuery = "DELETE FROM PODetails WHERE PONumber = @PONumber;";
                    SqlCommand deletePODetailsCmd = new SqlCommand(deletePODetailsQuery, con);
                    deletePODetailsCmd.Parameters.AddWithValue("@PONumber", poNo);

                    // Execute the command to delete PODetails
                    deletePODetailsCmd.ExecuteNonQuery();

                    // Step 2: Delete the record from the PO table
                    string deletePOQuery = "DELETE FROM PO WHERE PONo = @PONo;";
                    SqlCommand deletePOCmd = new SqlCommand(deletePOQuery, con);
                    deletePOCmd.Parameters.AddWithValue("@PONo", poNo);

                    // Execute the command to delete the PO
                    deletePOCmd.ExecuteNonQuery();

                    MessageBox.Show("PO Deleted Successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtPO.Clear();
                }
                catch (Exception ex)
                {
                    // Handle any errors that may occur during deletion
                    MessageBox.Show("Error: " + ex.Message, "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Ensure the connection is closed after the operation
                    if (con.State == ConnectionState.Open)
                    {
                        con.Close();
                    }
                }
            }
            else
            {
                // If the user clicked No, do nothing
                return;
            }
        }

        private void cmbSeason_SelectedIndexChanged(object sender, EventArgs e)
        {
          
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void cmbSeason_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int cusId = Convert.ToInt32(cmbCustomer.SelectedValue); // Assuming cusId is the selected value
            string season = cmbSeason.SelectedValue.ToString();    // Assuming season is selected in cmbSeason

            // Call LoadStyles with both cusId and season
            LoadStyles(cusId, season);
        }

        private void txtTolerance_TextChanged(object sender, EventArgs e)
        {
            // Check if the text contains only numeric characters
            string inputText = txtTolerance.Text;

            // Remove non-numeric characters in real-time
            string numericText = new string(inputText.Where(char.IsDigit).ToArray());

            if (inputText != numericText)
            {
                // If the user entered non-numeric characters, update the text
                txtTolerance.Text = numericText;

                // Optionally, place the cursor at the end of the text
                txtTolerance.SelectionStart = txtTolerance.Text.Length;

                // Show the error message only once
                if (!isErrorMessageShown)
                {
                    MessageBox.Show("Please enter only numeric characters.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isErrorMessageShown = true;
                }
            }
            else
            {
                // Reset error message flag if the input is numeric
                isErrorMessageShown = false;
            }
        }
    }
}
