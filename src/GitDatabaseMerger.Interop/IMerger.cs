using System.Threading.Tasks;

namespace GitDatabaseMerger.Interop
{
    public interface IMerger
    {
        public Task<MergeResult> MergeAsync(string local, string remote, string ancestor);
    }
}
