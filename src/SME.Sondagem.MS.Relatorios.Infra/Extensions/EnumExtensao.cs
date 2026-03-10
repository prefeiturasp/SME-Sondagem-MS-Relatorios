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
}
