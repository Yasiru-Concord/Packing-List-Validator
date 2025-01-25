using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2013.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;



namespace PackingValidationSystem
{
    public partial class Frm_ScanPkList : Form
    {
        private SqlConnection con;
        private bool isErrorMessageShown = false;
        private Timer barcodeTimer;
        private const int BarcodeDelay = 500; // 500 milliseconds delay (adjust as needed)

        // Dictionary to track sizes and quantities per carton
        private Dictionary<int, Dictionary<string, int>> cartonDetails = new Dictionary<int, Dictionary<string, int>>();

        public Frm_ScanPkList(string pkListNo)
        {
            InitializeComponent();

            // Initialize the database connection
            con = new SqlConnection(@"Data Source=124.43.16.11,1444;Initial Catalog=db_packing_list;User ID=sa;Password=_scon12;");
            try
            {
                con.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to the database: " + ex.Message);
                return;
            }

            // Set the TextBox with the PkListNo value
            txtPKListNo.Text = pkListNo;

            // Set default value of txtNo to 1 when the form is loaded
            txtNo.Text = "1";

            // Initialize DataGridView columns
            InitializeDataGridView();

            // Load ComboBox data
            LoadPONoComboBox(pkListNo);

            // Initialize and configure the barcode timer
            barcodeTimer = new Timer();
            barcodeTimer.Interval = BarcodeDelay; // Set to 500 milliseconds (adjust as needed)
            barcodeTimer.Tick += BarcodeTimer_Tick;

            // Bind the TextChanged event to the handler
            txtBarcode.TextChanged += txtBarcode_TextChanged;



            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);

            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);

            dataGridView1.Refresh();

