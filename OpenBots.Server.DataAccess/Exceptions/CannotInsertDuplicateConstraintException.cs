#nullable enable
using System;

namespace OpenBots.Server.DataAccess.Exceptions
{
    [Serializable]
    public class CannotInsertDuplicateConstraintException : Exception
    {
        public string? EntityName { get; internal set; }
        public string? PropertyName { get; internal set; }
        public string? Value { get; internal set; }

        public CannotInsertDuplicateConstraintException()
        {
        }

        public CannotInsertDuplicateConstraintException(Exception ex, string tableName, string constraintName, string valueName) : base("", ex)
        {
           
            EntityName = tableName;
            PropertyName = constraintName;
            Value = valueName;
        }
    }
}