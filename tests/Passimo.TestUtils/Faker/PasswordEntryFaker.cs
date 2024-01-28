﻿using Bogus;
using Passimo.Domain.Model;
using Passimo.Cryptography.Encryption;
using System.Text;

namespace Passimo.TestUtils.Faker;

public class PasswordEntryFaker : Faker<PasswordEntry>
{
    private readonly DateTimeOffset _minDate = new DateTimeOffset(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly DateTimeOffset _maxDate = new DateTimeOffset(2024, 12, 31, 0, 0, 0, TimeSpan.Zero);

    public PasswordEntryFaker(string masterPassword, int? seed = null)
    {
        if (seed is not null) UseSeed(seed.Value);

        var encryption = EncryptionMethod.Aes256;

        RuleFor(e => e.Name, f => f.Company.CompanyName());
        RuleFor(e => e.Created, f => f.Date.BetweenOffset(_minDate, _maxDate));
        RuleFor(e => e.Updated, (f, e) => f.Date.BetweenOffset(e.Created, _maxDate));
        RuleFor(e => e.Fields, (f, e) =>
        {
            var url = $"https://{e.Name.ToLower().Replace(" ", "-").Replace(".", "-")}.com";
            var pw = f.Internet.Password();

            var pwBytes = Encoding.UTF8.GetBytes(pw);
            var encryptedPw = encryption.EncryptWithPassword(pwBytes, masterPassword);

            var fields = new List<PasswordEntryField>
            {
                new PasswordEntryInfoField { Name = "PasswordContainer_Description", NameLocalized = true, Type = PasswordEntryFiedType.LongText, Value = $"Password for {e.Name}" },
                new PasswordEntryInfoField { Name = "PasswordContainer_Username", NameLocalized = true, Type = PasswordEntryFiedType.Text, Value = "miriam.muster" },
                new PasswordEntryInfoField { Name = "PasswordContainer_Email", NameLocalized = true, Type = PasswordEntryFiedType.Text, Value = "miriam.muster@mail.com" },
                new PasswordEntryInfoField { Name = "PasswordContainer_Url", NameLocalized = true, Type = PasswordEntryFiedType.Text, Value = url },
                new PasswordEntryCryptographicField { Name = "PasswordContainer_Password", NameLocalized = true, Type = PasswordEntryFiedType.Password, EncryptedPassword = encryptedPw }
            };

            return fields;
        });
    }
}