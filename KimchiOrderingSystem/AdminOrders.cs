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

namespace KimchiOrderingSystem
{
    public partial class AdminOrders : Form
    {
        public AdminOrders()
        {
            InitializeComponent();
        }


        private void btnHome_Click(object sender, EventArgs e)
        {
            AdminMain home = new AdminMain();
            home.Show();
            this.Hide();
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
            
        }

        private void LoadOrders()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to get orders grouped by users
                    string queryOrders = @"
                SELECT o.OrderId, o.UserId, u.FullName, u.Email, o.TotalAmount, o.Status, o.OrderDate
                FROM Orders o
                INNER JOIN Users u ON o.UserId = u.Id
                ORDER BY u.FullName, o.OrderDate";

                    SqlDataAdapter adapter = new SqlDataAdapter(queryOrders, conn);
                    DataTable ordersTable = new DataTable();
                    adapter.Fill(ordersTable);

                    // Clear the FlowLayoutPanel
                    flowLayoutPanelOrders.Controls.Clear();

                    // Group orders by UserId
                    var groupedOrders = ordersTable.AsEnumerable()
                        .GroupBy(row => new { UserId = row.Field<int>("UserId"), FullName = row.Field<string>("FullName") });

                    foreach (var group in groupedOrders)
                    {
                        // Create a panel for each user
                        Panel userPanel = new Panel
                        {
                            Size = new Size(400, 300), // Adjust height as needed
                            BorderStyle = BorderStyle.FixedSingle,
                            Margin = new Padding(10),
                            BackColor = Color.White,
                            AutoScroll = true // Enable scrolling for the panel
                        };

                        // Add user details
                        Label lblUserName = new Label
                        {
                            Text = $"User: {group.Key.FullName}",
                            Font = new Font("Arial", 12, FontStyle.Bold),
                            Location = new Point(10, 10)
                        };
                        userPanel.Controls.Add(lblUserName);

                        int yPosition = 40; // Start position for orders

                        foreach (var orderRow in group)
                        {
                            // Extract order details
                            int orderId = orderRow.Field<int>("OrderId");
                            decimal totalAmount = orderRow.Field<decimal>("TotalAmount");
                            string status = orderRow.Field<string>("Status");
                            DateTime orderDate = orderRow.Field<DateTime>("OrderDate");

                            // Create a panel for each order
                            Guna2Panel orderPanel = new Guna2Panel
                            {
                                Size = new Size(380, 200), // Adjust height for product list
                                BorderRadius = 10,
                                BorderThickness = 2,
                                BorderColor = Color.Orange,
                                Location = new Point(10, yPosition),
                                BackColor = Color.LightGray,
                                AutoScroll = true // Enable scrolling for the order panel
                            };

                            // Add order details
                            Label lblOrderDetails = new Label
                            {
                                Text = $"Order Date: {orderDate:dd/MM/yyyy}\nTotal: ${totalAmount:F2}\nStatus: {status}",
                                AutoSize = false,
                                Size = new Size(350, 60),
                                Location = new Point(10, 10)
                            };
                            orderPanel.Controls.Add(lblOrderDetails);

                            // Query to get products for this order
                            string queryProducts = @"
                        SELECT P.Name, OD.Quantity, OD.Price
                        FROM OrderDetails OD
                        INNER JOIN Products P ON OD.ProductId = P.Id
                        WHERE OD.OrderId = @OrderId";

                            using (SqlCommand cmdProducts = new SqlCommand(queryProducts, conn))
                            {
                                cmdProducts.Parameters.AddWithValue("@OrderId", orderId);

                                SqlDataAdapter productAdapter = new SqlDataAdapter(cmdProducts);
                                DataTable productsTable = new DataTable();
                                productAdapter.Fill(productsTable);

                                int productYPosition = 80;

                                // Add product details to the panel
                                foreach (DataRow productRow in productsTable.Rows)
                                {
                                    string productName = productRow["Name"].ToString();
                                    int quantity = Convert.ToInt32(productRow["Quantity"]);
                                    decimal price = Convert.ToDecimal(productRow["Price"]);

                                    Label lblProductDetails = new Label
                                    {
                                        Text = $"- {productName} x{quantity} @ ${price:F2}",
                                        Font = new Font("Arial", 9),
                                        ForeColor = Color.Gray,
                                        AutoSize = false,
                                        Size = new Size(orderPanel.Width - 20, 20),
                                        Location = new Point(10, productYPosition)
                                    };

                                    productYPosition += 25; // Adjust position for the next product
                                    orderPanel.Controls.Add(lblProductDetails);
                                }
                            }

                            // Add Mark as Done button
                            Button btnMarkDone = new Button
                            {
                                Text = "Mark as Done",
                                Size = new Size(100, 30),
                                Location = new Point(10, 150),
                                Tag = orderId
                            };
                            btnMarkDone.Click += (s, e) => UpdateOrderStatus(orderId, "Done");
                            orderPanel.Controls.Add(btnMarkDone);

                            // Add Decline button
                            Button btnDecline = new Button
                            {
                                Text = "Decline",
                                Size = new Size(100, 30),
                                Location = new Point(120, 150),
                                Tag = orderId
                            };
                            btnDecline.Click += (s, e) => UpdateOrderStatus(orderId, "Declined");
                            orderPanel.Controls.Add(btnDecline);

                            userPanel.Controls.Add(orderPanel);
                            yPosition += 210; // Adjust position for the next order
                        }

                        flowLayoutPanelOrders.Controls.Add(userPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading orders: {ex.Message}");
            }
        }

        private void UpdateOrderStatus(int orderId, string status)
        {
            // Confirm before marking as Done
            if (status == "Done")
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to mark Order {orderId} as Done?",
                    "Confirm Action",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result != DialogResult.Yes)
                {
                    return; // Abort if the user selects "No"
                }
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show($"Order {orderId} has been marked as {status}.");

                // Remove the order panel after updating the status
                RemoveOrderPanel(orderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the order: {ex.Message}");
            }
        }

        private void RemoveOrderPanel(int orderId)
        {
            foreach (Control userPanel in flowLayoutPanelOrders.Controls)
            {
                foreach (Control orderPanel in userPanel.Controls)
                {
                    // Check if the order panel contains the specific orderId
                    if (orderPanel is Panel && orderPanel.Controls.OfType<Button>().Any(btn => (int)btn.Tag == orderId))
                    {
                        userPanel.Controls.Remove(orderPanel); // Remove the order panel
                        return; // Exit after removing
                    }
                }
            }
        }
        private void AdminOrders_Load(object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            InitialForm initialForm = new InitialForm();
            initialForm.Show();
            this.Close();
        }
    }
}
