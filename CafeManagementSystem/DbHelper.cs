

using System;
using System.Data;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace CafeManagementSystem
{
    public static class DbHelper
    {
        // Connection to SQL Server LocalDB
        private static readonly string connectionString =
            @"Server=(localdb)\MSSQLLocalDB;
              Database=CafeDB;
              Integrated Security=True;
              TrustServerCertificate=True;";

        // Connection to the master database
        private static readonly string masterConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;
              Database=master;
              Integrated Security=True;
              TrustServerCertificate=True;";

        // Get database connection
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // Create database and tables
        public static void InitializeDatabase()
        {
            // Step 1: Create database if it does not exist
            using (SqlConnection master =
                new SqlConnection(masterConnectionString))
            {
                master.Open();

                string createDatabase = @"
                    IF DB_ID('CafeDB') IS NULL
                    BEGIN
                        CREATE DATABASE CafeDB;
                    END;
                ";

                using (SqlCommand cmd =
                    new SqlCommand(createDatabase, master))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // Step 2: Create tables
            using (SqlConnection con = GetConnection())
            {
                con.Open();

                string createTables = @"
                    IF OBJECT_ID('dbo.MenuItems', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.MenuItems
                        (
                            ItemId INT IDENTITY(1,1) PRIMARY KEY,
                            ItemName NVARCHAR(100) NOT NULL UNIQUE,
                            Price DECIMAL(10,2) NOT NULL
                        );
                    END;

                    IF OBJECT_ID('dbo.Orders', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Orders
                        (
                            OrderId INT IDENTITY(1,1) PRIMARY KEY,
                            OrderDate DATETIME2 NOT NULL
                                DEFAULT SYSDATETIME(),
                            Subtotal DECIMAL(10,2) NOT NULL,
                            GST DECIMAL(10,2) NOT NULL,
                            GrandTotal DECIMAL(10,2) NOT NULL,
                            PaymentMethod NVARCHAR(30) NOT NULL
                        );
                    END;

                    IF OBJECT_ID('dbo.OrderDetails', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.OrderDetails
                        (
                            DetailId INT IDENTITY(1,1) PRIMARY KEY,
                            OrderId INT NOT NULL,
                            ItemName NVARCHAR(100) NOT NULL,
                            UnitPrice DECIMAL(10,2) NOT NULL,
                            Quantity INT NOT NULL,
                            LineTotal DECIMAL(10,2) NOT NULL,

                            FOREIGN KEY (OrderId)
                                REFERENCES dbo.Orders(OrderId)
                        );
                    END;
                ";

                using (SqlCommand cmd =
                    new SqlCommand(createTables, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Step 3: Insert sample menu items
                string insertMenu = @"
                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.MenuItems
                        WHERE ItemName = 'Coffee')
                        INSERT INTO dbo.MenuItems
                        (ItemName, Price)
                        VALUES ('Coffee', 80);

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.MenuItems
                        WHERE ItemName = 'Tea')
                        INSERT INTO dbo.MenuItems
                        (ItemName, Price)
                        VALUES ('Tea', 30);

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.MenuItems
                        WHERE ItemName = 'Cold Coffee')
                        INSERT INTO dbo.MenuItems
                        (ItemName, Price)
                        VALUES ('Cold Coffee', 120);

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.MenuItems
                        WHERE ItemName = 'Sandwich')
                        INSERT INTO dbo.MenuItems
                        (ItemName, Price)
                        VALUES ('Sandwich', 100);

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.MenuItems
                        WHERE ItemName = 'Burger')
                        INSERT INTO dbo.MenuItems
                        (ItemName, Price)
                        VALUES ('Burger', 150);

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.MenuItems
                        WHERE ItemName = 'Pizza')
                        INSERT INTO dbo.MenuItems
                        (ItemName, Price)
                        VALUES ('Pizza', 200);

                    IF NOT EXISTS (
                        SELECT 1 FROM dbo.MenuItems
                        WHERE ItemName = 'French Fries')
                        INSERT INTO dbo.MenuItems
                        (ItemName, Price)
                        VALUES ('French Fries', 90);
                ";

                using (SqlCommand cmd =
                    new SqlCommand(insertMenu, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Save order and order items
        public static int SaveOrder(
            DataGridView dgv,
            decimal subtotal,
            string paymentMethod)
        {
            decimal gst = Math.Round(
                subtotal * 0.05m, 2);

            decimal grandTotal = subtotal + gst;

            using (SqlConnection con = GetConnection())
            {
                con.Open();

                using (SqlTransaction transaction =
                    con.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Save main order
                        string orderSql = @"
                            INSERT INTO dbo.Orders
                            (Subtotal, GST, GrandTotal, PaymentMethod)
                            OUTPUT INSERTED.OrderId
                            VALUES
                            (@Subtotal, @GST, @GrandTotal, @PaymentMethod);
                        ";

                        int orderId;

                        using (SqlCommand cmd =
                            new SqlCommand(
                                orderSql, con, transaction))
                        {
                            AddMoneyParameter(
                                cmd, "@Subtotal", subtotal);

                            AddMoneyParameter(
                                cmd, "@GST", gst);

                            AddMoneyParameter(
                                cmd, "@GrandTotal", grandTotal);

                            cmd.Parameters.Add(
                                "@PaymentMethod",
                                SqlDbType.NVarChar, 30
                            ).Value = paymentMethod;

                            orderId = Convert.ToInt32(
                                cmd.ExecuteScalar());
                        }

                        // Step 2: Save each ordered item
                        string detailSql = @"
                            INSERT INTO dbo.OrderDetails
                            (OrderId, ItemName, UnitPrice,
                             Quantity, LineTotal)
                            VALUES
                            (@OrderId, @ItemName, @UnitPrice,
                             @Quantity, @LineTotal);
                        ";

                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            if (row.IsNewRow)
                                continue;

                            using (SqlCommand cmd =
                                new SqlCommand(
                                    detailSql, con, transaction))
                            {
                                cmd.Parameters.Add(
                                    "@OrderId", SqlDbType.Int
                                ).Value = orderId;

                                cmd.Parameters.Add(
                                    "@ItemName",
                                    SqlDbType.NVarChar, 100
                                ).Value = Convert.ToString(
                                    row.Cells["Item"].Value);

                                AddMoneyParameter(
                                    cmd,
                                    "@UnitPrice",
                                    Convert.ToDecimal(
                                        row.Cells["Price"].Value));

                                cmd.Parameters.Add(
                                    "@Quantity", SqlDbType.Int
                                ).Value = Convert.ToInt32(
                                    row.Cells["Quantity"].Value);

                                AddMoneyParameter(
                                    cmd,
                                    "@LineTotal",
                                    Convert.ToDecimal(
                                        row.Cells["Total"].Value));

                                cmd.ExecuteNonQuery();
                            }
                        }

                        // Step 3: Commit transaction
                        transaction.Commit();

                        return orderId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Helper method for decimal values
        private static void AddMoneyParameter(
            SqlCommand cmd,
            string parameterName,
            decimal value)
        {
            SqlParameter parameter = cmd.Parameters.Add(
                parameterName, SqlDbType.Decimal);

            parameter.Precision = 10;
            parameter.Scale = 2;
            parameter.Value = value;
        }
    }
}
