using System.ComponentModel.DataAnnotations;

namespace school_diary.Models
{
    public class ErrorViewModel
    {
        [StringLength(100)]
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}