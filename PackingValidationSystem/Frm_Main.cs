using SmartPacker;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PackingValidationSystem
{
    public partial class Frm_Main : Form
    {
        string poNo;
        string pkListNo;
        public Frm_Main()
        {
            InitializeComponent();

            // Apply flat style to all buttons and remove border
            SetButtonStyle(button1);
            SetButtonStyle(button2);
            SetButtonStyle(button3);
            SetButtonStyle(button4);
            SetButtonStyle(button5);
            SetButtonStyle(btnScan);
        }

        // Helper method to apply the style to a button
        private void SetButtonStyle(Button button)
        {
            // Set the flat style and remove the border
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = button.BackColor;  // Hover effect
            button.FlatAppearance.MouseDownBackColor = button.BackColor; // Click effect

            // Set background color and text color
            button.BackColor = Color.Transparent;  // Or any color you want as the button's background
            button.ForeColor = Color.White;        // Set the text color to White

            // Set the font to Segoe UI Bold with size 10
            button.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NewCustomer f1 = new NewCustomer();
            f1.TopLevel = false;
            f1.FormBorderStyle = FormBorderStyle.None;
            f1.Dock = DockStyle.Fill;
            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(f1);
            f1.Show();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Frm_PO f1 = new Frm_PO(poNo);
            f1.TopLevel = false;
            f1.FormBorderStyle = FormBorderStyle.None;
            f1.Dock = DockStyle.Fill;
            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(f1);
            f1.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Frm_PackingList f1 = new Frm_PackingList();
            f1.TopLevel = false;
            f1.FormBorderStyle = FormBorderStyle.None;
            f1.Dock = DockStyle.Fill;
            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(f1);
            f1.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Frm_Factory f1 = new Frm_Factory();
            f1.TopLevel = false;
            f1.FormBorderStyle = FormBorderStyle.None;
            f1.Dock = DockStyle.Fill;
            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(f1);
            f1.Show();
        }
        private void button3_Click(object sender, EventArgs e)
        {

            Frm_Styles f1 = new Frm_Styles();
            f1.TopLevel = false;
            f1.FormBorderStyle = FormBorderStyle.None;
            f1.Dock = DockStyle.Fill;
            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(f1);
            f1.Show();
        }
       private void Frm_Main_Load(object sender, EventArgs e)
        {
            decimal pnlWidth = this.pnlContainer.Width;
            decimal pnlHeight = this.pnlContainer.Height;
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            Frm_ScanPkList f1 = new Frm_ScanPkList(pkListNo);
            f1.TopLevel = false;
            f1.FormBorderStyle = FormBorderStyle.None;
            f1.Dock = DockStyle.Fill;
            pnlContainer.Controls.Clear();
            pnlContainer.Controls.Add(f1);
            f1.Show();
        }
    }
}
