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
using static KimchiOrderingSystem.Your_Orders;

namespace KimchiOrderingSystem
{
    public partial class UserDashboard : Form
    {
        private string loggedInUsername;

        public UserDashboard(string username, int userId)
        {
            InitializeComponent();
            loggedInUsername = string.IsNullOrEmpty(username) ? "Guest" : username;
            this.username = username;
            this.userId = userId;
        }

        private void UserDashboard_Load(object sender, EventArgs e)
        {
            LoadProductsForPOS();
            lblUsername.Text = $"Welcome {loggedInUsername}!";

        }
        private string username;
        
        private int userId;

        public void SetUserId(int loggedInUserId)
        {
            userId = loggedInUserId;
        }
        private void LoadProductsForPOS()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to fetch products available to the logged-in user
                    string query = @"
                SELECT Id, Name, Price, StockQuantity, Description, ImagePath
                FROM Products";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Clear the FlowLayoutPanel before adding new controls
                    flowLayoutPanelProducts.Controls.Clear();

                    foreach (DataRow row in dt.Rows)
                    {
                        // Retrieve the data for the product
                        string productName = row["Name"].ToString();
                        decimal price = Convert.ToDecimal(row["Price"]);
                        int stockQuantity = Convert.ToInt32(row["StockQuantity"]);
                        string description = row["Description"].ToString();
                        string imagePath = row["ImagePath"].ToString();
                        int productId = Convert.ToInt32(row["Id"]); // Retrieve product ID

                        // Create a Guna2Panel to represent the product and its details
                        Guna2Panel productPanel = new Guna2Panel
                        {
                            Size = new Size(250, 300), // Set the size for each product panel
                            Margin = new Padding(10),
                            BorderRadius = 10, // Rounded corners
                            BorderColor = Color.Orange, // Orange border color
                            BorderThickness = 2, // Thickness of the border
                            FillColor = Color.White // Background color
                        };

                        // Create a container for the image
                        Panel imageContainer = new Panel
                        {
                            Size = new Size(150, 150),
                            Location = new Point((productPanel.Width - 150) / 2, 10), // Center horizontally
                            BackColor = Color.Transparent
                        };

                        // Create a Guna2PictureBox for the product image
                        Guna2PictureBox productImage = new Guna2PictureBox
                        {
                            Size = new Size(150, 150), // Image size
                            SizeMode = PictureBoxSizeMode.StretchImage
                        };

                        // Check if the ImagePath is valid
                        if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
                        {
                            Image placeholder = new Bitmap(150, 150); // Create a blank 150x150 image
                            using (Graphics g = Graphics.FromImage(placeholder))
                            {
                                g.Clear(Color.Gray); // Fill the image with a gray color as a placeholder
                            }
                            productImage.Image = placeholder;
                        }
                        else
                        {
                            productImage.Image = Image.FromFile(imagePath); // Load the product image
                        }

                        // Add the image to the container
                        imageContainer.Controls.Add(productImage);

                        // Create a Guna2HtmlLabel for the product name
                        Guna2HtmlLabel lblName = new Guna2HtmlLabel
                        {
                            Text = productName,
                            Font = new Font("Arial", 12, FontStyle.Bold),
                            ForeColor = Color.Black, // Text color
                            AutoSize = true,
                            Location = new Point(10, imageContainer.Bottom + 5) // Place below the image
                        };

                        // Create a Guna2HtmlLabel for the product price
                        Guna2HtmlLabel lblPrice = new Guna2HtmlLabel
                        {
                            Text = $"Price: ₱{price:F2}",
                            Font = new Font("Arial", 10),
                            ForeColor = Color.Black, // Text color
                            AutoSize = true,
                            Location = new Point(10, lblName.Bottom + 5)
                        };

                        // Create a Guna2HtmlLabel for the stock quantity
                        Guna2HtmlLabel lblStock = new Guna2HtmlLabel
                        {
                            Text = $"Stock: {stockQuantity}",
                            Font = new Font("Arial", 10),
                            ForeColor = Color.Black, // Text color
                            AutoSize = true,
                            Location = new Point(10, lblPrice.Bottom + 5)
                        };

                        
                        // Add to Cart button
                        Guna2Button btnAddToCart = new Guna2Button
                        {
                            Text = "Add to Cart",
                            Size = new Size(100, 30),
                            Location = new Point(80, lblStock.Bottom + 5) // Next to quantity selector
                        };

                        // Button click event to add product to cart
                        btnAddToCart.Click += (sender, e) =>
                        {
                            int quantity = 1; // Get selected quantity

                            // Ensure sufficient stock
                            if (quantity > stockQuantity)
                            {
                                MessageBox.Show("Not enough stock available.");
                                return;
                            }

                            AddToCart(userId, productId, quantity);
                        };

                        // Add controls to the product panel
                        productPanel.Controls.Add(imageContainer);
                        productPanel.Controls.Add(lblName);
                        productPanel.Controls.Add(lblPrice);
                        productPanel.Controls.Add(lblStock);
                        
                        productPanel.Controls.Add(btnAddToCart);

                        // Add the product panel to the FlowLayoutPanel
                        flowLayoutPanelProducts.Controls.Add(productPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading products: {ex.Message}");
            }
        }
        private void AddToCart(int userId, int productId, int quantity)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check stock availability
                    string stockQuery = "SELECT StockQuantity FROM Products WHERE Id = @ProductId";
                    using (SqlCommand stockCmd = new SqlCommand(stockQuery, conn))
                    {
                        stockCmd.Parameters.AddWithValue("@ProductId", productId);
                        int StockQuantity = Convert.ToInt32(stockCmd.ExecuteScalar());

                        if (!UserExists(userId))
                        {
                            MessageBox.Show("Invalid user. Please log in again.");
                            return;
                        }

                        // Check if the product is already in the cart
                        string query = "SELECT Quantity FROM Cart WHERE UserId = @UserId AND ProductId = @ProductId";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.Parameters.AddWithValue("@ProductId", productId);

                            object result = cmd.ExecuteScalar();
                            int existingQuantity = result != null ? Convert.ToInt32(result) : 0;

                            // Check if adding the new quantity exceeds stock
                            if (existingQuantity + quantity > StockQuantity)
                            {
                                MessageBox.Show("Adding this quantity exceeds available stock.");
                                return; // Exit the method if there's insufficient stock
                            }

                            if (result != null)
                            {
                                // Update the existing cart entry
                                query = "UPDATE Cart SET Quantity = @Quantity WHERE UserId = @UserId AND ProductId = @ProductId";
                                using (SqlCommand updateCmd = new SqlCommand(query, conn))
                                {
                                    updateCmd.Parameters.AddWithValue("@Quantity", existingQuantity + quantity);
                                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                                    updateCmd.Parameters.AddWithValue("@ProductId", productId);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                               
                                query = "INSERT INTO Cart (UserId, ProductId, Quantity) VALUES (@UserId, @ProductId, @Quantity)";
                                using (SqlCommand insertCmd = new SqlCommand(query, conn))
                                {
                                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                                    insertCmd.Parameters.AddWithValue("@ProductId", productId);
                                    insertCmd.Parameters.AddWithValue("@Quantity", quantity);
                                    insertCmd.ExecuteNonQuery();
                                }

                               
                            }
                            string updateStockQuery = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE Id = @ProductId";
                            using (SqlCommand updateStockCmd = new SqlCommand(updateStockQuery, conn))
                            {
                                updateStockCmd.Parameters.AddWithValue("@Quantity", quantity);
                                updateStockCmd.Parameters.AddWithValue("@ProductId", productId);
                                updateStockCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    
                    UpdateCartDisplay(userId); // Refresh cart display
                    LoadProductsForPOS(); // Refresh product stock display
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }

        private void UpdateCartDisplay(int userId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Query to fetch cart items for the logged-in user
                    string query = @"
                SELECT P.Id, P.Name, C.Quantity, P.Price, (C.Quantity * P.Price) AS TotalPrice
                FROM Cart C
                JOIN Products P ON C.ProductId = P.Id
                WHERE C.UserId = @UserId";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@UserId", userId);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Clear existing items in the cart FlowLayoutPanel
                    cartFlowLayoutPanel.Controls.Clear();

                    foreach (DataRow row in dt.Rows)
                    {
                        int productId = Convert.ToInt32(row["Id"]);
                        string productName = row["Name"].ToString();
                        int quantity = Convert.ToInt32(row["Quantity"]);
                        decimal price = Convert.ToDecimal(row["Price"]);
                        decimal totalPrice = Convert.ToDecimal(row["TotalPrice"]);

                        // Create a compact panel for each cart item
                        Guna2Panel cartItemPanel = new Guna2Panel
                        {
                            Size = new Size(300, 70), // Smaller height for compact display
                            Margin = new Padding(5),
                            BorderRadius = 8,
                            BorderColor = Color.Gray,
                            BorderThickness = 2,
                            FillColor = Color.White
                        };

                        // Add a small label for the product name
                        Label lblName = new Label
                        {
                            Text = productName,
                            Font = new Font("Arial", 9, FontStyle.Bold),
                            Location = new Point(10, 5), // Top-left corner
                            AutoSize = true
                        };

                        // Add a small label for the quantity
                        Label lblQuantity = new Label
                        {
                            Text = $"Qty: {quantity}",
                            Font = new Font("Arial", 8),
                            Location = new Point(10, 25), // Below the name
                            AutoSize = true
                        };

                        // Add a small label for the price
                        Label lblPrice = new Label
                        {
                            Text = $"Price: ₱{price:F2}",
                            Font = new Font("Arial", 8),
                            Location = new Point(150, 5), // Right side
                            AutoSize = true
                        };

                        // Add a small label for the total price
                        Label lblTotal = new Label
                        {
                            Text = $"Total: ₱{totalPrice:F2}",
                            Font = new Font("Arial", 8, FontStyle.Bold),
                            Location = new Point(150, 25), // Below the price
                            AutoSize = true
                        };

                        // Add a small remove button
                        Guna2Button btnRemove = new Guna2Button
                        {
                            Text = "X", // Compact button with "X" to denote removal
                            Size = new Size(30, 30),
                            Location = new Point(260, 20), // Positioned to the far right
                            BorderRadius = 15,
                            FillColor = Color.Red,
                            ForeColor = Color.White,
                            Font = new Font("Arial", 9, FontStyle.Bold)
                        };
                        btnRemove.Click += (sender, e) => RemoveFromCart(userId, productId);

                        // Add controls to the cart item panel
                        cartItemPanel.Controls.Add(lblName);
                        cartItemPanel.Controls.Add(lblQuantity);
                        cartItemPanel.Controls.Add(lblPrice);
                        cartItemPanel.Controls.Add(lblTotal);
                        cartItemPanel.Controls.Add(btnRemove);

                        // Add the panel to the FlowLayoutPanel
                        cartFlowLayoutPanel.Controls.Add(cartItemPanel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }

        }

        private void RemoveFromCart(int userId, int productId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Retrieve the quantity being removed
                    string getQuantityQuery = "SELECT Quantity FROM Cart WHERE UserId = @UserId AND ProductId = @ProductId";
                    int quantity = 0;

                    using (SqlCommand getQuantityCmd = new SqlCommand(getQuantityQuery, conn))
                    {
                        getQuantityCmd.Parameters.AddWithValue("@UserId", userId);
                        getQuantityCmd.Parameters.AddWithValue("@ProductId", productId);

                        object result = getQuantityCmd.ExecuteScalar();
                        if (result != null)
                        {
                            quantity = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("Product not found in the cart.");
                            return; // Exit if the product doesn't exist
                        }
                    }

                    // Remove the product from the cart (No alias for DELETE)
                    string deleteQuery = "DELETE FROM Cart WHERE UserId = @UserId AND ProductId = @ProductId";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@UserId", userId);
                        deleteCmd.Parameters.AddWithValue("@ProductId", productId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Restore the stock quantity
                    string restoreStockQuery = "UPDATE Products SET StockQuantity = StockQuantity + @Quantity WHERE Id = @ProductId";
                    using (SqlCommand restoreStockCmd = new SqlCommand(restoreStockQuery, conn))
                    {
                        restoreStockCmd.Parameters.AddWithValue("@Quantity", quantity);
                        restoreStockCmd.Parameters.AddWithValue("@ProductId", productId);
                        restoreStockCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Product removed from cart!");
                UpdateCartDisplay(userId); // Refresh cart display
                LoadProductsForPOS(); // Refresh product stock display
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while removing the product: {ex.Message}");
            }
        }
        private bool IsCartNotEmpty(int userid)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT COUNT(*) FROM Cart C WHERE C.UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId); // Pass userId as a parameter
                        int cartItemCount = Convert.ToInt32(cmd.ExecuteScalar());
                        return cartItemCount > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                return false;
            }
        }

