namespace Auth.BusinessLayer.Security;

public static class StringExtension
{
    public static string Encryptor(this string str)
    {
        var mas = str.Split();
        for (var i = 1; i < mas.Length - 1; i++)
            mas[i] = "*";
        return string.Join("", mas);
    }
}