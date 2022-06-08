using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MonobankExporter.Client.Models.Consts
{
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum CashbackTypes
    {
        [EnumMember(Value = "")]
        None = 0,

        [EnumMember(Value = "UAH")]
        UAH = 1,

        [EnumMember(Value = "Miles")]
        Miles = 2
    }
}
