using System;
using System.Reflection;
using System.Windows.Forms;

namespace QMK {
    partial class Options : Form {
        public System.Windows.Forms.Label[] layer_labels = new Label[16];
        public System.Windows.Forms.CheckBox[] keymap_checkboxes = new CheckBox[8];
        public System.Windows.Forms.Button colorButton;
        delegate void SetIntBoolCallback(int i, bool enabled);
        delegate void SetBoolCallback(bool enabled);
        delegate void SetUintCallback(uint i);
        public Options() {
            InitializeComponent();
            colorButton = button1;

            layer_labels[0] = label2;
            layer_labels[1] = label3;
            layer_labels[2] = label4;
            layer_labels[3] = label5;
            layer_labels[4] = label6;
            layer_labels[5] = label7;
            layer_labels[6] = label8;
            layer_labels[7] = label9;
            layer_labels[8] = label10;
            layer_labels[9] = label11;
            layer_labels[10] = label12;
            layer_labels[11] = label13;
            layer_labels[12] = label14;
            layer_labels[13] = label15;
            layer_labels[14] = label16;
            layer_labels[15] = label17;

            keymap_checkboxes[0] = checkBox1;
            keymap_checkboxes[1] = checkBox2;
            keymap_checkboxes[2] = checkBox3;
            keymap_checkboxes[3] = checkBox4;
            keymap_checkboxes[4] = checkBox5;
            keymap_checkboxes[5] = checkBox6;
            keymap_checkboxes[6] = checkBox7;
            keymap_checkboxes[7] = checkBox8;

            availableKeyboards.DataSource = Keyboard.available_keyboards;
            if (Keyboard.available_keyboards.Contains(Properties.Settings.Default.Keyboard))
                availableKeyboards.SelectedItem = Properties.Settings.Default.Keyboard;

            //Keyboard.updateInput();
            //Keyboard.updateOutput();
            Reinit();
        }

        public void Reinit() {
            foreach (CheckBox cb in keymap_checkboxes) {
                cb.Enabled = false;
            }
            RGBLightCheckBox.Enabled = false;
            audioCheckBox.Enabled = false;
            backlightCheckBox.Enabled = false;
            RGBLightMode.Enabled = false;
            colorButton.Enabled = false;
            label22.Enabled = false;
            label31.Enabled = false;

            for (int i = 0; i < 8; i++) {
                layer_labels[i].BackColor = System.Drawing.Color.FromName("ControlLight");
            }

            Keyboard.MT_GET_DATA(Keyboard.DT.DEFAULT_LAYER, null);
            Keyboard.MT_GET_DATA(Keyboard.DT.CURRENT_LAYER, null);
            Keyboard.MT_GET_DATA(Keyboard.DT.RGBLIGHT, null);
            Keyboard.MT_GET_DATA(Keyboard.DT.KEYMAP_OPTIONS, null);
        }

        public void updateKeymapCheckbox(int i, bool enabled) {
            if (this.keymap_checkboxes[i].InvokeRequired) {
                SetIntBoolCallback d = new SetIntBoolCallback(updateKeymapCheckbox);
                this.Invoke(d, new object[] { i, enabled });
            } else {
                keymap_checkboxes[i].Enabled = true;
                keymap_checkboxes[i].Checked = enabled;
            }
        }


        #region Assembly Attribute Accessors

