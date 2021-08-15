using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitDatabaseMerger.Client.Helpers
{
    public static class EnumEx
    {
        #region Methods

        public static string Name(this Enum e)
        {
            return Enum.GetName(e.GetType(), e);
        }

        #endregion
    }
}
