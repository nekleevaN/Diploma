namespace TrustMarket.UserService.Domain.Entities;

public enum AuthProvider
{
    Email  = 1,
    Google = 2
}

public enum PublicNameMode
{
    FirstNameOnly          = 1,
    FirstNameAndInitial    = 2,
    FullName               = 3
}
