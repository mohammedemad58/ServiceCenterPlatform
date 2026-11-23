using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CzechServiceCenter
{
    /// <summary>Small ADO.NET helper. Always pass user input as parameters.</summary>
    public static class Db
    {
        public static string ConnStr
        {
            get { return ConfigurationManager.ConnectionStrings["CSC"].ConnectionString; }
        }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static DataTable Table(string sql, params SqlParameter[] ps)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static DataRow Row(string sql, params SqlParameter[] ps)
        {
            var t = Table(sql, ps);
            return t.Rows.Count > 0 ? t.Rows[0] : null;
        }

        public static int Exec(string sql, params SqlParameter[] ps)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                cn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string sql, params SqlParameter[] ps)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                cn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static int Count(string sql, params SqlParameter[] ps)
        {
            object o = Scalar(sql, ps);
            return (o == null || o is DBNull) ? 0 : Convert.ToInt32(o);
        }
    }
}
