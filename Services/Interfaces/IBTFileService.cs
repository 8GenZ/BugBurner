using BugBurner.Models.Enums;

namespace BugBurner.Services.Interfaces
{
	public interface IBTFileService
	{
		public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

		public string ConvertByteArrayToFile(byte[] fileData, string extension, DefaultImage imageType);

		public string GetFileIcon(string file);

		public string FormatFileSize(long bytes);
	}
}
