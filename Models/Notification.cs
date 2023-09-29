using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace BugBurner.Models
{
    public class Notification
    {
        private DateTime _created;

        //Primary key
        public int Id { get; set; }

        //foreign key
        public int? ProjectId { get; set; }

        //foreign key
        public int? TicketId { get; set; }

        [Required]
        [Display(Name = "Title")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Title { get; set; }

        [Required]
        [Display(Name = "Message")]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? Message { get; set; }

        public DateTime Created
        {
            get { return _created; }
            set { _created = value.ToUniversalTime(); }
        }

        //Foreign Key
        [Required]
        public string? SenderId { get; set; }

        //Foreign Key 
        [Required]
        public string? RecipientId { get; set; }

        public int NotificationTypeId { get; set; }

        public bool HasBeenViewed { get; set; }

        //Navigational Properties
        public virtual NotificationType? NotificationType { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }


    }
}
