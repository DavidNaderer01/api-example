using Microsoft.EntityFrameworkCore;

namespace Repositories.Base;

public abstract class BaseDbContext(DbContextOptions options) : DbContext(options);
