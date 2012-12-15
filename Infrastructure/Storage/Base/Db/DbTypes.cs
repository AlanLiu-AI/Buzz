using System.Data;

namespace Runner.Base.Db
{
    public enum SqlType
    {
        MsSql,
        Oracle,
        MsSqlCe
    }

    public enum DbCommonType
    {
        Common = 0,
        LongBinary = 1,
        LongChar = 2,
        Xml = 3,
        Boolean = 4,
        Guid = 5
    }

    struct ExtType
    {
        public DbType DBType;
        public DbCommonType Type;
    }
}