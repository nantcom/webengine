namespace NC.WebEngine.Core.Membership
{

    public class RBACSettings
    {
        public string SiteId { get; set; }

        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string RoleAttribute { get; set; }

        /// <summary>
        /// If set, entry in the Role Claim array must include SiteId after underscore
        /// for example ["Admin", "Editor"] would not grant Admin or Editor Role
        /// but ["Admin_3c33258d-e8ee-477e-b94c-680fe0900f3f", "Editor"] will grant
        /// Admin role for Site that has site Id 3c33258d-e8ee-477e-b94c-680fe0900f3f set in RBAC settings
        /// </summary>
        public bool IsUseSiteSpecificRole { get; set; }
    }

}
