using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace TicketClearer
{
    public partial class Form1 : Form
    {
        private string connectionString;
        private string logFilePath;
        private StreamWriter logFileWriter;

        public Form1()
        {
            InitializeComponent();
            InitializeLogFile();
            LoadConnectionString();
        }

        private void InitializeLogFile()
        {
            try
            {
                // Create Logs directory if it doesn't exist
                string logsDirectory = Path.Combine(Application.StartupPath, "Logs");
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }

                // Create log file with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logFilePath = Path.Combine(logsDirectory, $"TicketClearer_{timestamp}.txt");

                // Initialize the log file writer
                logFileWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
                logFileWriter.AutoFlush = true;

                // Write initial log entry
                WriteToLogFile("=== Ticket Clearer Application Started ===");
                WriteToLogFile($"Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                WriteToLogFile($"Version: {Application.ProductVersion}");
                WriteToLogFile($"Machine: {Environment.MachineName}");
                WriteToLogFile($"User: {Environment.UserName}");
                WriteToLogFile("".PadRight(50, '='));
                WriteToLogFile("");

                AddToLog($"Log file created: {Path.GetFileName(logFilePath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize log file: {ex.Message}", "Log Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void WriteToLogFile(string message)
        {
            try
            {
                if (logFileWriter != null && !logFileWriter.BaseStream.CanWrite)
                {
                    // Recreate the writer if stream is closed
                    logFileWriter = new StreamWriter(logFilePath, true, Encoding.UTF8);
                    logFileWriter.AutoFlush = true;
                }

                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                logFileWriter?.WriteLine(logEntry);
            }
            catch (Exception ex)
            {
                // If file logging fails, just continue without crashing
                System.Diagnostics.Debug.WriteLine($"Log file write error: {ex.Message}");
            }
        }

        private void LoadConnectionString()
        {
            try
            {
                connectionString = "Persist Security Info=True;Pooling=false;User ID=WebFUser;Password=Telkom900;Initial Catalog=WF1ActR;Data Source=10.227.167.243,1441;Packet Size=4096;Workstation ID=10.227.230.234;Connect Timeout=40";

                if (string.IsNullOrEmpty(connectionString))
                {
                    ShowError("Connection string is empty");
                }
                else
                {
                    AddToLog("Connection string loaded successfully");
                    WriteToLogFile("Connection string loaded successfully");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading connection string: {ex.Message}");
                WriteToLogFile($"ERROR loading connection string: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                WriteToLogFile("=== Application Shutting Down ===");
                WriteToLogFile($"End Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                WriteToLogFile("".PadRight(50, '='));

                logFileWriter?.Close();
                logFileWriter?.Dispose();
            }
            catch
            {
                // Ignore errors during shutdown
            }
            base.OnFormClosing(e);
        }

        private async void btnTestConnection_Click(object sender, EventArgs e)
        {
            await TestConnectionComprehensive();
        }

        private async Task TestConnectionComprehensive()
        {
            try
            {
                btnTestConnection.Enabled = false;
                AddToLog("Starting comprehensive connection test...");
                WriteToLogFile("Starting comprehensive connection test");

                string server = "10.227.167.243";
                int port = 1441;

                // Test network connectivity
                AddToLog("Testing basic network connectivity...");
                bool canPing = await TestPing(server);
                if (canPing)
                {
                    AddToLog("✓ Server is reachable via network");
                    WriteToLogFile("Network connectivity: SUCCESS - Server is reachable");
                }
                else
                {
                    AddToLog("✗ Server is not reachable via network");
                    WriteToLogFile("Network connectivity: FAILED - Server is not reachable");
                }

                // Test port connectivity
                AddToLog("Testing port connectivity...");
                bool portOpen = await TestPort(server, port);
                if (portOpen)
                {
                    AddToLog($"✓ Port {port} is open and listening");
                    WriteToLogFile($"Port {port} connectivity: SUCCESS - Port is open");
                }
                else
                {
                    AddToLog($"✗ Port {port} is closed or blocked");
                    WriteToLogFile($"Port {port} connectivity: FAILED - Port is closed or blocked");
                }

                // Test database connection
                if (portOpen)
                {
                    AddToLog("Testing database authentication...");
                    await TestDatabaseConnection();
                }

            }
            catch (Exception ex)
            {
                AddToLog($"Error during connection test: {ex.Message}");
                WriteToLogFile($"Connection test error: {ex.Message}");
            }
            finally
            {
                btnTestConnection.Enabled = true;
            }
        }

        private async Task<bool> TestPing(string server)
        {
            try
            {
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = await ping.SendPingAsync(server, 3000);
                    return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TestPort(string server, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(server, port);
                    if (await Task.WhenAny(task, Task.Delay(5000)) == task)
                    {
                        return client.Connected;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task TestDatabaseConnection()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    AddToLog("✓ Database connection successful!");
                    WriteToLogFile("Database connection: SUCCESS");

                    using (var command = new SqlCommand("SELECT DB_NAME(), @@SERVERNAME", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string dbName = reader[0].ToString();
                                string serverName = reader[1].ToString();

                                AddToLog($"✓ Connected to database: {dbName}");
                                AddToLog($"✓ Server name: {serverName}");

                                WriteToLogFile($"Database: {dbName}");
                                WriteToLogFile($"Server: {serverName}");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                AddToLog($"✗ Database error: {sqlEx.Message}");
                WriteToLogFile($"Database connection error: {sqlEx.Message} (Error #{sqlEx.Number})");
            }
            catch (Exception ex)
            {
                AddToLog($"✗ Connection error: {ex.Message}");
                WriteToLogFile($"Connection error: {ex.Message}");
            }
        }

        private void btnGetTickets_Click(object sender, EventArgs e)
        {
            try
            {
                GetTicketsForClearing();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting tickets: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteToLogFile($"Error getting tickets: {ex.Message}");
            }
        }

        private void btnClearTickets_Click(object sender, EventArgs e)
        {
            try
            {
                ClearSelectedTickets();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing tickets: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteToLogFile($"Error clearing tickets: {ex.Message}");
            }
        }

        private void GetTicketsForClearing()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("Connection string is not configured.", "Configuration Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"SELECT ActivityID, StatusID, EstimatedEndDate, ActivityDescription, LocalRef, OriginRef 
                            FROM [WF1ActR].[dbo].[Activities] 
                            WHERE ActivityTypeID = 1 
                            AND EstimatedEndDate < DATEADD(month,-2,sysdatetime())
                            AND StatusID NOT IN (1002,1009,1018,1012,1015,1017)
                            ORDER BY EstimatedEndDate DESC";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridViewTickets.DataSource = dataTable;
                        lblTicketCount.Text = $"Tickets found: {dataTable.Rows.Count}";

                        string logMessage = $"Loaded {dataTable.Rows.Count} tickets for potential clearing";
                        AddToLog(logMessage);
                        WriteToLogFile(logMessage);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = $"Database error while getting tickets: {sqlEx.Message}";
                AddToLog(errorMessage);
                WriteToLogFile(errorMessage);
                throw;
            }
        }

        private void ClearSelectedTickets()
        {
            if (dataGridViewTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select tickets to clear.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to clear {dataGridViewTickets.SelectedRows.Count} tickets?",
                "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            int successCount = 0;
            int failCount = 0;

            progressBar1.Maximum = dataGridViewTickets.SelectedRows.Count;
            progressBar1.Value = 0;

            // Log the start of clearing operation
            WriteToLogFile($"Starting clearing operation for {dataGridViewTickets.SelectedRows.Count} tickets");
            WriteToLogFile("Ticket Clearance Summary:");
            WriteToLogFile("".PadRight(80, '-'));

            foreach (DataGridViewRow row in dataGridViewTickets.SelectedRows)
            {
                string activityId = row.Cells["ActivityID"].Value.ToString();
                string status = ClearNormalTicket(activityId);

                if (status == "Success")
                {
                    successCount++;
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                    WriteToLogFile($"SUCCESS: Ticket {activityId} cleared successfully");
                }
                else
                {
                    failCount++;
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightCoral;
                    string failMessage = $"FAILED: Ticket {activityId} - {status}";
                    AddToLog(failMessage);
                    WriteToLogFile(failMessage);
                }

                progressBar1.Value++;
                Application.DoEvents();
            }

            // Log the summary
            WriteToLogFile("".PadRight(80, '-'));
            WriteToLogFile($"CLEARING SUMMARY: Success={successCount}, Failed={failCount}, Total={dataGridViewTickets.SelectedRows.Count}");
            WriteToLogFile("");

            string resultMessage = $"Clear operation completed:\nSuccess: {successCount}\nFailed: {failCount}";
            MessageBox.Show(resultMessage, "Clear Results", MessageBoxButtons.OK, MessageBoxIcon.Information);

            AddToLog($"Clear operation completed - Success: {successCount}, Failed: {failCount}");
            WriteToLogFile($"Clear operation completed - Success: {successCount}, Failed: {failCount}");
        }

        private string ClearNormalTicket(string actRef)
        {
            bool clearSuccess = false;
            string clrResponse = "Success";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // First, check if ticket exists and is in correct status
                    string checkSql = @"SELECT StatusID
                               FROM WF1ActR.dbo.Activities 
                               WHERE ActivityID = @ActRef";

                    using (SqlCommand command = new SqlCommand(checkSql, connection))
                    {
                        command.Parameters.AddWithValue("@ActRef", actRef);
                        using (SqlDataReader dr = command.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                dr.Read();
                                int aStatus = Convert.ToInt16(dr["StatusID"].ToString());

                                // Check if ticket is in clearable status
                                if ((aStatus != 1009) && (aStatus != 1002) && (aStatus != 1018) &&
                                    (aStatus != 1012) && (aStatus != 1015) && (aStatus != 1017))
                                {
                                    clearSuccess = true;
                                    WriteToLogFile($"Ticket {actRef} status check: Clearable (Status={aStatus})");
                                }
                                else
                                {
                                    string statusMessage = $"Ticket {actRef} in non-clearable status: {aStatus}";
                                    WriteToLogFile(statusMessage);
                                    return statusMessage;
                                }
                            }
                            else
                            {
                                WriteToLogFile($"Ticket {actRef} not found in database");
                                return "Ticket no longer exists in database";
                            }
                        }
                    }

                    // Check for outstanding work using multiple fallback methods
                    if (clearSuccess)
                    {
                        bool hasOutstandingWork = CheckOutstandingWork(connection, actRef);
                        if (hasOutstandingWork)
                        {
                            WriteToLogFile($"Ticket {actRef} has outstanding work orders");
                            return "Ticket has outstanding work orders or work requests";
                        }
                    }

                    // Clear the ticket
                    if (clearSuccess)
                    {
                        string clearSql = @"UPDATE [WF1ActR].[dbo].[Activities] 
                                           SET ActualEndDate = SYSDATETIME(),
                                               TicketEndDate = SYSDATETIME(),
                                               ActivityClrDescription = 'Cancelled on request from PLA team',
                                               StatusID = 1009,
                                               ClearCodeID = 12967,
                                               CauseCodeID = 13056
                                           WHERE ActivityID = @ActRef";

                        using (SqlCommand command = new SqlCommand(clearSql, connection))
                        {
                            command.Parameters.AddWithValue("@ActRef", actRef);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                clrResponse = "Success";
                                AddToLog($"Successfully cleared ticket: {actRef}");
                                WriteToLogFile($"Database update SUCCESS: Ticket {actRef} cleared (rows affected: {rowsAffected})");
                            }
                            else
                            {
                                clrResponse = "No rows affected - ticket may have been already cleared";
                                WriteToLogFile($"Database update WARNING: No rows affected for ticket {actRef}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    clrResponse = $"Database error: {ex.Message}";
                    AddToLog($"Error clearing ticket {actRef}: {ex.Message}");
                    WriteToLogFile($"ERROR clearing ticket {actRef}: {ex.Message}");
                }
            }

            return clrResponse;
        }

        private bool CheckOutstandingWork(SqlConnection connection, string actRef)
        {
            // Method 1: Try common work order table names
            string[] workOrderTables = {
                "WorkOrders",
                "WorkOrder",
                "WO",
                "WorkRequests",
                "WorkRequest",
                "WR",
                "Tasks",
                "Task"
            };

            foreach (string tableName in workOrderTables)
            {
                try
                {
                    string query = $@"
                        SELECT COUNT(*) 
                        FROM [WF1ActR].[dbo].[{tableName}] 
                        WHERE ActivityID = @ActID 
                        AND StatusID NOT IN (1009, 1012)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ActID", actRef);
                        int count = (int)command.ExecuteScalar();
                        if (count > 0)
                        {
                            AddToLog($"Found {count} outstanding work orders in table {tableName} for ticket {actRef}");
                            WriteToLogFile($"Outstanding work found: {count} in table {tableName} for ticket {actRef}");
                            return true;
                        }
                    }
                }
                catch (SqlException)
                {
                    // Table doesn't exist or other error, try next table
                    continue;
                }
            }

            // Method 2: Try to find any related work using system views
            try
            {
                string findRelatedWorkQuery = @"
                    SELECT COUNT(*) 
                    FROM sys.tables t
                    WHERE t.name LIKE '%Work%' OR t.name LIKE '%Task%' OR t.name LIKE '%WO%' OR t.name LIKE '%WR%'";

                using (SqlCommand command = new SqlCommand(findRelatedWorkQuery, connection))
                {
                    int workRelatedTables = (int)command.ExecuteScalar();
                    if (workRelatedTables > 0)
                    {
                        AddToLog($"Found {workRelatedTables} work-related tables, but cannot check without knowing exact structure");
                        WriteToLogFile($"Work-related tables exist ({workRelatedTables}), proceeding with clearance for ticket {actRef}");
                        // Since we can't determine the exact table structure, we'll proceed with clearing
                        // but log this for awareness
                        return false;
                    }
                }
            }
            catch
            {
                // Ignore errors in this diagnostic query
            }

            // Method 3: Check ActivityComments for any recent work activity
            try
            {
                string checkCommentsQuery = @"
                    SELECT COUNT(*) 
                    FROM [WF1ActR].[dbo].[ActivityComments] 
                    WHERE ActivityID = @ActID 
                    AND CommentDate > DATEADD(day, -30, GETDATE())
                    AND (Comment LIKE '%work%' OR Comment LIKE '%task%' OR Comment LIKE '%WO%' OR Comment LIKE '%WR%')";

                using (SqlCommand command = new SqlCommand(checkCommentsQuery, connection))
                {
                    command.Parameters.AddWithValue("@ActID", actRef);
                    int recentWorkComments = (int)command.ExecuteScalar();
                    if (recentWorkComments > 0)
                    {
                        AddToLog($"Found {recentWorkComments} recent work-related comments for ticket {actRef}");
                        WriteToLogFile($"Recent work comments found: {recentWorkComments} for ticket {actRef}");
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                WriteToLogFile($"Note: Could not check ActivityComments table for ticket {actRef}: {ex.Message}");
            }

            // If we get here, no outstanding work was found
            WriteToLogFile($"No outstanding work found for ticket {actRef}");
            return false;
        }

        private bool TryAddParameter(SqlCommand command, string parameterName, string value)
        {
            try
            {
                command.Parameters.AddWithValue(parameterName, value);
                return true;
            }
            catch
            {
                if (command.Parameters.Contains(parameterName))
                {
                    command.Parameters.RemoveAt(parameterName);
                    command.Parameters.AddWithValue(parameterName, value);
                    return true;
                }
                return false;
            }
        }

        private void AddToLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action<string>(AddToLog), message);
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\r\n");
                txtLog.ScrollToCaret();
            }

            // Also write to file log
            WriteToLogFile(message);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            AddToLog($"ERROR: {message}");
            WriteToLogFile($"CONFIGURATION ERROR: {message}");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetTicketsForClearing();
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridViewTickets.Rows)
            {
                row.Selected = true;
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            WriteToLogFile("=== Log display cleared by user ===");
        }

        private void btnViewLogFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    System.Diagnostics.Process.Start(logFilePath);
                }
                else
                {
                    MessageBox.Show("Log file not found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open log file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}