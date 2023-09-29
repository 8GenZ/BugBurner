using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace BugBurner.Models
{
    public class TicketComment
    {
        private DateTime _created;
            
        //Primary Key
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Comment")]
        [StringLength(800, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Comment { get; set; }

        public DateTime Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }
        //start foreign keys
        public int TicketId { get; set; }

        [Required]
        public string? UserId { get; set; }
        //End foreign keys

        //Navigational Properties

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; }

    }
}
