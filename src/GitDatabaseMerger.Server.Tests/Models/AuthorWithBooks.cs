using System.Collections.Generic;

namespace GitDatabaseMerger.Server.Tests.Models
{
    public class AuthorWithBooks : EntityBase
    {
        public int AuthorId { get; set; }
        public string Name { get; set; }
        public List<BookWithAuthor> Books { get; set; }
    }
}
