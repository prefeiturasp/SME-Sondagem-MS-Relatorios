using Microsoft.AspNetCore.WebUtilities;
using System.Collections;
using System.Reflection;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class ExtensionMethods
{
    public static List<T?> ObterConstantesPublicas<T>(this Type type)
    {
        return type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
            .Select(x => (T)x.GetRawConstantValue())
            .ToList();
    }

    public static MethodInfo? ObterMetodo(this Type type, string method)
    {
        var executar = type.GetMethod(method);

        if (executar != null)
            return executar;

        foreach (var itf in type.GetInterfaces())
        {
            executar = ObterMetodo(itf, method);

            if (executar != null)
                break;
        }

        return executar;
    }

    public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
    {
        dynamic awaitable = @this.Invoke(obj, parameters);
        await awaitable;
        return awaitable.GetAwaiter().GetResult();
    }

    public static string ObjetoParaQueryStringExtensions(this object obj, string baseUrl)
    {
        if (obj == null) return baseUrl;

        var queryParams = new Dictionary<string, string>();
        var propriedades = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in propriedades)
        {
            var valor = prop.GetValue(obj);

            if (valor == null) continue;

            if (valor is int intValor && intValor == 0) continue;

            if (valor is string strValor && string.IsNullOrWhiteSpace(strValor)) continue;

            if (valor is IEnumerable lista && !(valor is string))
            {
                foreach (var item in lista)
                {
                    // Nota: O QueryHelpers lida com chaves duplicadas se necessário, 
                    // mas para dicionários simples, concatenamos ou usamos abordagens específicas.
                    // Aqui, simplificamos para o valor único ou primeira ocorrência.
                }
                continue;
            }

            queryParams.Add(prop.Name, valor.ToString()!);
        }

        return QueryHelpers.AddQueryString(baseUrl, queryParams);
    }
}