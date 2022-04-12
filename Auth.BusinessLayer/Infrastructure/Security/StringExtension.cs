namespace Auth.BusinessLayer.Security;

public static class StringExtension
{
    public static string Encryptor(this string str)
    {
        var mas = str.ToCharArray();
        for (var i = 3; i < mas.Length - 3; i++)
            mas[i] = '*';
        return string.Join("", mas);
    }
}