using System.Net.Mail;
using System.Text.RegularExpressions;

namespace RotaDoSabor.Server.Validations;

public static class RuleValidation
{
    public static bool EmailValido(string email) => MailAddress.TryCreate(email, out _);

    public static bool TelefoneValido(string telefone)
    {
        var numeros = Regex.Replace(telefone ?? string.Empty, "[^0-9]", string.Empty);
        return numeros.Length is 10 or 11;
    }

    public static bool CnpjValido(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj)) return false;
        cnpj = Regex.Replace(cnpj, "[^0-9]", string.Empty);
        if (cnpj.Length != 14 || new string(cnpj[0], 14) == cnpj) return false;

        int[] p1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] p2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var baseCnpj = cnpj[..12];
        var soma = baseCnpj.Select((c, i) => (c - '0') * p1[i]).Sum();
        var d1 = soma % 11 < 2 ? 0 : 11 - soma % 11;
        var base2 = baseCnpj + d1;
        soma = base2.Select((c, i) => (c - '0') * p2[i]).Sum();
        var d2 = soma % 11 < 2 ? 0 : 11 - soma % 11;
        return cnpj.EndsWith($"{d1}{d2}");
    }
}
