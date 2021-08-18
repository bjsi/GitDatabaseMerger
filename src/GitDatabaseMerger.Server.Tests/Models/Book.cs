using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Models
{
    public class Book : SimpleBook
    {
        public Author Author { get; set; }
    }

    public class SimpleBook : BaseEntity
    {
        [Key]
        public int BookId { get; set; }

        public string Title { get; set; }
    }
}
