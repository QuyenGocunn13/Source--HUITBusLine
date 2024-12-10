using QLBVXK.Models;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace QLBVXK.Areas.AdminHome.Controllers
{
    public class BackupController : Controller
    {
        QLBanVeXeKhachEntities db = new QLBanVeXeKhachEntities();

        // GET: AdminHome/Backup
        public ActionResult Index()
        {
            return View();
        }

        // GET: Danh sách các tệp sao lưu
        public ActionResult DSBackup()
        {
            string backupDirectory = @"E:\Backup_QLBanVeXeKhach\";
            var backupFiles = Directory.GetFiles(backupDirectory, "*.bak")
                                        .Select(f => Path.GetFileName(f))
                                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                                        .ToList();

            return View(backupFiles);
        }

        public ActionResult BackupDatabase(string backupType)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            Random random = new Random();
            int randomCode = random.Next(100, 999);

            string backupFile = $"E:\\Backup_QLBanVeXeKhach\\QLBanVeXeKhach_{timestamp}_{backupType}_{randomCode}.bak";
            string backupQuery = "";

            try
            {
                if (backupType == "Differential")
                {
                    backupQuery = $"BACKUP DATABASE [QLBanVeXeKhach] TO DISK = '{backupFile}' WITH DIFFERENTIAL;";
                }
                else
                {
                    backupQuery = $"BACKUP DATABASE [QLBanVeXeKhach] TO DISK = '{backupFile}' WITH INIT;";
                }

                db.Database.ExecuteSqlCommand(System.Data.Entity.TransactionalBehavior.DoNotEnsureTransaction, backupQuery);

                TempData["SuccessMessage"] = $"Sao lưu cơ sở dữ liệu thành công! Tệp sao lưu: {backupFile}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi sao lưu cơ sở dữ liệu: {ex.Message}";
            }

            return RedirectToAction("DSBackup");
        }

        private void ExecuteSqlCommand(SqlConnection connection, string commandText)
        {
            using (SqlCommand command = new SqlCommand(commandText, connection))
            {
                command.CommandTimeout = 600;
                command.ExecuteNonQuery();
            }
        }

        private void KillAllConnections(SqlConnection connection, string databaseName)
        {
            string killQuery = @"
                DECLARE @killCommand NVARCHAR(MAX) = '';
                SELECT @killCommand += 'KILL ' + CAST(session_id AS NVARCHAR(10)) + ';'
                FROM sys.dm_exec_sessions
                WHERE database_id = DB_ID(@databaseName);
                EXEC sp_executesql @killCommand;
            ";

            using (SqlCommand command = new SqlCommand(killQuery, connection))
            {
                command.Parameters.AddWithValue("@databaseName", databaseName);
                command.ExecuteNonQuery();
            }
        }

        public ActionResult RestoreDatabase(string backupFile)
        {
            string conn = "Server=GOCUNNPC;Database=master;Integrated Security=True;";
            string backupDirectory = @"E:\Backup_QLBanVeXeKhach\";
            string backupFilePath = Path.Combine(backupDirectory, backupFile);

            // Kiểm tra file sao lưu có tồn tại không
            if (!System.IO.File.Exists(backupFilePath))
            {
                TempData["ErrorMessage"] = "Tệp sao lưu không tồn tại. Vui lòng kiểm tra lại!";
                return RedirectToAction("DSBackup");
            }

            // Lấy loại file backup từ tên file (Full hoặc Differential)
            string[] fileNameParts = backupFile.Split('_');
            string backupType = fileNameParts.Length > 3 ? fileNameParts[3] : string.Empty;

            // Kiểm tra loại sao lưu có hợp lệ không
            if (string.IsNullOrEmpty(backupType) || (backupType != "Full" && backupType != "Differential"))
            {
                TempData["ErrorMessage"] = "Loại sao lưu không hợp lệ!";
                return RedirectToAction("DSBackup");
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();

                    // Đóng tất cả kết nối và chuyển về chế độ SINGLE_USER
                    KillAllConnections(connection, "QLBanVeXeKhach");
                    ExecuteSqlCommand(connection, "ALTER DATABASE [QLBanVeXeKhach] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;");

                    // Thực hiện khôi phục dựa trên loại backup
                    if (backupType == "Full")
                    {
                        // Khôi phục bản sao lưu Full với WITH MOVE
                        string restoreQuery = $@"
                RESTORE DATABASE QLBanVeXeKhach
                FROM DISK = @backupFilePath
                WITH 
                    MOVE 'QLBanVeXeKhach_Data' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\QLBanVeXeKhach.mdf',
                    MOVE 'QLBanVeXeKhach_Log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\QLBanVeXeKhach.ldf',
                    REPLACE;
                ";
                        SqlCommand command = new SqlCommand(restoreQuery, connection);
                        command.Parameters.AddWithValue("@backupFilePath", backupFilePath);
                        command.ExecuteNonQuery();
                    }
                    else if (backupType == "Differential")
                    {
                        // Lấy thời gian của bản Differential Backup
                        DateTime differentialTime = new FileInfo(backupFilePath).LastWriteTime;

                        // Tìm bản Full Backup gần nhất
                        var fullBackupFile = Directory.GetFiles(backupDirectory, "*.bak")
                                                      .Where(f => f.Contains("_Full_"))
                                                      .Select(f => new FileInfo(f))
                                                      .Where(f => f.LastWriteTime <= differentialTime) // Chỉ chọn file trước hoặc bằng thời gian bản Differential
                                                      .OrderByDescending(f => f.LastWriteTime) // Sắp xếp giảm dần theo thời gian
                                                      .FirstOrDefault();

                        if (fullBackupFile == null)
                        {
                            TempData["ErrorMessage"] = "Không tìm thấy bản sao lưu Full để khôi phục từ bản sao lưu khác biệt.";
                            return RedirectToAction("DSBackup");
                        }

                        // Khôi phục bản sao lưu Full trước (với NORECOVERY để giữ cơ sở dữ liệu ở trạng thái khôi phục)
                        string restoreFullQuery = $@"
                RESTORE DATABASE QLBanVeXeKhach
                FROM DISK = @fullBackupFilePath
                WITH 
                    NORECOVERY,  -- Giữ cơ sở dữ liệu ở trạng thái khôi phục để có thể khôi phục tiếp bản Differential
                    MOVE 'QLBanVeXeKhach_Data' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\QLBanVeXeKhach.mdf',
                    MOVE 'QLBanVeXeKhach_Log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\QLBanVeXeKhach.ldf';
                ";
                        SqlCommand fullCommand = new SqlCommand(restoreFullQuery, connection);
                        fullCommand.Parameters.AddWithValue("@fullBackupFilePath", fullBackupFile.FullName);
                        fullCommand.ExecuteNonQuery();

                        // Sau khi khôi phục Full, tiến hành khôi phục bản Differential
                        string restoreDifferentialQuery = $@"
                RESTORE DATABASE QLBanVeXeKhach
                FROM DISK = @differentialBackupFilePath
                WITH 
                    RECOVERY,  -- Hoàn tất khôi phục và chuyển cơ sở dữ liệu sang trạng thái sẵn sàng sử dụng
                    MOVE 'QLBanVeXeKhach_Data' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\QLBanVeXeKhach.mdf',
                    MOVE 'QLBanVeXeKhach_Log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\QLBanVeXeKhach.ldf';
                ";
                        SqlCommand diffCommand = new SqlCommand(restoreDifferentialQuery, connection);
                        diffCommand.Parameters.AddWithValue("@differentialBackupFilePath", backupFilePath);
                        diffCommand.ExecuteNonQuery();
                    }

                    // Chuyển cơ sở dữ liệu về chế độ MULTI_USER
                    ExecuteSqlCommand(connection, "ALTER DATABASE [QLBanVeXeKhach] SET MULTI_USER;");
                }

                TempData["SuccessMessage"] = $"Khôi phục cơ sở dữ liệu thành công từ tệp sao lưu: {backupFile}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi khôi phục cơ sở dữ liệu: {ex.Message}";
            }

            return RedirectToAction("DSBackup");
        }

        public ActionResult DeleteBackup(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                TempData["ErrorMessage"] = "Không có tệp sao lưu nào để xóa!";
                return RedirectToAction("DSBackup");
            }

            try
            {
                string backupDirectory = @"E:\Backup_QLBanVeXeKhach\";
                string filePath = Path.Combine(backupDirectory, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    TempData["SuccessMessage"] = $"Tệp sao lưu '{fileName}' đã được xóa thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Tệp '{fileName}' không tồn tại!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa tệp sao lưu: {ex.Message}";
            }

            return RedirectToAction("DSBackup");
        }
    }
}
