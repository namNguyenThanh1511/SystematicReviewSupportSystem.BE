using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.SupabaseService
{
	public class SupabaseStorageService : ISupabaseStorageService
	{
		private readonly Supabase.Client _supabaseClient;
		private readonly string _bucketName = "research-papers";

		public SupabaseStorageService(IConfiguration configuration)
		{
			var url = configuration["SupabaseSettings:Url"] ?? throw new InvalidOperationException("SupabaseSettings:Url is required");
			var key = configuration["SupabaseSettings:Key"] ?? throw new InvalidOperationException("SupabaseSettings:Key is required");


			// Khởi tạo Supabase Client
			var options = new Supabase.SupabaseOptions { AutoConnectRealtime = false };
			_supabaseClient = new Supabase.Client(url, key, options);
		}

		public async Task<string> UploadArticlePdfAsync(IFormFile file, Guid projectId, Guid processId)
		{
			if (file == null || file.Length == 0)
				throw new ArgumentException("File không hợp lệ");

			var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
			if (fileExtension != ".pdf")
				throw new ArgumentException("Chỉ chấp nhận file PDF");

			if (file.Length > 20 * 1024 * 1024) // 20MB
				throw new ArgumentException("Kích thước file vượt quá 20MB");

			try
			{
				// Khởi tạo kết nối client (bắt buộc trước khi gọi các API)
				await _supabaseClient.InitializeAsync();

				// 1. Làm sạch tên file: Xóa ký tự đặc biệt, dấu ~ và khoảng trắng
                string rawFileName = Path.GetFileName(file.FileName);
                string cleanFileName = Regex.Replace(rawFileName, @"[^a-zA-Z0-9\._-]", "_");

				// Tạo đường dẫn file lưu trên Supabase (ví dụ: papers/project-id/process-id/ten-file-random.pdf)
				var fileName = $"{Guid.NewGuid()}_{cleanFileName}";
				var storagePath = $"papers/{projectId}/{processId}/{fileName}";

				// Đọc IFormFile thành mảng byte để upload lên Supabase
				using var memoryStream = new MemoryStream();
				await file.CopyToAsync(memoryStream);
				var fileBytes = memoryStream.ToArray();

				// Cấu hình định dạng file - Sử dụng Supabase.Storage.FileOptions
				var fileOptions = new Supabase.Storage.FileOptions
				{
					ContentType = "application/pdf",
					Upsert = false // Không ghi đè nếu trùng tên (đã xử lý bằng GUID ở trên)
				};

				// Thực hiện Upload
				var storage = _supabaseClient.Storage.From(_bucketName);
				await storage.Upload(fileBytes, storagePath, fileOptions);

				// Lấy Public URL để lưu vào Database
				var publicUrl = storage.GetPublicUrl(storagePath);

				return publicUrl;
			}
			catch (Exception ex)
			{
				throw new Exception($"Lỗi khi upload file lên Supabase: {ex.Message}", ex);
			}
		}
	}
}