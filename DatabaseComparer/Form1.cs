<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseComparer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string resultFileName()
        {
            string text = "";
            this.Invoke((MethodInvoker)delegate ()
            {
                text = comboBox1.Text;
            });
            return AppDomain.CurrentDomain.BaseDirectory + text + "_result.txt";
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        

        protected internal class ComparerSetings
        {
            public string sourceServer { get; set; }
            public string sourceAuthentication { get; set; }
            public string sourceUser { get; set; }
            public string sourcePassword { get; set; }
            public string sourceDb { get; set; }
            public string sourceSchema { get; set; }

            public string destServer { get; set; }
            public string destAuthentication { get; set; }
            public string destUser { get; set; }
            public string destPassword { get; set; }
            public string destDb { get; set; }
            public string destSchema { get; set; }

            public string tableData { get; set; }
            public string ignoreData { get; set; }
            public string fastComparison { get; set; }

            public string modifiedDate { get; set; }

        }
        ComparerSetings comp = new ComparerSetings();
        public void loadDataFromView()
        {
            comp = new ComparerSetings();

            comp.sourceServer = textBox1.Text;
            comp.sourceAuthentication = radioButton1.Checked ? "Windows" : "SQL";
            comp.sourceUser = textBox2.Text;
            comp.sourcePassword = textBox3.Text;
            comp.sourceDb = textBox4.Text;
            comp.sourceSchema = textBox5.Text.Length > 0 ? textBox5.Text : "dbo";

            comp.destServer = textBox6.Text;
            comp.destAuthentication = radioButton3.Checked ? "Windows" : "SQL";
            comp.destUser = textBox7.Text;
            comp.destPassword = textBox8.Text;
            comp.destDb = textBox9.Text;
            comp.destSchema = textBox10.Text.Length > 0 ? textBox10.Text : "dbo";

            comp.tableData = textBox12.Text.Trim();
            comp.ignoreData = textBox14.Text.Trim();
            comp.fastComparison = checkBox1.Checked ? "true" : "false";
            comp.modifiedDate = DateTime.Now.ToString();
        }

        public void loadDataToView()
        {
            textBox1.Text = comp.sourceServer;
            if(comp.sourceAuthentication == "Windows")
            {
                radioButton1.Checked = true;
            }else
            {
                radioButton2.Checked = true;
            }
            textBox2.Text = comp.sourceUser;
            textBox3.Text = comp.sourcePassword;
            textBox4.Text = comp.sourceDb;
            textBox5.Text = comp.sourceSchema;

            textBox6.Text = comp.destServer;
            if (comp.destAuthentication == "Windows")
            {
                radioButton3.Checked = true;
            }
            else
            {
                radioButton4.Checked = true;
            }
            textBox7.Text = comp.destUser;
            textBox8.Text = comp.destPassword;
            textBox9.Text = comp.destDb;
            textBox10.Text = comp.destSchema;

            textBox12.Text = comp.tableData;
            textBox14.Text = comp.ignoreData;
            checkBox1.Checked = (comp.fastComparison == "true");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadDataFromView();

            string sourceDbString = "Data Source = " + comp.sourceServer + "; Initial Catalog = " + comp.sourceDb + "; User ID = " + comp.sourceUser + "; Password = " + comp.sourcePassword;

           
            string destDbString = "Data Source = " + comp.destServer + "; Initial Catalog = " + comp.destDb + "; User ID = " + comp.destUser + "; Password = " + comp.destPassword;
            
            if (comp.tableData.Length == 0)
            {
                textBox13.Text = "Please enter table details in table name [space] search key (optional) order";
                MessageBox.Show(textBox13.Text, "Error");
                return;
            }
            textBox13.Text = "Comparison Started!";

            
            string[] tableStringer = comp.tableData.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            Dictionary<string, string> tableString = new Dictionary<string, string>();

            for (int i = 0; i < tableStringer.Length; i++)
            {
                string[] ta = tableStringer[i].Split(new[] { " " }, StringSplitOptions.None);

                if(ta[0].Length > 0)
                    tableString.Add(ta[0], ta.Length > 1 ? ta[1] : "");
            }

            List<string> ignoreList = new List<string>();
            
            if (comp.ignoreData.Length > 0)
            {
                ignoreList = comp.ignoreData.Split(
                    new[] { Environment.NewLine, " " , ","},
                    StringSplitOptions.None
                ).ToList();
            }

            Dictionary<String, List<string>> result = new Dictionary<string, List<string>>();
            
            SqlConnection sourceCon = new SqlConnection();
            SqlConnection destCon = new SqlConnection();
            try
            {
                sourceCon.ConnectionString = sourceDbString;
                sourceCon.Open();

               
                destCon.ConnectionString = destDbString;
                destCon.Open();

                int totalDiffRecords = 0;
                foreach (KeyValuePair<string, string> entry in tableString)
                {
                    List<string> res = new List<string>();
                    SqlCommand cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + entry.Key + "')", sourceCon);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable sdt = new DataTable();
                    da.Fill(sdt);

                    cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + entry.Key + "')", destCon);
                    da = new SqlDataAdapter(cmd);
                    DataTable ddt = new DataTable();
                    da.Fill(ddt);


                    string searchKey = entry.Value;
                    if (searchKey == null || searchKey.Length == 0)
                    {
                        string sql = "SELECT ColumnName = col.column_name FROM information_schema.table_constraints tc INNER JOIN information_schema.key_column_usage col"
                        + " ON col.Constraint_Name = tc.Constraint_Name"
                        + " AND col.Constraint_schema = tc.Constraint_schema"
                        + " WHERE tc.Constraint_Type = 'Primary Key' AND col.Table_name = '" + entry.Key + "'";
                        cmd = new SqlCommand(sql, sourceCon);
                        da = new SqlDataAdapter(cmd);
                        DataTable primaryt = new DataTable();
                        da.Fill(primaryt);
                        searchKey = primaryt.Rows[0][0].ToString();
                    }
                    //foreach (string str in ignoreList)
                    //{
                    //    foreach (DataRow row in sdt.Rows)
                    //    {
                    //        if (ignoreList.Contains(row[0].ToString()))
                    //        {
                    //            sdt.Rows.Remove(row);
                    //        }
                    //    }
                    //    foreach (DataRow row in ddt.Rows)
                    //    {
                    //        if (ignoreList.Contains(row[0].ToString()))
                    //        {
                    //            sdt.Rows.Remove(row);
                    //        }
                    //    }
                    //}
                    if (sdt.Rows.Count == ddt.Rows.Count)
                    {
                        cmd = new SqlCommand("SELECT * FROM " + entry.Key + "", sourceCon);
                        da = new SqlDataAdapter(cmd);
                        DataTable sdata = new DataTable();
                        da.Fill(sdata);

                        cmd = new SqlCommand("SELECT * FROM " + entry.Key + "", destCon);
                        da = new SqlDataAdapter(cmd);
                        DataTable ddata = new DataTable();
                        da.Fill(ddata);

                        
                        if (sdata.Rows.Count > 0 && ddata.Rows.Count > 0)
                        {
                            if (comp.fastComparison == "true")
                            {
                                if (sdata.Rows.Count == ddata.Rows.Count)
                                {
                                    res.Add("Tables have same number of rows."+Environment.NewLine);
                                }else
                                {
                                    res.Add("Tables have different number of rows." + Environment.NewLine);
                                    totalDiffRecords++;
                                }
                            }
                            else
                            {


                                foreach (DataRow sdr in sdata.Rows)
                                {
                                    foreach (DataRow ddr in ddata.Rows)
                                    {
                                        if (sdr[searchKey].ToString() == ddr[searchKey].ToString())
                                        {
                                            bool alltrue = true;
                                            foreach (DataRow row in sdt.Rows)
                                            {
                                                if (!ignoreList.Contains(row[0].ToString()))
                                                {
                                                    if (sdr[row[0].ToString()].ToString() != ddr[row[0].ToString()].ToString())
                                                    {
                                                        alltrue = false;
                                                        res.Add(sdr[searchKey] + ", " + row[0].ToString() + " => source :" + sdr[row[0].ToString()] + "\tdest :" + ddr[row[0].ToString()]);
                                                        //break;
                                                    }
                                                }
                                            }
                                            if (!alltrue)
                                            {
                                                totalDiffRecords++;
                                                res.Add("\n");
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    result.Add(entry.Key, res);
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resultFileName()))
                {
                    foreach (KeyValuePair<string, List<string>> entry in result)
                    {
                        file.WriteLine("Table : " + entry.Key);
                        file.WriteLine("============================");
                        foreach (string line in entry.Value)
                        {
                            file.WriteLine(line);
                        }
                    }
                }
                textBox13.Text = "Comparison Complete!!"+ Environment.NewLine + "There are "+ totalDiffRecords + " numbers of differences.";
                textBox11.Text = resultFileName();
                Thread thread = new Thread(new ParameterizedThreadStart(param =>
                {
                    DialogResult dialogResult = MessageBox.Show("Report file exported. Open file now?", "Report Export", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        startApp(resultFileName());
                    }
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                sourceCon.Close();
                destCon.Close();
            }
        }

        
        static void startApp(String fileName)
        {
            if (File.Exists(fileName))
            {
                System.Diagnostics.Process pptProcess = new System.Diagnostics.Process();
                pptProcess.StartInfo.FileName = fileName;
                pptProcess.StartInfo.UseShellExecute = true;
                pptProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                pptProcess.Start();
            }
            else
            {
                MessageBox.Show("Report file does not exist", "Error");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startApp(resultFileName());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadDbSettings();
            
        }

        string settingsDir = AppDomain.CurrentDomain.BaseDirectory + "\\settings\\";

        public void loadDbSettings()
        {
            try
            {
                if (Directory.Exists(settingsDir))
                {
                    DirectoryInfo d = new DirectoryInfo(settingsDir);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles("*.dbsettings"); //Getting Text files

                    Dictionary<string, string> test = Files.ToDictionary(f => f.Name, f => Path.GetFileNameWithoutExtension(f.Name));
                    if (test.Count > 0)
                    {
                        comboBox1.DataSource = new BindingSource(test, null);
                        comboBox1.DisplayMember = "Value";
                        comboBox1.ValueMember = "Key";
                    }
                    else
                    {
                        comboBox1.Text = "Defualt";
                    }
                }else
                {
                    Directory.CreateDirectory(settingsDir);
                    comboBox1.Text = "Defualt";
                }
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.StackTrace);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            loadDataFromView();
            string fileName = comboBox1.Text;
            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }
            File.WriteAllText(settingsDir+ fileName + ".dbsettings", comp.ToJson());
            loadDbSettings();
            comboBox1.Text = fileName;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(File.Exists(settingsDir+comboBox1.Text + ".dbsettings"))
            {
                File.Delete(settingsDir+comboBox1.Text + ".dbsettings");
                loadDbSettings();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(settingsDir + comboBox1.Text + ".dbsettings"))
            {
                comp = File.ReadAllText(settingsDir + comboBox1.Text + ".dbsettings").FromJson<ComparerSetings>();
                loadDataToView();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filePath = resultFileName();
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Report file does not exist","Error");
                return;
            }
            string argument = "/select,\"" + filePath + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseComparer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string resultFileName()
        {
            string text = "";
            this.Invoke((MethodInvoker)delegate ()
            {
                text = comboBox1.Text;
            });
            return AppDomain.CurrentDomain.BaseDirectory + text + "_result.txt";
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
        

        protected internal class ComparerSetings
        {
            public string sourceServer { get; set; }
            public string sourceAuthentication { get; set; }
            public string sourceUser { get; set; }
            public string sourcePassword { get; set; }
            public string sourceDb { get; set; }
            public string sourceSchema { get; set; }

            public string destServer { get; set; }
            public string destAuthentication { get; set; }
            public string destUser { get; set; }
            public string destPassword { get; set; }
            public string destDb { get; set; }
            public string destSchema { get; set; }

            public string tableData { get; set; }
            public string ignoreData { get; set; }
            public string fastComparison { get; set; }

            public string modifiedDate { get; set; }

        }
        ComparerSetings comp = new ComparerSetings();
        public void loadDataFromView()
        {
            comp = new ComparerSetings();

            comp.sourceServer = textBox1.Text;
            comp.sourceAuthentication = radioButton1.Checked ? "Windows" : "SQL";
            comp.sourceUser = textBox2.Text;
            comp.sourcePassword = textBox3.Text;
            comp.sourceDb = textBox4.Text;
            comp.sourceSchema = textBox5.Text.Length > 0 ? textBox5.Text : "dbo";

            comp.destServer = textBox6.Text;
            comp.destAuthentication = radioButton3.Checked ? "Windows" : "SQL";
            comp.destUser = textBox7.Text;
            comp.destPassword = textBox8.Text;
            comp.destDb = textBox9.Text;
            comp.destSchema = textBox10.Text.Length > 0 ? textBox10.Text : "dbo";

            comp.tableData = textBox12.Text.Trim();
            comp.ignoreData = textBox14.Text.Trim();
            comp.fastComparison = checkBox1.Checked ? "true" : "false";
            comp.modifiedDate = DateTime.Now.ToString();
        }

        public void loadDataToView()
        {
            textBox1.Text = comp.sourceServer;
            if(comp.sourceAuthentication == "Windows")
            {
                radioButton1.Checked = true;
            }else
            {
                radioButton2.Checked = true;
            }
            textBox2.Text = comp.sourceUser;
            textBox3.Text = comp.sourcePassword;
            textBox4.Text = comp.sourceDb;
            textBox5.Text = comp.sourceSchema;

            textBox6.Text = comp.destServer;
            if (comp.destAuthentication == "Windows")
            {
                radioButton3.Checked = true;
            }
            else
            {
                radioButton4.Checked = true;
            }
            textBox7.Text = comp.destUser;
            textBox8.Text = comp.destPassword;
            textBox9.Text = comp.destDb;
            textBox10.Text = comp.destSchema;

            textBox12.Text = comp.tableData;
            textBox14.Text = comp.ignoreData;
            checkBox1.Checked = (comp.fastComparison == "true");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            loadDataFromView();

            string sourceDbString = "Data Source = " + comp.sourceServer + "; Initial Catalog = " + comp.sourceDb + "; User ID = " + comp.sourceUser + "; Password = " + comp.sourcePassword;

           
            string destDbString = "Data Source = " + comp.destServer + "; Initial Catalog = " + comp.destDb + "; User ID = " + comp.destUser + "; Password = " + comp.destPassword;
            
            if (comp.tableData.Length == 0)
            {
                textBox13.Text = "Please enter table details in table name [space] search key (optional) order";
                MessageBox.Show(textBox13.Text, "Error");
                return;
            }
            textBox13.Text = "Comparison Started!";

            
            string[] tableStringer = comp.tableData.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            Dictionary<string, string> tableString = new Dictionary<string, string>();

            for (int i = 0; i < tableStringer.Length; i++)
            {
                string[] ta = tableStringer[i].Split(new[] { " " }, StringSplitOptions.None);

                if(ta[0].Length > 0)
                    tableString.Add(ta[0], ta.Length > 1 ? ta[1] : "");
            }

            List<string> ignoreList = new List<string>();
            
            if (comp.ignoreData.Length > 0)
            {
                ignoreList = comp.ignoreData.Split(
                    new[] { Environment.NewLine, " " , ","},
                    StringSplitOptions.None
                ).ToList();
            }

            Dictionary<String, List<string>> result = new Dictionary<string, List<string>>();
            
            SqlConnection sourceCon = new SqlConnection();
            SqlConnection destCon = new SqlConnection();
            try
            {
                sourceCon.ConnectionString = sourceDbString;
                sourceCon.Open();

               
                destCon.ConnectionString = destDbString;
                destCon.Open();

                int totalDiffRecords = 0;
                foreach (KeyValuePair<string, string> entry in tableString)
                {
                    List<string> res = new List<string>();
                    SqlCommand cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + entry.Key + "')", sourceCon);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable sdt = new DataTable();
                    da.Fill(sdt);

                    cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + entry.Key + "')", destCon);
                    da = new SqlDataAdapter(cmd);
                    DataTable ddt = new DataTable();
                    da.Fill(ddt);


                    string searchKey = entry.Value;
                    if (searchKey == null || searchKey.Length == 0)
                    {
                        string sql = "SELECT ColumnName = col.column_name FROM information_schema.table_constraints tc INNER JOIN information_schema.key_column_usage col"
                        + " ON col.Constraint_Name = tc.Constraint_Name"
                        + " AND col.Constraint_schema = tc.Constraint_schema"
                        + " WHERE tc.Constraint_Type = 'Primary Key' AND col.Table_name = '" + entry.Key + "'";
                        cmd = new SqlCommand(sql, sourceCon);
                        da = new SqlDataAdapter(cmd);
                        DataTable primaryt = new DataTable();
                        da.Fill(primaryt);
                        searchKey = primaryt.Rows[0][0].ToString();
                    }
                    //foreach (string str in ignoreList)
                    //{
                    //    foreach (DataRow row in sdt.Rows)
                    //    {
                    //        if (ignoreList.Contains(row[0].ToString()))
                    //        {
                    //            sdt.Rows.Remove(row);
                    //        }
                    //    }
                    //    foreach (DataRow row in ddt.Rows)
                    //    {
                    //        if (ignoreList.Contains(row[0].ToString()))
                    //        {
                    //            sdt.Rows.Remove(row);
                    //        }
                    //    }
                    //}
                    if (sdt.Rows.Count == ddt.Rows.Count)
                    {
                        cmd = new SqlCommand("SELECT * FROM " + entry.Key + "", sourceCon);
                        da = new SqlDataAdapter(cmd);
                        DataTable sdata = new DataTable();
                        da.Fill(sdata);

                        cmd = new SqlCommand("SELECT * FROM " + entry.Key + "", destCon);
                        da = new SqlDataAdapter(cmd);
                        DataTable ddata = new DataTable();
                        da.Fill(ddata);

                        
                        if (sdata.Rows.Count > 0 && ddata.Rows.Count > 0)
                        {
                            if (comp.fastComparison == "true")
                            {
                                if (sdata.Rows.Count == ddata.Rows.Count)
                                {
                                    res.Add("Tables have same number of rows."+Environment.NewLine);
                                }else
                                {
                                    res.Add("Tables have different number of rows." + Environment.NewLine);
                                    totalDiffRecords++;
                                }
                            }
                            else
                            {


                                foreach (DataRow sdr in sdata.Rows)
                                {
                                    foreach (DataRow ddr in ddata.Rows)
                                    {
                                        if (sdr[searchKey].ToString() == ddr[searchKey].ToString())
                                        {
                                            bool alltrue = true;
                                            foreach (DataRow row in sdt.Rows)
                                            {
                                                if (!ignoreList.Contains(row[0].ToString()))
                                                {
                                                    if (sdr[row[0].ToString()].ToString() != ddr[row[0].ToString()].ToString())
                                                    {
                                                        alltrue = false;
                                                        res.Add(sdr[searchKey] + ", " + row[0].ToString() + " => source :" + sdr[row[0].ToString()] + "\tdest :" + ddr[row[0].ToString()]);
                                                        //break;
                                                    }
                                                }
                                            }
                                            if (!alltrue)
                                            {
                                                totalDiffRecords++;
                                                res.Add("\n");
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    result.Add(entry.Key, res);
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(resultFileName()))
                {
                    foreach (KeyValuePair<string, List<string>> entry in result)
                    {
                        file.WriteLine("Table : " + entry.Key);
                        file.WriteLine("============================");
                        foreach (string line in entry.Value)
                        {
                            file.WriteLine(line);
                        }
                    }
                }
                textBox13.Text = "Comparison Complete!!"+ Environment.NewLine + "There are "+ totalDiffRecords + " numbers of differences.";
                textBox11.Text = resultFileName();
                Thread thread = new Thread(new ParameterizedThreadStart(param =>
                {
                    DialogResult dialogResult = MessageBox.Show("Report file exported. Open file now?", "Report Export", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        startApp(resultFileName());
                    }
                }));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                sourceCon.Close();
                destCon.Close();
            }
        }

        
        static void startApp(String fileName)
        {
            if (File.Exists(fileName))
            {
                System.Diagnostics.Process pptProcess = new System.Diagnostics.Process();
                pptProcess.StartInfo.FileName = fileName;
                pptProcess.StartInfo.UseShellExecute = true;
                pptProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Maximized;
                pptProcess.Start();
            }
            else
            {
                MessageBox.Show("Report file does not exist", "Error");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            startApp(resultFileName());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadDbSettings();
            
        }

        string settingsDir = AppDomain.CurrentDomain.BaseDirectory + "\\settings\\";

        public void loadDbSettings()
        {
            try
            {
                if (Directory.Exists(settingsDir))
                {
                    DirectoryInfo d = new DirectoryInfo(settingsDir);//Assuming Test is your Folder
                    FileInfo[] Files = d.GetFiles("*.dbsettings"); //Getting Text files

                    Dictionary<string, string> test = Files.ToDictionary(f => f.Name, f => Path.GetFileNameWithoutExtension(f.Name));
                    if (test.Count > 0)
                    {
                        comboBox1.DataSource = new BindingSource(test, null);
                        comboBox1.DisplayMember = "Value";
                        comboBox1.ValueMember = "Key";
                    }
                    else
                    {
                        comboBox1.Text = "Defualt";
                    }
                }else
                {
                    Directory.CreateDirectory(settingsDir);
                    comboBox1.Text = "Defualt";
                }
            }
            catch(Exception exp)
            {
                Console.WriteLine(exp.StackTrace);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            loadDataFromView();
            string fileName = comboBox1.Text;
            if (!Directory.Exists(settingsDir))
            {
                Directory.CreateDirectory(settingsDir);
            }
            File.WriteAllText(settingsDir+ fileName + ".dbsettings", comp.ToJson());
            loadDbSettings();
            comboBox1.Text = fileName;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(File.Exists(settingsDir+comboBox1.Text + ".dbsettings"))
            {
                File.Delete(settingsDir+comboBox1.Text + ".dbsettings");
                loadDbSettings();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (File.Exists(settingsDir + comboBox1.Text + ".dbsettings"))
            {
                comp = File.ReadAllText(settingsDir + comboBox1.Text + ".dbsettings").FromJson<ComparerSetings>();
                loadDataToView();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filePath = resultFileName();
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Report file does not exist","Error");
                return;
            }
            string argument = "/select,\"" + filePath + "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }
    }
}
>>>>>>> e68e7ed5666dfbde3e41c1f1195ad72efec876fc
