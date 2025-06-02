using Microsoft.AspNetCore.Identity;

namespace Auto.Services
{
    public class UserService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserService(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task CreateRole(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        public async Task CreateUser(string roleName, string email, string password, bool emailconfirmed)
        {
            // Überprüfen, ob die Rolle existiert
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                // Wenn die Rolle nicht existiert, erstelle sie
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = emailconfirmed, };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }

        // Neue Funktion zum Löschen eines Benutzers
        public async Task<bool> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                return result.Succeeded;
            }
            return false; // Benutzer nicht gefunden
        }
        // Erweiterte Funktion zum Aktualisieren eines Benutzers
        public async Task<bool> UpdateUser(string email, string newEmail, string newUserName, string newRole, bool emailConfirmed)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Aktualisiere E-Mail und Benutzername
                if (!string.IsNullOrEmpty(newEmail))
                {
                    user.Email = newEmail;
                    user.UserName = newEmail;  // Benutzername auf die neue E-Mail setzen, falls gewünscht
                }

                if (!string.IsNullOrEmpty(newUserName))
                {
                    user.UserName = newUserName;  // Benutzername separat aktualisieren
                }

                // Aktualisiere den EmailConfirmed-Status
                user.EmailConfirmed = emailConfirmed;

                // Benutzer aktualisieren
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return false; // Fehler beim Aktualisieren des Benutzers
                }

                // Überprüfe die aktuellen Rollen des Benutzers
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Entferne Benutzer aus aktuellen Rollen
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                    {
                        return false; // Fehler beim Entfernen der aktuellen Rollen
                    }
                }

                // Füge den Benutzer zur neuen Rolle hinzu
                if (!string.IsNullOrEmpty(newRole))
                {
                    // Stelle sicher, dass die Rolle existiert
                    if (!await _roleManager.RoleExistsAsync(newRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(newRole));
                    }

                    var addToRoleResult = await _userManager.AddToRoleAsync(user, newRole);
                    if (!addToRoleResult.Succeeded)
                    {
                        return false; // Fehler beim Hinzufügen der neuen Rolle
                    }
                }

                return true; // Erfolgreich aktualisiert
            }

            return false; // Benutzer nicht gefunden
        }
    }


}
