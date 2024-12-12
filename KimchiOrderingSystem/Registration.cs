using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KimchiOrderingSystem
{
    public partial class Registration : Form
    {
        public Registration()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            InitialForm start = new InitialForm();
            start.Show();
            this.Hide();
        }

        private void Registration_Load(object sender, EventArgs e)
        {

        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();
            string fullName = $"{txtFirstName.Text.Trim()} {txtLastName.Text.Trim()}";
            string email = txtEmail.Text.Trim();
            string contactNumber = txtContactNumber.Text.Trim();

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address.");
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Users (Username, Password, FullName, Email, Phone, IsAdmin) " +
                                   "VALUES (@Username, @Password, @FullName, @Email, @Phone, 0)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Phone", contactNumber);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Registration successful!");
                        this.Hide();
                        InitialForm initialForm = new InitialForm();
                        initialForm.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

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
    }
}