        public string AssemblyTitle {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0) {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "") {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion {
            get {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany {
            get {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0) {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion



        private void sendButton_Click(object sender, EventArgs e) {
            //System.Threading.Thread.Sleep(1000);
        }

        public void UpdateAudio(bool enabled) {
            if (this.audioCheckBox.InvokeRequired) {
                SetBoolCallback d = new SetBoolCallback(UpdateAudio);
                this.Invoke(d, new object[] { enabled });
            } else {
                audioCheckBox.Enabled = true;
                if (enabled)
                    audioCheckBox.Checked = true;
                else
                    audioCheckBox.Checked = false;
            }   
        }

        public void UpdateBacklight(bool enabled) {
            if (this.audioCheckBox.InvokeRequired) {
                SetBoolCallback d = new SetBoolCallback(UpdateBacklight);
                this.Invoke(d, new object[] { enabled });
            } else {
                backlightCheckBox.Enabled = true;
                if (enabled)
                    backlightCheckBox.Checked = true;
                else
                    backlightCheckBox.Checked = false;
            }
        }

        public void UpdateRGBLight(bool enabled) { 
            if (this.audioCheckBox.InvokeRequired) {
                SetBoolCallback d = new SetBoolCallback(UpdateRGBLight);
                this.Invoke(d, new object[] { enabled });
            } else {
                colorButton.Enabled = true;
                label22.Enabled = true;
                label31.Enabled = true;
                RGBLightCheckBox.Enabled = true;
                if (enabled)
                    RGBLightCheckBox.Checked = true;
                else
                    RGBLightCheckBox.Checked = false;
            }
        }

        public void UpdateRGBLightMode(uint mode) {
            if (this.audioCheckBox.InvokeRequired) {
                SetUintCallback d = new SetUintCallback(UpdateRGBLightMode);
                this.Invoke(d, new object[] { mode });
            } else {
                RGBLightMode.DataSource = Enum.GetValues(typeof(Keyboard.RGBLightModes));
                RGBLightMode.SelectedIndex = (int)mode - 1;
                RGBLightMode.Enabled = true;
            }
        }

        private void tableLayoutPanel_Paint(object sender, PaintEventArgs e) {

        }

        private void button2_Click(object sender, EventArgs e) {
            Keyboard.MT_GET_DATA(Keyboard.DT.CURRENT_LAYER);
        }

        private void label7_Click(object sender, EventArgs e) {

        }

        private void label3_Click(object sender, EventArgs e) {

        }

        private void label5_Click(object sender, EventArgs e) {

        }

        private void label6_Click(object sender, EventArgs e) {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) {

        }

        private void label4_Click(object sender, EventArgs e) {

        }

        private void AboutBox_Load(object sender, EventArgs e) {

        }

        private void RGBLightMode_SelectedIndexChanged(object sender, EventArgs e) {
            ComboBox comboBox = (ComboBox)sender;
            //if (Keyboard.rgb_state != null) {
                Keyboard.rgb_state.mode = (byte)(comboBox.SelectedIndex + 1);
                Keyboard.MT_SET_DATA(Keyboard.DT.RGBLIGHT, Keyboard.rgb_state.bytes);
            //}
        }

        private void button1_Click(object sender, EventArgs e) {
            if (colorDialog1.ShowDialog() == DialogResult.OK) {
                button1.BackColor = colorDialog1.Color;
                Keyboard.rgb_state.hue = (ushort)button1.BackColor.GetHue();
                Keyboard.rgb_state.sat = (byte)(button1.BackColor.GetSaturation() * 255);
                Keyboard.rgb_state.val = (byte)(button1.BackColor.GetBrightness() * 255);
                Keyboard.MT_SET_DATA(Keyboard.DT.RGBLIGHT, Keyboard.rgb_state.bytes);
                if (button1.BackColor.GetBrightness() > 0.5)
                    button1.ForeColor = System.Drawing.Color.Black;
                else
                    button1.ForeColor = System.Drawing.Color.White;
            }
        }

        private void button2_Click_2(object sender, EventArgs e) {
            new ApplicationMapper().ShowDialog();
        }

        private void label2_Click(object sender, EventArgs e) {
            Label label = (Label)sender;
            for (int i = 0; i < 8; i++) {
                if (label == layer_labels[i]) {
                    Keyboard.default_layer ^= (uint)(1 << i);
                }
            }
            Keyboard.MT_SET_DATA(Keyboard.DT.DEFAULT_LAYER, new byte[] { (byte)Keyboard.default_layer });
        }

        private void availableKeyboards_SelectedValueChanged(object sender, EventArgs e) {
            //System.Diagnostics.Debug.WriteLine(availableKeyboards.SelectedItem.ToString());
            Properties.Settings.Default.Keyboard = availableKeyboards.SelectedItem.ToString();
            Properties.Settings.Default.Save();
            Keyboard.updateInput();
            Keyboard.updateOutput();
            Reinit();
        }

        private void button3_Click(object sender, EventArgs e) {
            Keyboard.FindKeyboards();
        }

        private void RGBLightCheckBox_CheckedChanged(object sender, EventArgs e) {
            CheckBox box = (CheckBox)sender;
            Keyboard.rgb_state.enabled = box.Checked;
            Keyboard.MT_SET_DATA(Keyboard.DT.RGBLIGHT, Keyboard.rgb_state.bytes);
        }
    }
}