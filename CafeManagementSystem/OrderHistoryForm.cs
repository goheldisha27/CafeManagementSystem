
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CafeManagementSystem
{
    public partial class OrderHistoryForm : Form
    {
        private DataGridView ordersGrid;
        private Button refreshButton;

        public OrderHistoryForm()
        {
            InitializeComponent();
            CreateScreen();
            LoadOrders();
        }

        private void CreateScreen()
        {
            // Form settings
            this.Text = "Cafe Management System - Order History";
            this.Size = new Size(900, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // Title
            Label title = new Label();
            title.Text = "ORDER HISTORY";
            title.Font = new Font(
                "Segoe UI", 22, FontStyle.Bold);
            title.ForeColor = Color.DarkRed;
            title.Location = new Point(25, 20);
            title.AutoSize = true;
            this.Controls.Add(title);

            // Refresh button
            refreshButton = new Button();
            refreshButton.Text = "Refresh Orders";
            refreshButton.Location = new Point(700, 25);
            refreshButton.Size = new Size(140, 40);
            refreshButton.BackColor = Color.DarkRed;
            refreshButton.ForeColor = Color.White;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Click += RefreshButton_Click;
            this.Controls.Add(refreshButton);

            // Orders table
            ordersGrid = new DataGridView();
            ordersGrid.Location = new Point(25, 90);
            ordersGrid.Size = new Size(825, 380);

            ordersGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            ordersGrid.BackgroundColor = Color.White;
            ordersGrid.ReadOnly = true;
            ordersGrid.AllowUserToAddRows = false;
            ordersGrid.AllowUserToDeleteRows = false;

            ordersGrid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            ordersGrid.MultiSelect = false;
            ordersGrid.RowHeadersVisible = false;
            ordersGrid.AutoGenerateColumns = true;

            ordersGrid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.DarkRed;
            ordersGrid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            ordersGrid.EnableHeadersVisualStyles = false;
            ordersGrid.ColumnHeadersHeight = 35;
            ordersGrid.RowTemplate.Height = 30;

            this.Controls.Add(ordersGrid);

            // Back button
            Button backButton = new Button();
            backButton.Text = "Back to Dashboard";
            backButton.Location = new Point(25, 485);
            backButton.Size = new Size(160, 35);
            backButton.Click += (sender, e) => this.Close();
            this.Controls.Add(backButton);
        }

        private void RefreshButton_Click(
            object sender, EventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                DataTable table = new DataTable();

                using (SqlConnection con =
                       DbHelper.GetConnection())
                {
                    con.Open();

                    string query =
                        "SELECT OrderId, OrderDate, Subtotal, GST, " +
                        "GrandTotal, PaymentMethod " +
                        "FROM Orders ORDER BY OrderId DESC";

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(query, con))
                    {
                        adapter.Fill(table);
                    }
                }

                ordersGrid.DataSource = table;

                // Format amount columns
                if (ordersGrid.Columns.Contains("Subtotal"))
                {
                    ordersGrid.Columns["Subtotal"]
                        .DefaultCellStyle.Format = "N2";
                }

                if (ordersGrid.Columns.Contains("GST"))
                {
                    ordersGrid.Columns["GST"]
                        .DefaultCellStyle.Format = "N2";
                }

                if (ordersGrid.Columns.Contains("GrandTotal"))
                {
                    ordersGrid.Columns["GrandTotal"]
                        .DefaultCellStyle.Format = "N2";
                }

                if (table.Rows.Count == 0)
                {
                    MessageBox.Show(
                        "No orders found. Please create an order first.",
                        "Order History",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading order history:\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
