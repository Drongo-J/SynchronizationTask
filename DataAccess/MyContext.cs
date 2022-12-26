using SynchronizationTask.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronizationTask.DataAccess
{
    public class MyContext : DbContext
    {
        public MyContext()
            : base("Data Source=(localdb)\\ProjectModels;Initial Catalog=BankDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Card> Cards { get; set; }
    }
}
