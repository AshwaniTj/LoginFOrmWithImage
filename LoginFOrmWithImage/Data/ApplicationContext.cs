using LoginFOrmWithImage.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginFOrmWithImage.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options): base (options)
        {
                
        }
        public DbSet<User> Users { get; set; }
    }
}
