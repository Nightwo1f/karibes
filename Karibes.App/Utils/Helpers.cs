using System;
using System.Globalization;
using System.Linq;

namespace Karibes.App.Utils
{
    public static class Helpers
    {
        public static string FormatarMoeda(decimal valor)
        {
            return valor.ToString("C", new CultureInfo("pt-BR"));
        }

        public static string FormatarData(DateTime data)
        {
            return data.ToString("dd/MM/yyyy");
        }

        public static string FormatarDataHora(DateTime data)
        {
            return data.ToString("dd/MM/yyyy HH:mm");
        }

        public static string FormatarCPF(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return cpf;

            return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
        }

        public static string FormatarCNPJ(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return string.Empty;

            cnpj = new string(cnpj.Where(char.IsDigit).ToArray());

            if (cnpj.Length != 14)
                return cnpj;

            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }

        public static string FormatarTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return string.Empty;

            telefone = new string(telefone.Where(char.IsDigit).ToArray());

            if (telefone.Length == 10)
                return $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 4)}-{telefone.Substring(6, 4)}";
            
            if (telefone.Length == 11)
                return $"({telefone.Substring(0, 2)}) {telefone.Substring(2, 5)}-{telefone.Substring(7, 4)}";

            return telefone;
        }
    }
}