            dataGridView2.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dataGridView2.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dataGridView2.DefaultCellStyle.ForeColor = Color.FromArgb(44, 95, 110);

            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);

            dataGridView2.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(44, 95, 110);

            dataGridView2.Refresh();


            dataGridView3.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView3.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridView3.DefaultCellStyle.ForeColor = Color.FromArgb(69, 14, 123);
            dataGridView3.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            dataGridView3.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(69, 14, 123);
            dataGridView3.Refresh();

            lblStatus.Visible = false;

            this.WindowState = FormWindowState.Maximized;

         
        }

        private void InitializeDataGridView()
        {
            // Clear existing columns (if any)
            dataGridView1.Columns.Clear();
            dataGridView2.Columns.Clear();  

            // Add necessary columns for DataGridView 1 (Main Grid)
            dataGridView1.Columns.Add("StyleId", "Style ID");
            dataGridView1.Columns.Add("ColourCode", "Colour Code");
            dataGridView1.Columns.Add("Size", "Size");
            dataGridView1.Columns.Add("EAN", "EAN");
            dataGridView1.Columns.Add("Quantity", "Quantity");
            dataGridView1.Columns.Add("Tolerance", "Qty.Tolerance");
            dataGridView1.Columns.Add("RBalance", "RBalance");
            dataGridView1.Columns.Add("Status", "Status");

            // Make StyleId and ColourCode columns invisible
            dataGridView1.Columns["StyleId"].Visible = false;
            dataGridView1.Columns["ColourCode"].Visible = false;
            dataGridView1.Columns["Quantity"].Visible = false;
            dataGridView1.Columns["RBalance"].Visible = false;

            // Bind the CellFormatting event
            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

            // Add necessary columns for DataGridView 2 (Carton Details)
            dataGridView2.Columns.Add("CartonNo", "Carton No");
            dataGridView2.Columns.Add("Size", "Size");
            dataGridView2.Columns.Add("Quantity", "Quantity");

            dataGridView3.Visible = false;
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Check if the current column is the Quantity column
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Quantity")
            {
                // Set the background color of the Quantity column to yellow
                e.CellStyle.BackColor = Color.LightGray;
            }
        }

        private void LoadPONoComboBox(string pkListNo)
        {
            try
            {
                string query = @"SELECT PONo FROM PackingList WHERE PkListNo = @PkListNo";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PkListNo", pkListNo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Bind the DataTable to the ComboBox
                cmbPONo.DataSource = dt;
                cmbPONo.DisplayMember = "PONo";
                cmbPONo.ValueMember = "PONo";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PONo values: " + ex.Message);
            }
        }

        private void cmbPONo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPONo.SelectedValue != null)
            {
                string selectedPONo = cmbPONo.SelectedValue.ToString();
                LoadStyleDetails(selectedPONo);
                LoadPODetails(selectedPONo);
                // Load the sum of Tolerance for the selected PONumber
                LoadToleranceSum(selectedPONo);

                txtBarcode.Focus();

            }
        }

        private void LoadToleranceSum(string poNo)
        {
            try
            {
                // Query to get the sum of the Tolerance column
                string query = @"
            SELECT SUM(POD.Tolerance) AS ToleranceSum
            FROM PODetails AS POD
            WHERE POD.PONumber = @PONumber";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONumber", poNo);

                // Execute the query and retrieve the sum of Tolerance
                object result = cmd.ExecuteScalar();

                // If the result is not null, round it and update the txtSum TextBox
                if (result != DBNull.Value)
                {
                    // Round the result to the nearest integer
                    int roundedValue = Convert.ToInt32(Math.Round(Convert.ToDecimal(result)));
                    txtSum.Text = roundedValue.ToString();
                }
                else
                {
                    txtSum.Clear(); // Clear txtSum if there is no result
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Tolerance sum: " + ex.Message);
            }
        }


        private void LoadPODetails(string poNo)
        {
            try
            {
                string query = @"
        SELECT 
            POD.StyleId,
            POD.ColourCode,
            POD.Size,
            POD.EAN,
            POD.Quantity,
            POD.Tolerance
        FROM PODetails AS POD
        WHERE POD.PONumber = @PONumber";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONumber", poNo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Clear existing rows in DataGridView
                dataGridView1.Rows.Clear();

                // Add fetched rows to DataGridView
                foreach (DataRow row in dt.Rows)
                {
                    // Round the Tolerance value to the nearest integer
                    int roundedTolerance = 0;
                    if (row["Tolerance"] != DBNull.Value)
                    {
                        roundedTolerance = Convert.ToInt32(Math.Round(Convert.ToDecimal(row["Tolerance"])));
                    }

                    dataGridView1.Rows.Add(
                        row["StyleId"].ToString(),
                        row["ColourCode"].ToString(),
                        row["Size"].ToString(),
                        row["EAN"].ToString(),
                        row["Quantity"].ToString(),
                        roundedTolerance.ToString() // Use the rounded value for Tolerance
                    );
                }

                // Set the default value for the Status column as "Not Started Scanning"
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    row.Cells["Status"].Value = "Not Started";
                    //row.DefaultCellStyle.BackColor = Color.MediumPurple;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading PO details: " + ex.Message);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Increment txtNo value (CartonNo)
            if (int.TryParse(txtNo.Text, out int currentValue))
            {
                currentValue++;
                txtNo.Text = currentValue.ToString();
            }
            else
            {
                txtNo.Text = "1";
            }
        }

        private void txtPKListNo_TextChanged(object sender, EventArgs e)
        {
            // Reload ComboBox if PkListNo is updated
            if (!string.IsNullOrWhiteSpace(txtPKListNo.Text))
            {
                LoadPONoComboBox(txtPKListNo.Text);
            }
        }

        private void Frm_ScanPkList_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Ensure the database connection is closed
            if (con != null && con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }
        private void LoadStyleDetails(string poNo)
        {
            try
            {
                // Query to fetch StyleName and ColourCode
                string query = @"
                    SELECT s.StyleName, pd.ColourCode
                    FROM PODetails pd
                    JOIN Style s ON pd.StyleId = s.StyleNo
                    WHERE pd.PONumber = @PONumber";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONumber", poNo);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Populate Style and ColourCode fields
                if (dt.Rows.Count > 0)
                {
                    txtStyle.Text = dt.Rows[0]["StyleName"].ToString();
                    txtColourCode.Text = dt.Rows[0]["ColourCode"].ToString();
                }
                else
                {
                    txtStyle.Clear();
                    txtColourCode.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Style details: " + ex.Message);
            }
        }
        // TextChanged event handler for barcode scanning
        private void txtBarcode_TextChanged(object sender, EventArgs e)
        {
            // Check if the text contains only numeric characters
            string inputText = txtBarcode.Text;

            // Remove non-numeric characters in real-time
            string numericText = new string(inputText.Where(char.IsDigit).ToArray());

            if (inputText != numericText)
            {
                // If the user entered non-numeric characters, update the text
                txtBarcode.Text = numericText;

                // Optionally, place the cursor at the end of the text
                txtBarcode.SelectionStart = txtBarcode.Text.Length;

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

            // Restart the timer each time the text changes
            barcodeTimer.Stop();
            barcodeTimer.Start();
        }


        private void BarcodeTimer_Tick(object sender, EventArgs e)
        {
            barcodeTimer.Stop();

            // Get the scanned barcode (EAN)
            string scannedEAN = txtBarcode.Text.Trim();

            if (string.IsNullOrWhiteSpace(scannedEAN))
            {
                return;
            }

            bool barcodeFound = false;

            // Loop through the rows in DataGridView to find the matching EAN
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                string ean = row.Cells["EAN"].Value.ToString();

                // Check if the scanned EAN matches the row's EAN
                if (ean == scannedEAN)
                {
                    barcodeFound = true;

                    string size = row.Cells["Size"].Value.ToString(); // Get the size from the row

                    // Check if this carton exists in the dictionary, and create if it doesn't
                    int cartonNo = int.Parse(txtNo.Text);
                    if (!cartonDetails.ContainsKey(cartonNo))
                    {
                        cartonDetails[cartonNo] = new Dictionary<string, int>();
                    }

                    // Increment the quantity for this size in the current carton
                    if (!cartonDetails[cartonNo].ContainsKey(size))
                    {
                        cartonDetails[cartonNo][size] = 0;
                    }

                    cartonDetails[cartonNo][size]++;

                    // Decrement the quantity in the DataGridView (main table)
                    int currentQuantity = Convert.ToInt32(row.Cells["Tolerance"].Value);
                    if (currentQuantity > 0)
                    {
                        row.Cells["Tolerance"].Value = currentQuantity - 1;

                        // Update the RBalance column to reflect the remaining quantity
                        int remainingBalance = Convert.ToInt32(row.Cells["RBalance"].Value);
                        row.Cells["RBalance"].Value = remainingBalance + 1;

                        lblStatus.Visible = true;
                        // Display success message in the status label
                        lblStatus.Text = $"Barcode for EAN {scannedEAN} scanned successfully.";

                        // Update Status column to "Pending"
                        row.Cells["Status"].Value = "Pending ⏳";

                        // Change row color to light yellow for pending scanning
                        row.DefaultCellStyle.BackColor = Color.Yellow;

                        // If tolerance reaches 0, mark it as completed
                        if (currentQuantity - 1 == 0)
                        {
                            // Change row color to green (Completed)
                            row.DefaultCellStyle.BackColor = Color.LightGreen;

                            // Update Status column to "Completed"
                            row.Cells["Status"].Value = "Completed ✅";

                        }

                        // Update DataGridView2 to show carton size-wise breakdown
                        dataGridView2.Rows.Clear();
                        foreach (var carton in cartonDetails)
                        {
                            foreach (var sizeEntry in carton.Value)
                            {
                                dataGridView2.Rows.Add(carton.Key, sizeEntry.Key, sizeEntry.Value);
                            }
                        }

                        // Clear the barcode input field for the next scan
                        txtBarcode.Clear();
                    }
                    else
                    {
                        // Play error sound using SoundPlayer
                        SoundPlayer player = new SoundPlayer(@"D:\New folder\sound.wav"); // Path to your .wav file
                        player.Play();

                        // If quantity is already 0 or less, show an error message
                        MessageBox.Show($"No remaining quantity for EAN {scannedEAN}. The size has been fully scanned.",
                                         "Error",
                                         MessageBoxButtons.OK,
                                         MessageBoxIcon.Error);

                        // Clear the barcode input field after error message
                        txtBarcode.Clear();
                    }

                    break; // Stop searching once the EAN is found and updated
                }
            }

            // If barcode is not found, show an error message
            if (!barcodeFound)
            {
                MessageBox.Show($"Barcode with EAN {scannedEAN} not found in the list.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtBarcode.Clear();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            // Initialize a new DataTable for the result
            DataTable dt13 = new DataTable();
            dt13.Columns.Add("crt from");
            dt13.Columns.Add("crt to");

            // List to store the grouped results
            List<(int startCarton, int endCarton, string size, int quantity)> groupedData = new List<(int, int, string, int)>();

            // List to keep track of rows in dataGridView2
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            // Collect rows from dataGridView2
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["CartonNo"].Value != null && row.Cells["Size"].Value != null && row.Cells["Quantity"].Value != null)
                {
                    rows.Add(row);
                }
            }

            // Sort the rows by CartonNo and then by Size to handle consecutive carton numbers correctly
            rows.Sort((r1, r2) =>
            {
                int cartonCompare = Convert.ToInt32(r1.Cells["CartonNo"].Value).CompareTo(Convert.ToInt32(r2.Cells["CartonNo"].Value));
                if (cartonCompare == 0)
                {
                    return r1.Cells["Size"].Value.ToString().CompareTo(r2.Cells["Size"].Value.ToString());
                }
                return cartonCompare;
            });

            // Variables to track the grouping
            int startCarton = -1;
            int endCarton = -1;
            string currentSize = string.Empty;
            int currentQuantity = -1;

            // Process the rows to group them by size, quantity, and consecutive carton numbers
            for (int i = 0; i < rows.Count; i++)
            {
                int cartonNo = Convert.ToInt32(rows[i].Cells["CartonNo"].Value);
                string size = rows[i].Cells["Size"].Value.ToString();
                int quantity = Convert.ToInt32(rows[i].Cells["Quantity"].Value);

                // If it's the first row or the same size, quantity, and consecutive carton number, group them
                if (startCarton == -1 || (size == currentSize && quantity == currentQuantity && cartonNo == endCarton + 1))
                {
                    // Start a new group or continue the current group
                    if (startCarton == -1)
                    {
                        startCarton = cartonNo; // Initialize the start of the group
                    }

                    endCarton = cartonNo;  // Update the end of the group
                    currentSize = size;    // Keep track of the size
                    currentQuantity = quantity; // Keep track of the quantity
                }
                else
                {
                    // When the sequence breaks, add the current group to the list
                    groupedData.Add((startCarton, endCarton, currentSize, currentQuantity));

                    // Start a new group
                    startCarton = cartonNo;
                    endCarton = cartonNo;
                    currentSize = size;
                    currentQuantity = quantity;
                }
            }

            // Add the last group after the loop
            if (startCarton != -1)
            {
                groupedData.Add((startCarton, endCarton, currentSize, currentQuantity));
            }

            // Create columns dynamically for each size (32, 34, etc.)
            HashSet<string> uniqueSizes = new HashSet<string>();
            foreach (var group in groupedData)
            {
                uniqueSizes.Add(group.size);
            }

            // Add the size columns
            foreach (var size in uniqueSizes)
            {
                dt13.Columns.Add(size);
            }

            //// Add the new columns for Totals
            //dt13.Columns.Add("Tot no crt");      // Total number of cartons in the group
            //dt13.Columns.Add("Tot/cart"); // The quantity for each size
            //dt13.Columns.Add("Total");            // Total quantity for the group

            // Add the grouped data to the DataTable
            foreach (var group in groupedData)
            {
                DataRow newRow = dt13.NewRow();
                newRow["crt from"] = group.startCarton;
                newRow["crt to"] = group.endCarton;

                // Fill the quantities for each size in the corresponding column
                foreach (var size in uniqueSizes)
                {
                    if (size == group.size)
                    {
                        newRow[size] = group.quantity;
                    }
                    else
                    {
                        newRow[size] = 0; // No quantity for this size in the current group
                    }
                }

                //// Calculate "Tot no crt" (number of cartons in the group)
                //int totNoCrt = group.endCarton - group.startCarton + 1;
                //newRow["Tot no crt"] = totNoCrt;

                //// Calculate "Tot/cart" (quantity for each size)
                //newRow["Tot/cart"] = group.quantity;

                //// Calculate "Total" (Tot/cart * Tot no crt)
                //int total = Convert.ToInt32(newRow["Tot/cart"]) * Convert.ToInt32(newRow["Tot no crt"]);
                //newRow["Total"] = total;

                // Add the row to the DataTable
                dt13.Rows.Add(newRow);
            }

            // Bind the DataTable to dataGridView3
            dataGridView3.DataSource = dt13;
        }


private void btnTest_Click(object sender, EventArgs e)
    {
        try
        {
            // Step 1: Sum the quantities in dataGridView2
            int totalQuantity = 0;
            foreach (DataGridViewRow row in dataGridView2.Rows)
            {
                if (row.Cells["Quantity"].Value != null)
                {
                    totalQuantity += Convert.ToInt32(row.Cells["Quantity"].Value);
                }
            }

            // Step 2: Compare the sum with the value in txtSum
            int txtSumValue;
            if (int.TryParse(txtSum.Text, out txtSumValue))
            {
                // Step 3: Check if the total quantity exceeds txtSum value
                if (totalQuantity > txtSumValue)
                {
                    // Play error sound using SoundPlayer
                    SoundPlayer player = new SoundPlayer(@"D:\New folder\sound.wav"); // Path to your .wav file
                    player.Play();

                    // Show error message with details
                    MessageBox.Show($"Error: The packing quantity of {totalQuantity} exceeds the ship quantity of {txtSumValue}. Please check your quantities.",
                                     "Quantity Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // Play success sound (optional)
                    System.Media.SystemSounds.Asterisk.Play();

                    // Show success message
                    MessageBox.Show($"Success: The packing quantity of {totalQuantity} is within the allowed limit of {txtSumValue}.",
                                     "Quantity Check", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Call button1_Click logic if it's not already part of another method
                        button1_Click(sender, e);


                        dataGridView1.Visible = false;
                        dataGridView3.Visible = false;
                        dataGridView2.Visible = false;
                        lblStatus.Visible = false;

                        cmbPONo.SelectedValue = -1;

                        // Call btnExport_Click_1 logic if necessary
                        btnExport_Click_1(sender, e);

                        UpdateStatusToCompleted();

                    }
                }
            else
            {

                // Show error message for invalid input
                MessageBox.Show("Error: Please enter a valid number in the 'Ship Quantity' field (txtSum).",
                                 "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {

            // Show unexpected error message
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

        // Method to update the status to "Completed"
        private void UpdateStatusToCompleted()
        {
            try
            {
                // Get the PO number from ComboBox
                string poNumber = cmbPONo.SelectedValue?.ToString();
                if (string.IsNullOrEmpty(poNumber))
                {
                    MessageBox.Show("Error: PO Number is not selected.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // SQL Query to update the status
                string query = @"UPDATE PackingList SET Status = 'Completed' WHERE PONo = @PONo";

                // Create the SqlCommand with the query and connection
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PONo", poNumber);

                // Execute the command
                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show($"Status for PO {poNumber} has been updated to 'Completed'.", "Status Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"No records found for PO {poNumber}.", "Status Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                // Show error message if the update fails
                MessageBox.Show($"An error occurred while updating the status: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void btnExport_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Check if DataGridView3 has rows
                if (dataGridView3.Rows.Count > 0)
                {
                    // Create a DataTable to hold the data
                    DataTable dt = new DataTable();

                    // Add columns to DataTable
                    foreach (DataGridViewColumn column in dataGridView3.Columns)
                    {
                        dt.Columns.Add(column.HeaderText);
                    }

                    // Add rows to DataTable
                    foreach (DataGridViewRow row in dataGridView3.Rows)
                    {
                        DataRow dataRow = dt.NewRow();
                        for (int i = 0; i < dataGridView3.Columns.Count; i++)
                        {
                            dataRow[i] = row.Cells[i].Value?.ToString() ?? string.Empty;
                        }
                        dt.Rows.Add(dataRow);
                    }

                    // Create an Excel workbook
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        // Add DataTable as a worksheet
                        var worksheet = workbook.Worksheets.Add(dt, "DataGridView3");

                        // Apply formatting
                        // Set font to Segoe UI, bold, size 14
                        worksheet.Style.Font.FontName = "Segoe UI";
                        worksheet.Style.Font.FontSize = 14;
                        worksheet.Style.Font.Bold = true;

                        // Add thin borders to all cells
                        worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // Adjust column widths to fit content
                        worksheet.Columns().AdjustToContents();

                        // Define the output file path
                        string folderPath = @"D:\New folder";
                        string pkListNo = txtPKListNo.Text.Trim();
                        string poNumber = cmbPONo.SelectedValue?.ToString() ?? "Unknown";

                        // Generate the file name dynamically based on the PKListNo and PONumber
                        string fileName = $"PKList_{pkListNo}_{poNumber}.xlsx";
                        string fullPath = Path.Combine(folderPath, fileName);

                        // Ensure the folder exists
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Save the Excel file
                        workbook.SaveAs(fullPath);

                        // Notify the user
                        MessageBox.Show($"Export successful! File saved at: {fullPath}",
                                        "Export to Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("No data available to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void txtNo_TextChanged(object sender, EventArgs e)
        {
            txtBarcode.Focus();
        }
    }
}



