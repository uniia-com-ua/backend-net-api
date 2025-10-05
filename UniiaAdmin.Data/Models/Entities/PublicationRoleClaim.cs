using Microsoft.AspNetCore.Identity;

namespace UniiaAdmin.Data.Models
{
    public class PublicationRoleClaim
    {
        public int PublicationId { get; set; }
        public Publication? Publication { get; set; }
        public int IdentityRoleClaimId { get; set; }
        public IdentityRoleClaim<string>? IdentityRoleClaim { get; set; }
    }
}
