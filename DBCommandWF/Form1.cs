using DBLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBCommandWF
{

    public partial class Form1 : Form
    {
        private DBCommand dbCommand = null;
        private LogSqlMessage logSqlMessage;
        public Form1()
        {
            InitializeComponent();
            logSqlMessage = handleLogSQL;
            dbCommand = new DBCommand(logSqlMessage);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ReloadDatabaseList();
        }
        private void handleLogSQL(string message)
        {
            Log(message, "sys");
        }
        private void btnReload_Click(object sender, EventArgs e)
        {
            ReloadDatabaseList();
        }
        private void ReloadDatabaseList()
        {
            checkedListDatabase.Items.Clear();
            var list = dbCommand.GetAllDatabaseNames();
            if (list is null)
            {
                MessageBox.Show("Không thể đọc dữ liệu!");
                return;
            }
            var filtered = list.Where(a => a.StartsWith(txtPrefix.Text)).ToList();
            foreach (var name in filtered)
            {
                checkedListDatabase.Items.Add(name);
            }
            for (int i = 0; i < checkedListDatabase.Items.Count; i++)
            {
                checkedListDatabase.SetItemChecked(i, true);
            }
        }
        private List<string> GetSelectedDatabase()
        {
            List<string> selectedList = new List<string>();
            for (int i = 0; i < checkedListDatabase.Items.Count; i++)
            {
                if (checkedListDatabase.GetItemChecked(i))
                {
                    selectedList.Add(checkedListDatabase.Items[i].ToString());
                }
            }

            return selectedList;
        }

        private void btnRunCommand_Click(object sender, EventArgs e)
        {
            var result = GetSelectedDatabase();
            if (result.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một database!");
                return;
            }
            if (txtCommand.Text == "")
            {
                MessageBox.Show("Query không được rỗng!");
                return;
            }
            foreach (var item in result)
            {
                var error = dbCommand.RunCommand(txtCommand.Text, item);
                if (error != null)
                {
                    Log(error, item, Color.Red);
                }
                else
                {
                    Log("Command run successfully", item, Color.Green);
                }
            }
        }
        private void Log(string message, string database)
        {
            Log(message, database, Color.Black);
        }
        private void Log(string message, string database, Color color)
        {
            txtResult.AppendText($"[{database} - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]:" + message + "\n", color);
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            ReloadDatabaseList();
            txtResult.Text = "";
        }
    }

}
