using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Biblioteka
{
    public class LibraryContext : DbContext
    {
        public DbSet<Song> Songs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=library.db");
    }

    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Album { get; set; }
        public string Location { get; set; }
        public string Source { get; set; }
    }
}
