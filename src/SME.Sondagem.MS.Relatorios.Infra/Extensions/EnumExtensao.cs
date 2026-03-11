using SME.Sondagem.MS.Relatorios.Dominio.Enums;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SME.Sondagem.MS.Relatorios.Infra.Extensions;

public static class EnumExtensao
{
    public static TAttribute GetAttribute<TAttribute>(this Enum enumValue)
        where TAttribute : Attribute
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<TAttribute>();
    }

    public static string ShortName(this Enum enumValue)
           => enumValue.GetAttribute<DisplayAttribute>().ShortName;

    public static TEnum? GetEnumByShortName<TEnum>(string shortName) where TEnum : struct, Enum
    {
        var type = typeof(TEnum);

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = field.GetCustomAttribute<DisplayAttribute>();

            if (attribute?.ShortName == shortName)
                return (TEnum)field.GetValue(null);
        }

        return null;
    }

    public static bool TryObterModalidadePorShortName(string shortName, out Modalidade modalidade)
    {
        foreach (var field in typeof(Modalidade).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = field.GetCustomAttribute<DisplayAttribute>();

            if (attribute?.ShortName == shortName)
            {
                modalidade = (Modalidade)field.GetValue(null);
                return true;
            }
        }

        modalidade = default;
        return false;
    }
}
