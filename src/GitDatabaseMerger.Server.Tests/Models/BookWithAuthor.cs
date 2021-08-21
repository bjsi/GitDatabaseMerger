using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Models
{
    public class BookWithAuthor : SimpleBook
    {
        public int AuthorId { get; set; }
        public AuthorWithBooks Author { get; set; }
    }
}
