using System;

namespace GitDatabaseMerger.Client.TestConsole.Helpers
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
