using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Interop
{
    public interface IMerger
    {
        public Task<MergeResult> Merge(string local, string remote, string ancestor);
    }
}
