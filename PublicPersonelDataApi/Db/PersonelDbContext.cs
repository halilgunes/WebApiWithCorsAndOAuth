using Microsoft.EntityFrameworkCore;
using PublicPersonelDataApi.Model;

namespace PublicPersonelDataApi.Db
{
    public class PersonelDbContext : DbContext
    {
        public PersonelDbContext(DbContextOptions<PersonelDbContext> options) : base(options) { 
        }
        public DbSet<Personel> Personels { get; set; }
    }
}
