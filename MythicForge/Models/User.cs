using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MythicForge.Models
{
    /// <summary>
    /// A registered customer. Passwords are stored as a salted PBKDF2 hash
    /// (see Services/PasswordHasher).
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string DisplayName { get; set; }

        public DateTime CreatedOn { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
