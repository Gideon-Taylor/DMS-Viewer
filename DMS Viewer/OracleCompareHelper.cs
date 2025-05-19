using DMSLib;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DMS_Viewer
{
    public class OracleHelper
    {
        public OracleCommand CreateSQLStatementForTable(DMSTable table, OracleConnection connection)
        {
            // Build the SQL string
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");

            // Collect non-key and key columns
            var keyColumns = table.Metadata.FieldMetadata
                            .Where(m => m.UseEditMask.HasFlag(UseEditFlags.KEY))
                            .Select(t => table.Columns
                                .First(c => c.Name == t.FieldName)).ToList();
            var nonKeyColumns = table.Metadata.FieldMetadata
                            .Where(m => !m.UseEditMask.HasFlag(UseEditFlags.KEY))
                            .Select(t => table.Columns
                                .First(c => c.Name == t.FieldName)).ToList();

            // Construct SELECT clause
            for (int i = 0; i < nonKeyColumns.Count; i++)
            {
                var column = nonKeyColumns[i];
                sql.AppendFormat("CASE WHEN {0} = :{0} THEN 1 ELSE 0 END AS {0}_MATCH", column.Name);
                if (i < nonKeyColumns.Count - 1)
                    sql.Append(", ");
            }

            // FROM clause
            sql.AppendFormat(" FROM {0} ", table.DBName);

            // WHERE clause
            sql.Append(" WHERE ");
            for (int i = 0; i < keyColumns.Count; i++)
            {
                var column = keyColumns[i];
                sql.AppendFormat("{0} = :{0}", column.Name);
                if (i < keyColumns.Count - 1)
                    sql.Append(" AND ");
            }
            Clipboard.SetText(sql.ToString());
            // Create OracleCommand
            OracleCommand cmd = new OracleCommand(sql.ToString(), connection);

            // Add parameters for non-key columns
            foreach (var column in nonKeyColumns)
            {
                cmd.Parameters.Add(GetOracleParameter(column));
            }

            // Add parameters for key columns
            foreach (var column in keyColumns)
            {
                cmd.Parameters.Add(GetOracleParameter(column));
            }

            return cmd;
        }

        private OracleParameter GetOracleParameter(DMSColumn column)
        {
            OracleParameter param = new OracleParameter
            {
                ParameterName = column.Name,
                OracleDbType = GetOracleDbType(column.Type)
            };
            return param;
        }

        private OracleDbType GetOracleDbType(string type)
        {
            switch (type.ToUpper())
            {
                case "CHAR":
                    return OracleDbType.Varchar2;
                case "LONG":
                    return OracleDbType.Clob;
                case "NUMBER":
                    return OracleDbType.Decimal;
                case "DATE":
                    return OracleDbType.Date;
                case "DATETIME":
                    return OracleDbType.TimeStamp;
                case "TIME":
                    return OracleDbType.Varchar2; // Handle as string if needed
                case "IMAGE":
                    return OracleDbType.Blob;
                default:
                    throw new Exception("Unknown data type: " + type);
            }
        }

        public Dictionary<string, bool> CheckRow(DMSTable table, OracleCommand cmd, DMSRow row)
        {
            // Set parameter values from the row
            foreach (OracleParameter param in cmd.Parameters)
            {
                string columnName = param.ParameterName;
                int columnIndex = table.Columns.FindIndex(p => p.Name == columnName);
                var value = row.GetValue(columnIndex);
                param.Value = value ?? DBNull.Value;
            }

            // Ensure connection is open
            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();

            // Execute the command
            using (OracleDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    // Collect match results
                    Dictionary<string, bool> result = new Dictionary<string, bool>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string matchColumnName = reader.GetName(i);
                        string columnName = matchColumnName.Replace("_MATCH", "");
                        int matchValue = reader.GetInt32(i);
                        result[columnName] = matchValue == 1;
                    }
                    return result;
                }
                else
                {
                    // No matching row found
                    return null;
                }
            }
        }
    }
}
