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
using System.Configuration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace KimchiOrderingSystem
{

    public partial class InitialForm : Form
    {
        public InitialForm()
        {
            InitializeComponent();
        }

        private void InitialForm_Load(object sender, EventArgs e)
        {

           

        }




        private void btnRegister_Click_1(object sender, EventArgs e)
        {
            Registration registrationForm = new Registration();
            registrationForm.Show();
            this.Hide();
        }

        private void btnAdminLogin_Click(object sender, EventArgs e)
        {
            AdminLogin adminLoginForm = new AdminLogin();
            adminLoginForm.Show();
            this.Hide();
        }

        private void btnUserLogin_Click(object sender, EventArgs e)
        {
            UserLogin userLoginForm = new UserLogin();
            userLoginForm.Show();
            this.Hide();
        }
        private int userId;
        private void btnGuestLogin_Click(object sender, EventArgs e)
        {
            string firstName = txtGuestFirstName.Text.Trim();
            string lastName = txtGuestLastName.Text.Trim();
            string email = txtGuestEmail.Text.Trim();
            string contactNumber = txtGuestContactNumber.Text.Trim();
            string username = null;
            

            // Validate inputs
            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(contactNumber))
            {
                MessageBox.Show("Please fill out all guest information fields before proceeding.");
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            UserDashboard guess = new UserDashboard(username, userId);
            guess.Show();
            this.Hide();

        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            
            Registration registrationForm = new Registration();
            registrationForm.Show();
            this.Hide();
            
        }
    }
}
