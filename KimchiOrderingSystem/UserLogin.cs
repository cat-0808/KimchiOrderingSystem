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
    public partial class UserLogin : Form
    {
        public UserLogin()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            InitialForm start = new InitialForm();
            start.Show();
            this.Hide();
        }
        
        private void btnAdminLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if the user exists and retrieve their ID
                    string query = "SELECT Id FROM Users WHERE Username = @Username AND Password = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            int userId = Convert.ToInt32(result);

                            
                            UserDashboard dashboard = new UserDashboard(username,userId);
                            dashboard.SetUserId(userId); 
                            dashboard.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }
        private int userId;
        public void SetUserId(int loggedInUserId)
        {
            userId = loggedInUserId;
        }
        private int GetUserId(string username, string password)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id FROM Users WHERE Username = @Username AND Password = @Password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private void UserLogin_Load(object sender, EventArgs e)
        {

        }
    }
}
