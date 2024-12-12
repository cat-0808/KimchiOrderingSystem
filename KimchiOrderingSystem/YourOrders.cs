using Guna.UI2.WinForms;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace KimchiOrderingSystem
{

    public partial class Your_Orders : Form
    {
        private int loggedInUserId;
        private string loggedInUsername;
        private string username;
        private int userId;
        public Your_Orders(string username, int userId)
        {
            InitializeComponent();
            loggedInUsername = username;
            loggedInUserId = userId;
            this.username = username;
            this.userId = userId;
            UserLogin(loggedInUserId);
        }
        
        private void Your_Orders_Load(object sender, EventArgs e)
        {
            complaint.Items.Add("Product Not Received");
            complaint.Items.Add("Product Arrived Late");
            complaint.Items.Add("Interface Problem");
            complaint.Items.Add("Relationship Problem");
            complaint.Items.Add("Money Problem");
            complaint.Items.Add("Grade Problem");
            flowLayoutPanelOrders.Controls.Clear();
            UserLogin(loggedInUserId);
            lblUsername.Text = $"Welcome {loggedInUsername}!";

        }
        private void LoadOrders(int userId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to get orders for the logged-in user
                    string queryOrders = @"
        SELECT O.OrderId, O.TotalAmount, O.Status, O.OrderDate, 
               OD.ProductId, P.Name AS ProductName, OD.Quantity, OD.Price
        FROM Orders O
        INNER JOIN OrderDetails OD ON O.OrderId = OD.OrderId
        INNER JOIN Products P ON OD.ProductId = P.Id
        WHERE O.UserId = @UserId
        ORDER BY O.OrderDate DESC";

                    using (SqlCommand cmdOrders = new SqlCommand(queryOrders, conn))
                    {
                        cmdOrders.Parameters.AddWithValue("@UserId", userId);

                        SqlDataAdapter adapter = new SqlDataAdapter(cmdOrders);
                        DataTable ordersTable = new DataTable();
                        adapter.Fill(ordersTable);

                        
                        flowLayoutPanelOrders.Controls.Clear();

                        
                        var groupedOrders = ordersTable.AsEnumerable()
                            .GroupBy(row => new
                            {
                                OrderId = row.Field<int>("OrderId"),
                                TotalAmount = row.Field<decimal>("TotalAmount"),
                                Status = row.Field<string>("Status"),
                                OrderDate = row.Field<DateTime>("OrderDate")
                            });

                        foreach (var group in groupedOrders)
                        {
                           
                            int orderId = group.Key.OrderId;
                            decimal totalAmount = group.Key.TotalAmount;
                            string status = group.Key.Status;
                            DateTime orderDate = group.Key.OrderDate;

                            
                            Guna2Panel orderPanel = new Guna2Panel
                            {
                                Size = new Size(350, 200),
                                BorderRadius = 10,
                                BorderColor = Color.Orange,
                                BorderThickness = 2,
                                Padding = new Padding(10),
                                Margin = new Padding(10),
                                BackColor = Color.White,
                                AutoScroll = true 
                            };

                            
                            Guna2HtmlLabel lblOrderDetails = new Guna2HtmlLabel
                            {
                                Text = $"Order Date: {orderDate:dd/MM/yyyy}\n" +
                                       $"Total: ₱{totalAmount:F2}\n" +
                                       $"Status: {status}",
                                Font = new Font("Arial", 10, FontStyle.Bold),
                                ForeColor = Color.Black,
                                AutoSize = false,
                                Size = new Size(orderPanel.Width - 20, 60),
                                Location = new Point(10, 10)
                            };
                            orderPanel.Controls.Add(lblOrderDetails);

                            int productYPosition = 80;

                            
                            foreach (var productRow in group)
                            {
                                string productName = productRow.Field<string>("ProductName");
                                int quantity = productRow.Field<int>("Quantity");
                                decimal price = productRow.Field<decimal>("Price");

                                Guna2HtmlLabel lblProductDetails = new Guna2HtmlLabel
                                {
                                    Text = $"- {productName} x{quantity} @ ₱{price:F2}",
                                    Font = new Font("Arial", 9),
                                    ForeColor = Color.Gray,
                                    AutoSize = false,
                                    Size = new Size(orderPanel.Width - 20, 20),
                                    Location = new Point(10, productYPosition)
                                };

                                productYPosition += 25; 
                                orderPanel.Controls.Add(lblProductDetails);
                            }

                            
                            if (status == "Pending")
                            {
                                Guna2Button btnCancel = new Guna2Button
                                {
                                    Text = "Cancel Order",
                                    Size = new Size(120, 30),
                                    Location = new Point(10, productYPosition + 10), 
                                    Tag = orderId
                                };

                                btnCancel.Click += (sender, e) =>
                                {
                                    CancelOrder(orderId, orderPanel);
                                };

                                orderPanel.Controls.Add(btnCancel);
                            }

                            
                            flowLayoutPanelOrders.Controls.Add(orderPanel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading orders: {ex.Message}");
            }

        }
        private void CancelOrder(int orderId, Guna2Panel orderPanel)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    
                    string deleteOrderDetailsQuery = "DELETE FROM OrderDetails WHERE OrderId = @OrderId";
                    string deleteOrderQuery = "DELETE FROM Orders WHERE OrderId = @OrderId";

                    using (SqlCommand cmdDeleteDetails = new SqlCommand(deleteOrderDetailsQuery, conn))
                    {
                        cmdDeleteDetails.Parameters.AddWithValue("@OrderId", orderId);
                        cmdDeleteDetails.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDeleteOrder = new SqlCommand(deleteOrderQuery, conn))
                    {
                        cmdDeleteOrder.Parameters.AddWithValue("@OrderId", orderId);
                        cmdDeleteOrder.ExecuteNonQuery();
                    }
                }

                
                flowLayoutPanelOrders.Controls.Remove(orderPanel);
                MessageBox.Show("Order canceled successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while canceling the order: {ex.Message}");
            }
        }
        private void UserLogin(int userId)
        {
           
            flowLayoutPanelOrders.Controls.Clear();

            LoadOrders(userId);
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
        public static class CurrentUser
        {
            public static int UserId { get; set; } = -1;
            public static string Username { get; set; } = "Guest"; 
        }

        private void LoginUser(int userId, string username)
        {
            CurrentUser.UserId = userId;
            CurrentUser.Username = username;

                

            
            LoadOrders(CurrentUser.UserId);
        }

        private void RemoveOrder(int orderId, Guna2Panel orderPanel)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Delete the order from the database
                    string deleteQuery = "DELETE FROM Orders WHERE OrderId = @OrderId";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Remove the panel from the FlowLayoutPanel
                flowLayoutPanelOrders.Controls.Remove(orderPanel);
                MessageBox.Show("Order removed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while removing the order: {ex.Message}");
            }
        }


        private void btnProducts_Click(object sender, EventArgs e)
        {
            
            UserDashboard userDashboardForm = new UserDashboard(username, userId);
            userDashboardForm.Show();
            this.Close();
        }

        private void btnProducts_Click_1(object sender, EventArgs e)
        {
            UserDashboard userDashboardForm = new UserDashboard(username, userId);
            userDashboardForm.Show();   
            this.Close();
        }
        private void Logout()
        {
           
            flowLayoutPanelOrders.Controls.Clear();
            userId = 0;

            
            UserLogin loginForm = new UserLogin();
            loginForm.Show();
            this.Close();
        }
        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            Logout();
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
    }

