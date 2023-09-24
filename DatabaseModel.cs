using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastPaperViewer
{
    public class DatabaseModel
    {
        public class DataContext : DbContext
        {
            public DbSet<Bookmarks> Bookmarks { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseSqlite("Data Source=database.db");
        }

        public class Bookmarks
        {
            public string PaperName { get; set; }
            public int PageNumber { get; set; }
        }
    }
}
