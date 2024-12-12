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
    public partial class AdminUsers : Form
    {
        public AdminUsers()
        {
            InitializeComponent();
        }

        private void AdminUsers_Load(object sender, EventArgs e)
        {
            cmbUserRole.Items.Add("Admin");
            cmbUserRole.Items.Add("User");
            cmbSearchCategory.Items.AddRange(new string[] { "All", "Username", "Full Name", "Email", "Phone" });
            cmbSearchCategory.SelectedIndex = 0; // Default to "All"
            LoadUsers();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Load Admin Users
                    string queryAdmins = "SELECT Id, Username, FullName, Email, Phone FROM Users WHERE IsAdmin = 1";
                    SqlDataAdapter adapterAdmins = new SqlDataAdapter(queryAdmins, conn);
                    DataTable dtAdmins = new DataTable();
                    adapterAdmins.Fill(dtAdmins);
                    dgvAdmins.DataSource = dtAdmins;

                    // Load Normal Users
                    string queryNormalUsers = "SELECT Id, Username, FullName, Email, Phone FROM Users WHERE IsAdmin = 0";
                    SqlDataAdapter adapterNormalUsers = new SqlDataAdapter(queryNormalUsers, conn);
                    DataTable dtNormalUsers = new DataTable();
                    adapterNormalUsers.Fill(dtNormalUsers);
                    dgvNormalUsers.DataSource = dtNormalUsers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading users: {ex.Message}");
            }
        }
        private void btnHome_Click_1(object sender, EventArgs e)
        {
            AdminMain home = new AdminMain();
            home.Show();
            this.Hide();
        }

        private void btnUsers_Click_1(object sender, EventArgs e)
        {
            AdminUsers user = new AdminUsers();
            user.Show();
        }

        private void btnProducts_Click_1(object sender, EventArgs e)
        {
            AdminProducts product = new AdminProducts();
            product.Show();
            this.Hide();
        }

        private void btnOrders_Click_1(object sender, EventArgs e)
        {
            AdminOrders order = new AdminOrders();
            order.Show();
            this.Hide();
        }

        private void guna2GradientTileButton2_Click(object sender, EventArgs e)
        {
            InitialForm initialForm = new InitialForm();
            initialForm.Show();
            this.Close();
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                    string.IsNullOrWhiteSpace(txtPassword.Text) ||
                    string.IsNullOrWhiteSpace(txtFullName.Text) ||
                    string.IsNullOrWhiteSpace(txtEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtPhone.Text) ||
                    cmbUserRole.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                

                bool isAdmin = cmbUserRole.SelectedItem.ToString() == "Admin";

                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                INSERT INTO Users (Username, Password, FullName, Email, Phone, IsAdmin)
                VALUES (@Username, @Password, @FullName, @Email, @Phone, @IsAdmin)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                        cmd.Parameters.AddWithValue("@FullName", txtFullName.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@IsAdmin", isAdmin);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User added successfully!");
                LoadUsers(); // Refresh the DataGridViews
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding the user: {ex.Message}");
            }
        }
        private void DeleteUser(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to delete.");
                return;
            }

            int userId = Convert.ToInt32(dgv.SelectedRows[0].Cells["Id"].Value);

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Users WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User deleted successfully!");
                LoadUsers(); // Refresh the DataGridViews
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the user: {ex.Message}");
            }
        }
        private void btnDeleteAdminUser_Click(object sender, EventArgs e)
        {
            DeleteUser(dgvAdmins);
        }

        private void btnDeleteNormalUser_Click(object sender, EventArgs e)
        {
            DeleteUser(dgvNormalUsers);
        }
    }
}
