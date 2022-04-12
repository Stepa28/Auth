using System;
using System.Security.Cryptography;
using Auth.BusinessLayer.Security;
using NUnit.Framework;

namespace Auth.BusinessLayer.Test;

public class PasswordHashTests
{
    [TestCase("gaga4982")]
    [TestCase("")]
    [TestCase("     ")]
    public void HashPasswordTest(string passwordForTest)
    {
        //given
        string password = passwordForTest;

        //when
        string actual = PasswordHash.HashPassword(password);
        string expected = CalcHash(password);

        //then
        Assert.AreEqual(actual.Split(":")[0], expected.Split(":")[0]);
        Assert.AreEqual(actual.Split(":")[1].Length, expected.Split(":")[1].Length);
        Assert.AreEqual(actual.Split(":")[2].Length, expected.Split(":")[2].Length);
        Assert.AreEqual(Convert.FromBase64String(actual.Split(":")[1]).Length, PasswordHash.SaltByteSize);
        Assert.AreEqual(Convert.FromBase64String(actual.Split(":")[2]).Length, PasswordHash.HashByteSize);
    }

    [TestCase("password8749", true)]
    [TestCase("unvalid", false)]
    public void ValidatePasswordTest(string currentPassword, bool expected)
    {
        //given
        string validPassword = "password8749";

        //when
        bool actual = PasswordHash.ValidatePassword(validPassword, CalcHash(currentPassword));

        //then
        Assert.AreEqual(actual, expected);
    }

    internal static string CalcHash(string password)
    {
        var salt = new byte[PasswordHash.SaltByteSize];
        RandomNumberGenerator.Create().GetBytes(salt);
        var hash = new Rfc2898DeriveBytes(password, salt)
            {
                IterationCount = PasswordHash.Pbkdf2Iterations
            }
            .GetBytes(PasswordHash.HashByteSize);
        return $"{PasswordHash.Pbkdf2Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
}