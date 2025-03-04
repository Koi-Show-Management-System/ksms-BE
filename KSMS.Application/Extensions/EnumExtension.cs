using System.ComponentModel;
using System.Reflection;

namespace KSMS.Application.Extensions;

public static class EnumExtension
{
    public static string GetDescription(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));

        return attribute != null ? attribute.Description : value.ToString();
    }
}