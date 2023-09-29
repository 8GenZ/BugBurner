using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BugBurner.Models
{
    public class Project
    {
        private DateTime _created;
        private DateTime? _startDate;
        private DateTime? _endDate;

        //Primary Key
        public int Id { get; set; }

        //Foreign Key
        [Display(Name = "Company Name")]
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        [StringLength(5000, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Description { get; set; }


        public DateTime Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                if (value.HasValue)
                {
                    _startDate = value.Value.ToUniversalTime();
                }
                else
                {
                    _startDate = null;
                }
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (value.HasValue)
                {
                    _endDate = value.Value.ToUniversalTime();
                }
                else
                {
                    _endDate = null;
                }
            }
        }

        public int ProjectPriorityId { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
        public bool Archived { get; set; }

        //Navigational Properties
        public virtual Company? Company { get; set; }

        public virtual ProjectPriority? ProjectPriority { get; set; }

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
