using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Models
{
    public class SimpleBook : EntityBase
    {
        [Key]
        public int BookId { get; set; }

        public string Title { get; set; }
    }
}
