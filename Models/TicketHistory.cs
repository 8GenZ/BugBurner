using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace BugBurner.Models
{
    public class TicketHistory
    {
        private DateTime _created;

        //Primary Key 
        public int Id { get; set; }

        //Foreign Key
        public int TicketId { get; set; }

        [Display(Name = "Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? PropertyName { get; set; }

        [Display(Name = "Description")]
        [StringLength(800, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Description { get; set; }

        public DateTime Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        [Required]
        public string? UserId { get; set; }

        //Navigational Properties
        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; }


    }
}
