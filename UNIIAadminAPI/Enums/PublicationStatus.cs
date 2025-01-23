using System.ComponentModel;

namespace UNIIAadminAPI.Enums
{
    public enum PublicationStatus
    {
        [Description("innactive")]
        INACTIVE,

        [Description("draft")]
        DRAFT,

        [Description("members_only")]
        MEMBERS_ONLY,

        [Description("published")]
        PUBLISHED
    }
}
