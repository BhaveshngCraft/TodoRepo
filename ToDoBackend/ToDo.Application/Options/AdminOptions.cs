namespace ToDo.Application.Options
{
    public class AdminOptions
    {
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }
    }

    public class JwtOptions
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public string Key { get; set; }
        public int TokenValidityInMinutes { get; set; }
        public int RefreshTokenValidityInDays { get; set; }


    }
}
