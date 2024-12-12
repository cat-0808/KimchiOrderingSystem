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
    public partial class AdminProducts : Form
    {
        public AdminProducts()
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

        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            AdminOrders order = new AdminOrders();
            order.Show();
            this.Hide();
        }
        
        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            string productName = txtProductName.Text.Trim();
            decimal productPrice;
            int stockQuantity;
            string description = txtDescription.Text.Trim();

            
            if (lblImagePath == null || string.IsNullOrWhiteSpace(lblImagePath.Text))
            {
                MessageBox.Show("Please upload an image for the product.");
                return;
            }

            string imagePath = lblImagePath.Text.Trim();


            if (!decimal.TryParse(txtProductPrice.Text.Trim(), out productPrice) || productPrice <= 0)
            {
                MessageBox.Show("Please enter a valid positive price.");
                return;
            }

            
            if (!int.TryParse(txtStockQuantity.Text.Trim(), out stockQuantity) || stockQuantity < 0)
            {
                MessageBox.Show("Please enter a valid non-negative stock quantity.");
                return;
            }

            
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(description))
            {
                MessageBox.Show("Please fill in all required fields (Product Name and Description).");
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO Products (Name, Price, StockQuantity, Description, ImagePath) VALUES (@Name, @Price, @StockQuantity, @Description, @ImagePath)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", productName);
                        cmd.Parameters.AddWithValue("@Price", productPrice);
                        cmd.Parameters.AddWithValue("@StockQuantity", stockQuantity);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@ImagePath", imagePath);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Product added successfully!");
                        LoadProducts();
                        LoadProductsForPOS();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private void LoadProducts()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, Name, Price, StockQuantity, Description, ImagePath FROM Products";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    guna2DataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void btnUploadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedImagePath = openFileDialog.FileName;
                    // Display the file path in the label
                    lblImagePath.Text = selectedImagePath;
                    pictureBoxPreview.Image = Image.FromFile(selectedImagePath);
                }
            }
        }

        private void AdminProducts_Load(object sender, EventArgs e)
        {
            LoadProducts();
            LoadProductsForPOS();
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            if (guna2DataGridView1.SelectedRows.Count > 0)
            {
                int productId = Convert.ToInt32(guna2DataGridView1.SelectedRows[0].Cells["Id"].Value);

                try
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // Delete related records from Cart
                        string deleteCartQuery = "DELETE FROM Cart WHERE ProductId = @ProductId";
                        using (SqlCommand cartCmd = new SqlCommand(deleteCartQuery, conn))
                        {
                            cartCmd.Parameters.AddWithValue("@ProductId", productId);
                            cartCmd.ExecuteNonQuery();
                        }

                        // Delete the product from Products
                        string deleteProductQuery = "DELETE FROM Products WHERE Id = @Id";
                        using (SqlCommand productCmd = new SqlCommand(deleteProductQuery, conn))
                        {
                            productCmd.Parameters.AddWithValue("@Id", productId);
                            productCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Product deleted successfully!");
                        LoadProducts();
                      
                        LoadProductsForPOS();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.");
            }

        }
        private void LoadProductsForPOS()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DatabaseConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT Id, Name, Price, StockQuantity, Description, ImagePath FROM Products";
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
                            Text = $"Price: ${price:F2}",
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

                        // Add the controls to the product panel
                        productPanel.Controls.Add(imageContainer);
                        productPanel.Controls.Add(lblName);
                        productPanel.Controls.Add(lblPrice);
                        productPanel.Controls.Add(lblStock);

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

        private void guna2GradientTileButton2_Click(object sender, EventArgs e)
        {
            InitialForm initialForm = new InitialForm();
            initialForm.Show();
            this.Close();
        }
    }
}
