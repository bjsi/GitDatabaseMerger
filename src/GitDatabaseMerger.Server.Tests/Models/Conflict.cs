using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Server.Tests.Models
{
    public class Conflict : EntityBase
    {
        [Key]
        public int ConflictId { get; set; }
    }
}
