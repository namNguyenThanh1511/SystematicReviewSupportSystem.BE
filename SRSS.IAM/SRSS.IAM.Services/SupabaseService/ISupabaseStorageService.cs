using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Supabase.Storage;

namespace SRSS.IAM.Services.SupabaseService
{
	public interface ISupabaseStorageService
	{
		Task<string> UploadArticlePdfAsync(IFormFile file, Guid projectId);
		Task<byte[]> DownloadFileAsync(string path);
	}
}