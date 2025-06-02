namespace Auto.Models
{
    public class UserRoleViewModel
    {
        public string? UserId { get; set; }        // Neue Eigenschaft für UserId
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? AlteEmail { get; set; }
        public string? RoleName { get; set; }
        public string? passwort { get; set; }
        public bool EmailConfirmed { get; set; }  // Neue Eigenschaft für EmailConfirmed
    }
}
