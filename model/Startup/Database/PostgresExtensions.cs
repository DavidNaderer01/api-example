namespace Startup.Database;

public static class PostgresExtensions
{
    public static IResourceBuilder<PostgresServerResource> Postgres(
        this IDistributedApplicationBuilder builder,
        string username,
        string password
    )
    {
        var dbUser = builder.AddParameter(
            "PostgresUser", 
            username);

        var dbPassword = builder.AddParameter(
            "PostgresPassword", 
            secret: true, 
            value: password);

        return builder
            .AddPostgres("postgresql")
            .WithUserName(dbUser)
            .WithPassword(dbPassword);
    }
}