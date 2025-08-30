using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestMaster.ViewModels
{
    public class AssignTestViewModel
    {
        public int TestId { get; set; }
        public string TestTitle { get; set; } = string.Empty;

        [Display(Name = "Giao cho")]
        public string AssignTo { get; set; } = "User"; // Giá trị mặc định

        [Display(Name = "Chọn nhân viên")]
        public int? SelectedUserId { get; set; }

        [Display(Name = "Chọn phòng ban")]
        public int? SelectedDepartmentId { get; set; }

        [Display(Name = "Chọn cấp độ")]
        public int? SelectedLevelId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn chót.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Hạn chót")]
        public DateTime? DueDate { get; set; }

        // Dữ liệu cho các ô dropdown
        public IEnumerable<SelectListItem> UsersList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> DepartmentsList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> LevelsList { get; set; } = new List<SelectListItem>();
    }
}
