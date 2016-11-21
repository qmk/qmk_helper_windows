using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Midi;
using System.Collections.Generic;

namespace QMK {

    static class Keyboard {
        [Flags]
        public enum SYSEX {
            // out-dated
            START = 0xF0,
            END = 0xF7,

            GET = 0x13,
            HANDSHAKE = 0x00,
            LAYERS = 0x04,

            LED = 0x27,
            HSV = 0x00,
            RGB = 0x01

        }
        public enum RGBLightModes {
            Static = 1,
            Breathing_Slow = 2,
            Breathing_Medium,
            Breathing_Fast,
            Breathing_Very_Fast,
            Rainbow_Slow = 6,
            Rainbow_Medium,
            Rainbow_Fast,
            Rainbow_Swirl_Super_Slow = 9,
            Rainbow_Swirl_Very_Slow,
            Rainbow_Swirl_Slow,
            Rainbow_Swirl_Medium,
            Rainbow_Swirl_Fast,
            Rainbow_Swirl_Very_Fast,
            Snake_Slow = 15,
            Snake_Reverse_Slow,
            Snake_Medium,
            Snake_Reverse_Medium,
            Snake_Fast,
            Snake_Reverse_Fast,
            Knight_Slow = 21,
            Knight_Medium,
            Knight_Fast
        }
        static public InputDevice Input;
        static public OutputDevice Output;
        static public uint default_layer;

        static public List<string> available_keyboards = new List<string>();

        static public void setupInput() {
            updateInput();
            if (Input != null) {
                System.Diagnostics.Debug.WriteLine("Setting up " + Input.Name);
                try {
                    if (!Input.IsOpen) Input.Open();
                    if (!Input.IsReceiving) Input.StartReceiving(null, true);
                    Input.SysEx += new InputDevice.SysExHandler(ReceiveSysex);
                } catch (Exception e) {

                }
            }
        }

        static public void setupOutput() {
            updateOutput();
            if (!Output.IsOpen) Output.Open();
        }

        static public void sendBytes(byte[] bytes) {
            setupOutput();
            byte[] message = new byte[bytes.Length + 5];
            Array.Copy(bytes, 0, message, 4, bytes.Length);
            message[0] = 0xF0;              // 01: F0 - specifies SysEx 
            message[1] = 0x7E;              // 02: 7E - manufacturer ID
            message[2] = 0x00;              // 03: 00 - device ID (not readable?)
            message[3] = 0x00;              // 04: 00 - model ID
            message[message.Length - 1] = 0xF7; // 0x: F7 - end of transmission
            Output.SendSysEx(message);
            Output.Close();
        }

        static public void FindKeyboards() {
            available_keyboards.Clear();
            //InputDevice.UpdateInstalledDevices();
            //OutputDevice.UpdateInstalledDevices();
            foreach (InputDevice device in InputDevice.InstalledDevices) {
                if (device != null) {
                    System.Diagnostics.Debug.WriteLine("Listening to " + device.Name);
                    if (!device.IsOpen)
                        device.Open();
                    if (!device.IsReceiving)
                        device.StartReceiving(null, true);
                    device.RemoveAllEventHandlers();
                    device.SysEx += new InputDevice.SysExHandler(ReceiveSysex);
                }
            }
            foreach (OutputDevice device in OutputDevice.InstalledDevices) {
                Handshake(device);
            }
            /* System.Timers.Timer _delayTimer = new System.Timers.Timer();
            _delayTimer.Interval = 1000;
            _delayTimer.AutoReset = false; //so that it only calls the method once
            _delayTimer.Elapsed += (s, args) => CloseKeyboards();
            _delayTimer.Start();
            */
        }

        static public void CloseKeyboards() {
            foreach (InputDevice device in InputDevice.InstalledDevices) {
                if (device != null) {
                    device.RemoveAllEventHandlers();
                    if (device.IsReceiving)
                        device.StopReceiving();
                    if (device.IsOpen)
                        device.Close();
                }
            }
        }

        static public void updateInput() {
            InputDevice.UpdateInstalledDevices();
            foreach (InputDevice device in InputDevice.InstalledDevices) {
                if (device.Name == Properties.Settings.Default.Keyboard)
                    Input = device;
            }
        }

        static public void updateOutput() {
            OutputDevice.UpdateInstalledDevices();
            foreach (OutputDevice device in OutputDevice.InstalledDevices) {
                if (device.Name == Properties.Settings.Default.Keyboard)
                    Output = device;
            }
        }

        static public void Handshake(OutputDevice device) {
            try {
                System.Diagnostics.Debug.WriteLine("Opening " + device.Name);
                if (!device.IsOpen)
                    device.Open();
                device.SendSysEx(new byte[] { 0xF0, 0x7E, 0x00, 0x00, 0x13, 0x00, 0xF7 });
            } catch (Exception e) {
            } finally {
                if (device.IsOpen)
                    device.Close();
            }
        }
        static public byte[] prepareHSVChunk(uint hue, uint sat, uint val) {
            uint chunk = (hue << 16) | (sat << 8) | val;
            return new byte[] { (byte)((chunk >> 28) & 0x7F), (byte)((chunk >> 21) & 0x7F), (byte)((chunk >> 14) & 0x7F), (byte)((chunk >> 7) & 0x7F), (byte)((chunk) & 0x7F) };
        }
        static public byte[] encode_uint32_chunk(uint data) {
            return new byte[] {
                (byte)((data >> 28) & 0x7F),
                (byte)((data >> 21) & 0x7F),
                (byte)((data >> 14) & 0x7F),
                (byte)((data >> 7) & 0x7F),
                (byte)((data) & 0x7F)
            };
        }
        static public byte[] encode_uint8_chunk(uint data) {
            return new byte[] {
                (byte)((data >> 7) & 0x7F),
                (byte)((data) & 0x7F)
            };
        }
        static public uint decode_uint32_chunk(byte[] data, uint index) {
            uint part1 = data[index++];
            uint part2 = data[index++];
            uint part3 = data[index++];
            uint part4 = data[index++];
            uint part5 = data[index++];
            return ((part1 & 0x1F) << 28) | (part2 << 21) | (part3 << 14) | (part4 << 7) | part5;
        }
        static public uint decode_uint8_chunk(byte[] data, uint index) {
            uint part4 = data[index++];
            uint part5 = data[index++];
            return (part4 << 7) | part5;
        }

