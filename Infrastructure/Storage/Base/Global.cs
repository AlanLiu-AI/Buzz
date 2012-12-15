namespace Runner.Base
{
    public static class Global
    {
        public static string ConnectionString
        {
            get { return Db.Dao.DefaultConnectionString; }
            set { Db.Dao.DefaultConnectionString = value; }
        }

        public static Db.SqlType DbType
        {
            get { return Db.Dao.DefaultDbType; }
            set { Db.Dao.DefaultDbType = value; }
        }

    }
}
