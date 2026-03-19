using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class EnumExtensao
{
    private static readonly ConcurrentDictionary<(Enum, Type), Attribute?> _attributeCache = new();

    public static TAttribute? GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
    {
        var key = (enumValue, typeof(TAttribute));
        var attribute = _attributeCache.GetOrAdd(key, (k) =>
        {
            var val = k.Item1;
            var attrType = k.Item2;
            var members = val.GetType().GetMember(val.ToString());
            return members.Length > 0 ? members[0].GetCustomAttribute(attrType) : null;
        });

        return attribute as TAttribute;
    }

    public static string? ShortName(this Enum enumValue) => enumValue.GetAttribute<DisplayAttribute>()?.ShortName;

    public static TEnum? GetEnumByShortName<TEnum>(string shortName) where TEnum : struct, Enum
    {
        foreach (var value in Enum.GetValues<TEnum>())
        {
            if (value.ShortName() == shortName)
                return value;
        }

        return null;
    }

    public static bool TryObterModalidadePorShortName(string shortName, out Modalidade modalidade)
    {
        var result = GetEnumByShortName<Modalidade>(shortName);
        modalidade = result ?? default;
        return result.HasValue;
    }
}
