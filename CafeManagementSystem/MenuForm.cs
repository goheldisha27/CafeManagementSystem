
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CafeManagementSystem
{
    public partial class MenuForm : Form
    {
        private DataGridView menuGrid;
        private TextBox itemNameTextBox;
        private NumericUpDown priceInput;

        public MenuForm()
        {
            InitializeComponent();
            CreateMenuScreen();
            LoadDefaultMenu();
        }

        private void CreateMenuScreen()
        {
            Text = "Cafe Management System - Menu";
            Size = new Size(850, 550);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            Label title = new Label();
            title.Text = "MENU MANAGEMENT";
            title.Font = new Font("Arial", 22, FontStyle.Bold);
            title.ForeColor = Color.SaddleBrown;
            title.AutoSize = true;
            title.Location = new Point(230, 20);
            Controls.Add(title);

            Label nameLabel = new Label();
            nameLabel.Text = "Item Name:";
            nameLabel.Location = new Point(40, 90);
            nameLabel.AutoSize = true;
            Controls.Add(nameLabel);

            itemNameTextBox = new TextBox();
            itemNameTextBox.Location = new Point(130, 85);
            itemNameTextBox.Width = 180;
            Controls.Add(itemNameTextBox);

            Label priceLabel = new Label();
            priceLabel.Text = "Price (Rs.):";
            priceLabel.Location = new Point(340, 90);
            priceLabel.AutoSize = true;
            Controls.Add(priceLabel);

            priceInput = new NumericUpDown();
            priceInput.Location = new Point(420, 85);
            priceInput.Width = 120;
            priceInput.Minimum = 1;
            priceInput.Maximum = 100000;
            priceInput.DecimalPlaces = 2;
            Controls.Add(priceInput);

            Button addButton = new Button();
            addButton.Text = "Add Item";
            addButton.Location = new Point(570, 80);
            addButton.Size = new Size(120, 35);
            addButton.BackColor = Color.SaddleBrown;
            addButton.ForeColor = Color.White;
            addButton.Click += AddItem;
            Controls.Add(addButton);

            Button refreshButton = new Button();
            refreshButton.Text = "Refresh Menu";
            refreshButton.Location = new Point(40, 145);
            refreshButton.Size = new Size(120, 35);
            refreshButton.Click += (s, e) => LoadDefaultMenu();
            Controls.Add(refreshButton);

            Button deleteButton = new Button();
            deleteButton.Text = "Delete Selected";
            deleteButton.Location = new Point(180, 145);
            deleteButton.Size = new Size(140, 35);
            deleteButton.BackColor = Color.Firebrick;
            deleteButton.ForeColor = Color.White;
            deleteButton.Click += DeleteItem;
            Controls.Add(deleteButton);

            menuGrid = new DataGridView();
            menuGrid.Location = new Point(40, 200);
            menuGrid.Size = new Size(750, 270);
            menuGrid.ReadOnly = true;
            menuGrid.AllowUserToAddRows = false;
            menuGrid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            menuGrid.MultiSelect = false;
            menuGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            menuGrid.BackgroundColor = Color.White;
            Controls.Add(menuGrid);
        }

        private void LoadDefaultMenu()
        {
            try
            {
                using (SqlConnection con = DbHelper.GetConnection())
                {
                    con.Open();

                    string query =
                        "SELECT ItemId, ItemName, Price " +
                        "FROM MenuItems ORDER BY ItemId";

                    using (SqlDataAdapter adapter =
                           new SqlDataAdapter(query, con))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        menuGrid.DataSource = table;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load menu items.\n\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void AddItem(object sender, EventArgs e)
        {
            string itemName = itemNameTextBox.Text.Trim();
            decimal price = priceInput.Value;

            if (string.IsNullOrWhiteSpace(itemName))
            {
                MessageBox.Show("Please enter an item name.");
                itemNameTextBox.Focus();
                return;
            }

            try
            {
                using (SqlConnection con = DbHelper.GetConnection())
                {
                    con.Open();

                    string query =
                        "INSERT INTO MenuItems (ItemName, Price) " +
                        "VALUES (@name, @price)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add(
                            "@name", SqlDbType.NVarChar, 100
                        ).Value = itemName;

                        cmd.Parameters.Add(
                            "@price", SqlDbType.Decimal
                        ).Value = price;

                        cmd.Parameters["@price"].Precision = 10;
                        cmd.Parameters["@price"].Scale = 2;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Menu item added successfully!",
                    "Success");

                itemNameTextBox.Clear();
                priceInput.Value = 1;

                LoadDefaultMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to add item.\n\n" + ex.Message,
                    "Database Error");
            }
        }

        private void DeleteItem(object sender, EventArgs e)
        {
            if (menuGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to delete.");
                return;
            }

            int itemId = Convert.ToInt32(
                menuGrid.SelectedRows[0].Cells["ItemId"].Value);

            string itemName = Convert.ToString(
                menuGrid.SelectedRows[0].Cells["ItemName"].Value);

            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete " + itemName + "?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                using (SqlConnection con = DbHelper.GetConnection())
                {
                    con.Open();

                    string query =
                        "DELETE FROM MenuItems WHERE ItemId = @id";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value =
                            itemId;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Menu item deleted successfully.");
                LoadDefaultMenu();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to delete item.\n\n" + ex.Message +
                    "\n\nIf this item is referenced by an existing order, " +
                    "the database may prevent deletion.",
                    "Database Error");
            }
        }
    }
}


