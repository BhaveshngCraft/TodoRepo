using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ToDo.Domain.Models
{
    public class RefreshToken
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); 

        public string? Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        [ForeignKey("AppUser")]
        public string UserId { get; set; } = string.Empty;

        public virtual AppUser AppUser { get; set; } = new AppUser();
    }

}