        private void btnAdminLogin_Click(object sender, EventArgs e)
        {
            try
            {

                // Validate user inputs
                if (string.IsNullOrEmpty(txtAddress.Text) || string.IsNullOrEmpty(txtPhoneNumber.Text) || string.IsNullOrEmpty(txtFullName.Text))
                {
                    MessageBox.Show("Please provide your full name, phone number, and address before finalizing the order.");
                    return;
                }

                // Check if the cart is empty
                if (!IsCartNotEmpty(userId))
                {
                    MessageBox.Show("Your cart is empty. Please add items to your cart before finalizing the order.");
                    return;
                }

                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Begin transaction
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            decimal totalAmount = 0;
                            string calculateTotalQuery = @"
                        SELECT SUM(C.Quantity * P.Price) AS TotalAmount FROM Cart C INNER JOIN Products P ON C.ProductId = P.Id WHERE C.UserId = @UserId";

                            using (SqlCommand cmd = new SqlCommand(calculateTotalQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);

                                totalAmount = Convert.ToDecimal(cmd.ExecuteScalar());
                            }

                            // Insert the order into the Orders table
                            string insertOrderQuery = @"
                        INSERT INTO Orders (UserId, CustomerName, PhoneNumber, Address, OrderDate, Status, TotalAmount)
                        VALUES (@UserId, @CustomerName, @PhoneNumber, @Address, GETDATE(), 'Pending', @TotalAmount);
                        SELECT SCOPE_IDENTITY();";

                            int orderId;
                            using (SqlCommand cmd = new SqlCommand(insertOrderQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@CustomerName", txtFullName.Text);
                                cmd.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text);
                                cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                                cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);

                                
                                orderId = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // Insert each cart item into the OrderDetails table
                            string insertOrderDetailsQuery = @"INSERT INTO OrderDetails (OrderId, ProductId, Quantity, Price) SELECT @OrderId, C.ProductId, C.Quantity, C.Quantity * P.Price FROM Cart C INNER JOIN Products P ON C.ProductId = P.Id WHERE C.UserId = @UserId";

                            using (SqlCommand cmd = new SqlCommand(insertOrderDetailsQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@OrderId", orderId);
                                cmd.Parameters.AddWithValue("@UserId", userId); // Pass userId as a parameter
                                cmd.ExecuteNonQuery();
                            }

                            // Clear the cart
                            string clearCartQuery = "DELETE FROM Cart WHERE UserId = @UserId";
                            using (SqlCommand cmd = new SqlCommand(clearCartQuery, conn, transaction))
                            {

                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.ExecuteNonQuery();
                            }

                            // Commit transaction
                            transaction.Commit();

                            MessageBox.Show("Order finalized successfully!");
                            UpdateCartDisplay(userId); // Refresh the cart display
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"An error occurred while finalizing the order: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private bool UserExists(int userId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE Id = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while checking user existence: {ex.Message}");
                return false;
            }
        }
        
        private void btnProducts_Click(object sender, EventArgs e)
        {
            
            Your_Orders yourOrdersForm = new Your_Orders(username, userId);
            yourOrdersForm.Show();
            this.Close();
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

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

        private void LogoutUser()
        {
            // Reset global user state
            CurrentUser.UserId = -1;
            CurrentUser.Username = "Guest";

            // Redirect to the login screen or main form
            InitialForm initialForm = new InitialForm();
            initialForm.Show();
            this.Close();
        }
        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {
            LogoutUser();
        }
    }
}
