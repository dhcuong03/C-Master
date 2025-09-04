using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TestMaster.Models;

namespace TestMaster.ViewComponents
{
    public class LogoUrlViewComponent : ViewComponent
    {
        private readonly EmployeeAssessmentContext _context;

        public LogoUrlViewComponent(EmployeeAssessmentContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Truy vấn CSDL để tìm cấu hình logo, không theo dõi thay đổi để tăng hiệu suất
            var logoConfig = await _context.SystemConfigurations
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync(c => c.ConfigKey == "SYSTEM_LOGO_URL");

            // Nếu có giá trị trong CSDL và không rỗng, dùng nó.
            // Nếu không, dùng đường dẫn mặc định là '/img/logo.png'.
            string logoUrl = !string.IsNullOrEmpty(logoConfig?.ConfigValue)
                             ? logoConfig.ConfigValue
                             : "/img/logo.png"; // Đường dẫn logo mặc định

            // Trả về chuỗi URL đã được xử lý
            return Content(Url.Content(logoUrl));
        }
    }
}