        static public void ReceiveSysex(Midi.SysExMessage message) {
            byte[] data = message.Data;
            //foreach (byte thing in data)
            //    System.Diagnostics.Debug.WriteLine("0x{0:X}", thing);
            uint index = 4;
            if (data[0] != 0xF0)
                return;

            switch (data[index++]) {
                case 0x00:
                    System.Diagnostics.Debug.WriteLine(message.Device.Name + " Available");
                    available_keyboards.Add(message.Device.Name);
                    updateInput();
                    updateOutput();
                    break;
                case 0x02:
                    if (Program.optionsWindow != null) {
                        default_layer = decode_uint8_chunk(data, index);
                        for (int i = 0; i < 8; i++) {
                            if ((default_layer >> i & 1) == 1) {
                                Program.optionsWindow.layer_labels[i].BackColor = System.Drawing.Color.FromName("ControlLightLight");
                            } else {
                                Program.optionsWindow.layer_labels[i].BackColor = System.Drawing.Color.FromName("ControlLight");
                            }
                        }
                    }
                    break;
                case 0x03:
                    if (Program.optionsWindow != null) {
                        Program.optionsWindow.UpdateAudio(Convert.ToBoolean(decode_uint8_chunk(data, index)));
                    }
                    break;
                case 0x05:
                    uint unicode = decode_uint32_chunk(data, index);
                    SendKeys.SendWait(char.ConvertFromUtf32((int)unicode).ToString());
                    break;
                case 0x06:
                    if (Program.optionsWindow != null) {
                        Program.optionsWindow.UpdateBacklight(Convert.ToBoolean(decode_uint8_chunk(data, index)));
                    }
                    break;
                case 0x07:
                    if (Program.optionsWindow != null) {
                        uint colordata = decode_uint32_chunk(data, index);
                        //System.Diagnostics.Debug.WriteLine("0x{0:X}", colordata);
                        bool rgblight_enable = Convert.ToBoolean((colordata) & 0x1);
                        Program.optionsWindow.UpdateRGBLight(rgblight_enable);
                        uint mode = (colordata >> 1) & ((1 << 6) - 1);
                        Program.optionsWindow.UpdateRGBLightMode(mode);
                        uint hue = (colordata >> 7) & ((1 << 9) - 1);
                        uint sat = (colordata >> 16) & ((1 << 8) - 1);
                        uint val = (colordata >> 24) & ((1 << 8) - 1);
                        Program.optionsWindow.colorButton.BackColor = ColorFromAhsb(255, hue * 1f, sat / 255.0f, val / 255.0f);
                        if (Program.optionsWindow.colorButton.BackColor.GetBrightness() > 0.5)
                            Program.optionsWindow.colorButton.ForeColor = System.Drawing.Color.Black;
                        else
                            Program.optionsWindow.colorButton.ForeColor = System.Drawing.Color.White;
                    }
                    break;
                case 0x08:
                    if (Program.optionsWindow != null) {
                        uint keymap_data = decode_uint8_chunk(data, index);
                        //System.Diagnostics.Debug.WriteLine("0x{0:X}", keymap_data);
                        bool swap_control_capslock = ((keymap_data & 0x1) == 0x1);
                        bool capslock_to_control = ((keymap_data >> 1 & 0x1) == 0x1);
                        bool swap_lalt_lgui = ((keymap_data >> 2 & 0x1) == 0x1);
                        bool swap_ralt_rgui = ((keymap_data >> 3 & 0x1) == 0x1);
                        bool no_gui = ((keymap_data >> 4 & 0x1) == 0x1);
                        bool swap_grave_esc = ((keymap_data >> 5 & 0x1) == 0x1);
                        bool swap_backslash_backspace = ((keymap_data >> 6 & 0x1) == 0x1);
                        bool nkro = ((keymap_data >> 7 & 0x1) == 0x1);

                        for (int i = 0; i < 8; i++) {
                            Program.optionsWindow.updateKeymapCheckbox(i, (((keymap_data >> i) & 1) == 1));
                        }
                    }
                    break;
            }
        }
        public static Color ColorFromAhsb(int a, float h, float s, float b) {

            if (0 > a || 255 < a) {
                //throw new ArgumentOutOfRangeException("a", a, Properties.Resources.InvalidAlpha);
            }
            if (0f > h || 360f < h) {
                //throw new ArgumentOutOfRangeException("h", h, Properties.Resources.InvalidHue);
            }
            if (0f > s || 1f < s) {
                //throw new ArgumentOutOfRangeException("s", s, Properties.Resources.InvalidSaturation);
            }
            if (0f > b || 1f < b) {
                //throw new ArgumentOutOfRangeException("b", b, Properties.Resources.InvalidBrightness);
            }

            if (0 == s) {
                return Color.FromArgb(a, Convert.ToInt32(b * 255),
                  Convert.ToInt32(b * 255), Convert.ToInt32(b * 255));
            }

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b) {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            } else {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h) {
                h -= 360f;
            }
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2) {
                fMid = h * (fMax - fMin) + fMin;
            } else {
                fMid = fMin - h * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant) {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        }
    }
}
