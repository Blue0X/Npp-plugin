using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace NppScripts.DBHelper {
    class DBHelper {
        private string connectionString = string.Empty;

        public DBHelper(string dbPath) {
            this.connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbPath + ";Persist Security Info=False";
        }

        public DataTable Query(string query, Dictionary<string, object> parameters = null) {
            DataTable dataTable = new DataTable();
            using (OleDbConnection conn = new OleDbConnection(connectionString)) {
                using (OleDbCommand cmd = new OleDbCommand(query, conn)) {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null) {
                        foreach (KeyValuePair<string, object> parameter in parameters) {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd)) {
                        adapter.Fill(dataTable);
                    }
                }
                conn.Close();
            }
            return dataTable;
        }

        public int Execute(string query, Dictionary<string, object> parameters = null) {
            int value = 1;
            using (OleDbConnection conn = new OleDbConnection(connectionString)) {
                using (OleDbCommand cmd = new OleDbCommand(query, conn)) {
                    cmd.CommandType = CommandType.Text;
                    if (parameters != null) {
                        foreach (KeyValuePair<string, object> parameter in parameters) {
                            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                    }
                    cmd.Connection.Open();
                    value = cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            return value;
        }
     }
}