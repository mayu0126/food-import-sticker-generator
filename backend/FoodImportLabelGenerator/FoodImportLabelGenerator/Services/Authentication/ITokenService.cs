using Microsoft.AspNetCore.Identity;

namespace FoodImportLabelGenerator.Services.Authentication;

public interface ITokenService
{
    public string CreateToken(IdentityUser user);
}