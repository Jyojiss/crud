using System.Text.RegularExpressions;

namespace UserCrudApi.Helpers;

public static class PasswordPolicy
{
    public static List<string> Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("La contraseña es obligatoria.");
            return errors;
        }

        if (password.Length < 8)
            errors.Add("La contraseña debe tener al menos 8 caracteres.");

        if (!Regex.IsMatch(password, "[A-Z]"))
            errors.Add("La contraseña debe contener al menos una letra mayúscula.");

        if (!Regex.IsMatch(password, "[a-z]"))
            errors.Add("La contraseña debe contener al menos una letra minúscula.");

        if (!Regex.IsMatch(password, "[0-9]"))
            errors.Add("La contraseña debe contener al menos un número.");

        if (!Regex.IsMatch(password, @"[\W_]"))
            errors.Add("La contraseña debe contener al menos un carácter especial.");

        if (Regex.IsMatch(password, @"\s"))
            errors.Add("La contraseña no debe contener espacios.");

        return errors;
    }

    public static bool IsValid(string password)
    {
        return Validate(password).Count == 0;
    }
}
