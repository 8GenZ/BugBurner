using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BugBurner.Models
{
    public class Invite
    {
        private DateTime _inviteDate;
        private DateTime? _joinDate;
        //Primary Key 
        public int Id { get; set; }

        public DateTime InviteDate
        {
            get { return _inviteDate; }
            set { _inviteDate = value.ToUniversalTime(); }
        }

        public DateTime? JoinDate
        {
            get { return _joinDate; }
            set
            {
                if (value.HasValue)
                {
                    _joinDate = value.Value.ToUniversalTime();
                }
                else
                {
                    _joinDate = null;
                }
            }
        }

        public Guid CompanyToken;

        //Foreign Key 
        public int CompanyId { get; set; }

        //Foreign Key 
        public int? ProjectId { get; set; }

        //Foreign Key 
        [Required]
        public string? InvitorId { get; set; }
        
        //Foreign Key
        public string? InviteeId { get; set; }

        [Required]
        [Display(Name = "Email")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? InviteeFirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} character long", MinimumLength = 2)]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }

        //NavigationalProperties
        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set;}

        public virtual BTUser? Invitor { get; set; }

        public virtual BTUser? Invitee { get; set; }
    }
}
