﻿namespace RentIt.Users.Infrastructure.Options
{
    public class JwtOptions
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public double TokenLifetimeMinutes { get; set; }
        public double RefreshTokenLifetimeDays { get; set; }
    }
}
