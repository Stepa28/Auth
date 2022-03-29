using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BusinessLayer.Configurations;

public class AuthOptions
{
    public const string Issuer = "BgkBack";    // издатель токена
    public const string Audience = "FrontEnd"; // потребитель токена

    private const string _key = "BgkBackSuperSecretKye"; // ключ для шифрации

    public static SymmetricSecurityKey GetSymmetricSecurityKey()
    {
        return new(Encoding.UTF8.GetBytes(_key));
    }
}