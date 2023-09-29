using System.ComponentModel.DataAnnotations;

namespace BugBurner.Models
{
    public class TicketPriority
    {
        //Primary Key
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Name { get; set; }
    }
}
