
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class Form1 : Form
    {
        private ComboBox cmbMenu;
        private NumericUpDown numQuantity;
        private DataGridView dgvOrder;
        private Label lblSubtotal;
        private Label lblGST;
        private Label lblGrandTotal;

        private decimal subtotal = 0;
        private const decimal GST_RATE = 0.05m;

        public Form1()
        {
            InitializeComponent();
            CreateCafeScreen();
            LoadMenuItems();
        }

        private void CreateCafeScreen()
        {
            Text = "Cafe Management System - Billing";
            Size = new Size(950, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            Label title = new Label();
            title.Text = "CAFE BILLING SYSTEM";
            title.Font = new Font("Arial", 22, FontStyle.Bold);
            title.ForeColor = Color.SaddleBrown;
            title.AutoSize = true;
            title.Location = new Point(300, 20);
            Controls.Add(title);

            Label itemLabel = new Label();
            itemLabel.Text = "Select Item:";
            itemLabel.Location = new Point(40, 90);
            itemLabel.AutoSize = true;
            Controls.Add(itemLabel);

            cmbMenu = new ComboBox();
            cmbMenu.Location = new Point(130, 85);
            cmbMenu.Size = new Size(220, 30);
            cmbMenu.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMenu.DisplayMember = "ItemName";
            cmbMenu.ValueMember = "Price";
            Controls.Add(cmbMenu);

            Label quantityLabel = new Label();
            quantityLabel.Text = "Quantity:";
            quantityLabel.Location = new Point(390, 90);
            quantityLabel.AutoSize = true;
            Controls.Add(quantityLabel);

            numQuantity = new NumericUpDown();
            numQuantity.Location = new Point(460, 85);
            numQuantity.Width = 80;
            numQuantity.Minimum = 1;
            numQuantity.Maximum = 100;
            numQuantity.Value = 1;
            Controls.Add(numQuantity);

            Button btnAdd = new Button();
            btnAdd.Text = "Add to Order";
            btnAdd.Location = new Point(570, 80);
            btnAdd.Size = new Size(130, 35);
            btnAdd.BackColor = Color.SaddleBrown;
            btnAdd.ForeColor = Color.White;
            btnAdd.Click += BtnAdd_Click;
            Controls.Add(btnAdd);

            dgvOrder = new DataGridView();
            dgvOrder.Location = new Point(40, 140);
            dgvOrder.Size = new Size(850, 300);
            dgvOrder.AllowUserToAddRows = false;
            dgvOrder.ReadOnly = true;
            dgvOrder.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvOrder.MultiSelect = false;
            dgvOrder.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrder.BackgroundColor = Color.White;

            dgvOrder.Columns.Add("Item", "Item Name");
            dgvOrder.Columns.Add("Price", "Price (Rs.)");
            dgvOrder.Columns.Add("Quantity", "Quantity");
            dgvOrder.Columns.Add("Total", "Total (Rs.)");

            Controls.Add(dgvOrder);

            Button btnRemove = new Button();
            btnRemove.Text = "Remove Item";
            btnRemove.Location = new Point(40, 455);
            btnRemove.Size = new Size(130, 40);
            btnRemove.Click += BtnRemove_Click;
            Controls.Add(btnRemove);

            lblSubtotal = new Label();
            lblSubtotal.Text = "Subtotal: Rs. 0.00";
            lblSubtotal.Location = new Point(620, 455);
            lblSubtotal.AutoSize = true;
            lblSubtotal.Font = new Font("Arial", 11, FontStyle.Bold);
            Controls.Add(lblSubtotal);

            lblGST = new Label();
            lblGST.Text = "GST (5%): Rs. 0.00";
            lblGST.Location = new Point(620, 490);
            lblGST.AutoSize = true;
            lblGST.Font = new Font("Arial", 11);
            Controls.Add(lblGST);

            lblGrandTotal = new Label();
            lblGrandTotal.Text = "Grand Total: Rs. 0.00";
            lblGrandTotal.Location = new Point(620, 525);
            lblGrandTotal.AutoSize = true;
            lblGrandTotal.Font = new Font("Arial", 13, FontStyle.Bold);
            lblGrandTotal.ForeColor = Color.DarkGreen;
            Controls.Add(lblGrandTotal);

            Button btnCheckout = new Button();
            btnCheckout.Text = "Checkout";
            btnCheckout.Location = new Point(390, 570);
            btnCheckout.Size = new Size(150, 45);
            btnCheckout.BackColor = Color.DarkGreen;
            btnCheckout.ForeColor = Color.White;
            btnCheckout.Font = new Font("Arial", 11, FontStyle.Bold);
            btnCheckout.Click += BtnCheckout_Click;
            Controls.Add(btnCheckout);
        }

        private void LoadMenuItems()
        {
            try
            {
                DataTable menu = new DataTable();

                using (var con = DbHelper.GetConnection())
                {
                    con.Open();

                    string query =
                        "SELECT ItemName, Price " +
                        "FROM MenuItems ORDER BY ItemId";

                   

                    using (var cmd =
                           new Microsoft.Data.SqlClient.SqlCommand(query, con))
                    using (var adapter =
                           new Microsoft.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        adapter.Fill(menu);
                    }
                }

                cmbMenu.DataSource = menu;
                cmbMenu.DisplayMember = "ItemName";
                cmbMenu.ValueMember = "Price";

                if (menu.Rows.Count == 0)
                {
                    MessageBox.Show("No menu items found in the database.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to load menu items.\n\n" + ex.Message,
                    "Database Error");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbMenu.SelectedItem == null)
            {
                MessageBox.Show("Please select a menu item.");
                return;
            }

            string itemName = cmbMenu.Text;
            decimal price = Convert.ToDecimal(cmbMenu.SelectedValue);
            int quantity = (int)numQuantity.Value;

            // Add quantity to the existing row if item already exists.
            foreach (DataGridViewRow row in dgvOrder.Rows)
            {
                if (Convert.ToString(row.Cells["Item"].Value) == itemName)
                {
                    int oldQuantity =
                        Convert.ToInt32(row.Cells["Quantity"].Value);

                    int newQuantity = oldQuantity + quantity;

                    row.Cells["Quantity"].Value = newQuantity;
                    row.Cells["Total"].Value =
                        (price * newQuantity).ToString("0.00");

                    UpdateBill();
                    return;
                }
            }

            decimal lineTotal = price * quantity;

            dgvOrder.Rows.Add(
                itemName,
                price.ToString("0.00"),
                quantity,
                lineTotal.ToString("0.00"));

            UpdateBill();
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (dgvOrder.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to remove.");
                return;
            }

            dgvOrder.Rows.Remove(dgvOrder.SelectedRows[0]);
            UpdateBill();
        }

        private void UpdateBill()
        {
            subtotal = 0;

            foreach (DataGridViewRow row in dgvOrder.Rows)
            {
                subtotal += Convert.ToDecimal(row.Cells["Total"].Value);
            }

            decimal gst = Math.Round(
                subtotal * GST_RATE, 2, MidpointRounding.AwayFromZero);

            decimal grandTotal = subtotal + gst;

            lblSubtotal.Text = "Subtotal: Rs. " + subtotal.ToString("0.00");
            lblGST.Text = "GST (5%): Rs. " + gst.ToString("0.00");
            lblGrandTotal.Text =
                "Grand Total: Rs. " + grandTotal.ToString("0.00");
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (dgvOrder.Rows.Count == 0)
            {
                MessageBox.Show("Please add items before checkout.");
                return;
            }

            UpdateBill();

            decimal gst = Math.Round(
                subtotal * GST_RATE, 2, MidpointRounding.AwayFromZero);

            decimal grandTotal = subtotal + gst;

            try
            {
                int orderId = DbHelper.SaveOrder(
                    dgvOrder, subtotal, "Cash");

                MessageBox.Show(
                    "CAFE RECEIPT\n\n" +
                    "Order ID: " + orderId + "\n" +
                    "Subtotal: Rs. " + subtotal.ToString("0.00") + "\n" +
                    "GST (5%): Rs. " + gst.ToString("0.00") + "\n" +
                    "Grand Total: Rs. " + grandTotal.ToString("0.00") +
                    "\n\nPayment Method: Cash\n\nThank you for visiting!",
                    "Order Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                dgvOrder.Rows.Clear();
                UpdateBill();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not save order.\n\n" + ex.Message,
                    "Checkout Error");
            }
        }
    }
}

