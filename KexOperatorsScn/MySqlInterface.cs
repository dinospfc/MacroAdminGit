using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Linq.Expressions;


namespace ListBoxTest
{
    public class MySqlInterface
    {
        /* Common variables */
        MySqlConnection mySqlConnection;
        MySqlDataAdapter mySqlDataAdapter;
        MySqlCommandBuilder mySqlCommandBuilder;
        DataTable dataTable;
        BindingSource bindingSource;

        public bool isConnected = false;

        /* Public Functions */
        public bool Connect(string server, string database, string username, string password)
        {
            // please note that the following code is vulnerable
            mySqlConnection = new MySqlConnection(
                                "SERVER=" + server + ";" +
                                "DATABASE=" + database + ";" +
                                "UID=" + username + ";" +
                                "PASSWORD=" + password + ";"
                                );
            try
            {
                mySqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.GetBaseException().ToString(), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                isConnected = false;
                return false;//connection error
            }
            isConnected = true;
            return true;
        }

        public DataSet Query(String query_string)
        {
            DataSet dataSet = null;

            Console.WriteLine(query_string);

            try
            {
                mySqlDataAdapter = new MySqlDataAdapter(query_string, mySqlConnection);
                mySqlCommandBuilder = new MySqlCommandBuilder(mySqlDataAdapter);
                dataSet = new DataSet();
                mySqlDataAdapter.Fill(dataSet);

            }
            catch (MySqlException ex)
            {
                MessageBox.Show("fillColumnNames\n" + ex.GetBaseException().ToString(), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataSet;
        }

        public void Query(String query_string, String columnName, DataGridView dataGridView, string tableName)
        {
            String queryCommand = "SELECT * FROM " + tableName
                                  + " WHERE " + columnName
                                  + " LIKE " + "\'%" + query_string + "%\'";
            try
            {
                mySqlDataAdapter = new MySqlDataAdapter(queryCommand, mySqlConnection);
                mySqlCommandBuilder = new MySqlCommandBuilder(mySqlDataAdapter);
                dataTable = new DataTable();
                mySqlDataAdapter.Fill(dataTable);

                dataGridView.DataSource = dataTable;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Query\n" + ex.GetBaseException().ToString(), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void runProcedure(DataGridView dataGridView,
                                 String procedureName,
                                 params Object[] arguments)
        {
            /* 'arguments' parameter contains pairs of
             * parameter name, parameter value
             * ordered sequentially
             */
            MySqlCommand cmd = new MySqlCommand();
            mySqlDataAdapter = new MySqlDataAdapter();
            dataTable = new DataTable();
            Object[] internalArgs = arguments;

            try
            {
                cmd = new MySqlCommand(procedureName, mySqlConnection);
                for (int i = 0; i < internalArgs.Length - 1; i += 2)
                {
                    cmd.Parameters.Add(new MySqlParameter((String)internalArgs[i], (object)internalArgs[i + 1]));
                }
                cmd.CommandType = CommandType.StoredProcedure;

                mySqlDataAdapter.SelectCommand = cmd;
                mySqlDataAdapter.Fill(dataTable);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("RunProcedure\n" + ex.GetBaseException().ToString(), "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Close()
        {
            mySqlConnection.Close();
            isConnected = false;
        }
    }
}
