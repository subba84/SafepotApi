using Safepot.Entity;

namespace Safepot.Web.Api.Helpers
{
    public class SfpUserDto : SfpUser
    {
        public IEnumerable<SfpUserRoleMap>? UserRoles { get; set; }
        public string? Token { get; set; }
        public DateTime ExpirationToken { get; set; }
    }
}
