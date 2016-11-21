using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace QMK
{
    public partial class ApplicationMapper : Form
    {
        public ApplicationMapper()
        {
            InitializeComponent();
        }

        private void ApplicationMapper_Load(object sender, EventArgs e)
        {
            List<ApplicationMapping> dict = JsonConvert.DeserializeObject<List<ApplicationMapping>>(Properties.Settings.Default.ApplicationDictionary);
            dataGridView1.DataSource = new BindingSource(dict, null);
        }
    }
    public class ApplicationMapping
    {
        public string Application { get; set; }
        public int Layer { get; set; }
    }
}
