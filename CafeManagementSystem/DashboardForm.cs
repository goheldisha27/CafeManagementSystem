
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CafeManagementSystem
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
            CreateDashboard();
        }

        private void CreateDashboard()
        {
            this.Text = "Cafe Management System - Dashboard";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            Label title = new Label();
            title.Text = "CAFE MANAGEMENT SYSTEM";
            title.Font = new Font("Arial", 22, FontStyle.Bold);
            title.ForeColor = Color.SaddleBrown;
            title.Location = new Point(180, 30);
            title.AutoSize = true;
            this.Controls.Add(title);

            Label welcome = new Label();
            welcome.Text = "Welcome to Cafe Dashboard!";
            welcome.Font = new Font("Arial", 15);
            welcome.Location = new Point(280, 100);
            welcome.AutoSize = true;
            this.Controls.Add(welcome);

            Button billingButton = new Button();
            billingButton.Text = "Open Billing";
            billingButton.Size = new Size(200, 55);
            billingButton.Location = new Point(330, 190);
            billingButton.BackColor = Color.Peru;
            billingButton.ForeColor = Color.White;
            billingButton.Click += (s, e) =>
            {
                Form1 billing = new Form1();
                billing.Show();
            };
            this.Controls.Add(billingButton);


            Button menuButton = new Button();
            menuButton.Text = "Menu Management";
            menuButton.Size = new Size(200, 55);
            menuButton.Location = new Point(330, 260);
            menuButton.BackColor = Color.Sienna;
            menuButton.ForeColor = Color.White;

            menuButton.Click += (s, e) =>
            {
                MenuForm menu = new MenuForm();
                menu.ShowDialog();
            };

            this.Controls.Add(menuButton);


            Button historyButton = new Button();
            historyButton.Text = "Order History";
            historyButton.Size = new Size(200, 45);
            historyButton.Location = new Point(330, 330);
            historyButton.BackColor = Color.Sienna;
            historyButton.ForeColor = Color.White;

            historyButton.Click += (s, e) =>
            {
                OrderHistoryForm history = new OrderHistoryForm();
                history.ShowDialog();
            };

            this.Controls.Add(historyButton);



            Button exitButton = new Button();
            exitButton.Text = "Exit";
            exitButton.Size = new Size(200, 45);
            exitButton.Location = new Point(330, 400);
            exitButton.Click += (s, e) => this.Close();
            this.Controls.Add(exitButton);
        }
    }
}
