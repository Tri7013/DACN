using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Repository
{
    public class ChapterService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChapterService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        // Phương thức để đọc nội dung của tệp
        public async Task<string> GetChapterContentAsync(Chapter chapter)
        {
            if (chapter == null || string.IsNullOrEmpty(chapter.FilePath))
            {
                return "Không có nội dung để hiển thị.";
            }

            // Đường dẫn vật lý đến tệp
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, chapter.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                return "Tệp không tồn tại.";
            }

            // Đọc nội dung từ tệp
            return await System.IO.File.ReadAllTextAsync(filePath);
        }
    }
}
