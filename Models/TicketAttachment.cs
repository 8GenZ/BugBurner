using BugBurner.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugBurner.Models
{
    public class TicketAttachment
    {
        private DateTime _created;
        //Primary Key
        public int Id { get; set; }

        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Description { get; set; }

        public DateTime Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }

        //Start Foreign Keys
        public int TicketId { get; set; }

        [Required]
        public string? BTUserId { get; set; }
        //End Keys

        [NotMapped]
        [DisplayName("Select a file")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
        public IFormFile? FormFile { get; set; }
        public byte[]? FileData { get; set; }
        public string? FileType { get; set; }
        public string? FileName { get; set; }

        //Navigational Properties
        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? BTUser { get; set;}

    }
}
