using System;
using System.Collections.Generic;

namespace GitDatabaseMerger.Server.Tests.Models
{
    public class Author : BaseEntity
    {
        public int AuthorId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public List<Book> Books { get; set; }
    }
}
