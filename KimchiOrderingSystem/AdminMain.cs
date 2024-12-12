using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static KimchiOrderingSystem.Your_Orders;

namespace KimchiOrderingSystem
{
    public partial class AdminMain : Form
    {
        public AdminMain()
        {
            InitializeComponent();
            CustomizeUI();
            StartDateTimeUpdater();
        }

        private void AdminMain_Load(object sender, EventArgs e)
        {
            CustomizeUI();
            StartDateTimeUpdater();
        }

        private void CustomizeUI()
        {
            // Set header title
            lblTitle.Text = "Admin Dashboard";

            // Set footer text
            
            lblVersion.Text = "Version 1.0";
        }

        private void StartDateTimeUpdater()
        {
            Timer timer = new Timer
            {
                Interval = 1000 // Update every second
            };
            timer.Tick += (sender, e) =>
            {
                string currentDateTime = DateTime.Now.ToString("dddd, MMMM dd, yyyy hh:mm:ss tt");
               
                lblFooterDateTime.Text = currentDateTime;
            };
            timer.Start();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            AdminMain home = new AdminMain();
            home.Show();
            
        }

        private void btnUsers_Click(object sender, EventArgs e)
        {
            AdminUsers user = new AdminUsers();
            user.Show();
            this.Hide();
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            AdminProducts product = new AdminProducts();
            product.Show();
            this.Hide();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            AdminOrders order = new AdminOrders();
            order.Show();
            this.Hide();
        }

        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            InitialForm initialForm = new InitialForm();
            initialForm.Show();
            this.Close();
        }
    }
}
