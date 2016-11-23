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


        static ushort sysex_encoded_length(ushort decoded_length) {
            byte remainder = (byte)(decoded_length % 7);
            if (remainder > 0)
                return (ushort)((decoded_length / 7) * 8 + remainder + 1);
            else
                return (ushort)((decoded_length / 7) * 8);
        }

        static ushort sysex_decoded_length(ushort encoded_length) {
            byte remainder =(byte)(encoded_length % 8);
            if (remainder > 0)
                return (ushort)((encoded_length / 8) * 7 + remainder - 1);
            else
                return (ushort)((encoded_length / 8) * 7);
        }

        static ushort sysex_encode(byte[] encoded, byte[] source, ushort length){
            ushort encoded_full = (ushort)(length / 7); //number of full 8 byte sections from 7 bytes of input
            ushort i, j;

            //fill out the fully encoded sections
            for(i = 0; i<encoded_full; i++) {
                ushort encoded_msb_idx = (ushort)(i * 8);
                ushort input_start_idx = (ushort)(i * 7);
                encoded[encoded_msb_idx] = 0;
                for(j = 0; j< 7; j++){
                    byte current = source[input_start_idx + j];
                    encoded[encoded_msb_idx] |= (byte)((0x80 & current) >> (1 + j));
                    encoded[encoded_msb_idx + 1 + j] = (byte)(0x7F & current);
                }
            }

            //fill out the rest if there is any more
            byte remainder = (byte)(length % 7);
            if (remainder > 0) {
                ushort encoded_msb_idx = (ushort)(encoded_full * 8);
                ushort input_start_idx = (ushort)(encoded_full * 7);
                encoded[encoded_msb_idx] = 0;
                for(j = 0; j<remainder; j++){
                    byte current = source[input_start_idx + j];
                    encoded[encoded_msb_idx] |= (byte)((0x80 & current) >> (1 + j));
                    encoded[encoded_msb_idx + 1 + j] = (byte)(0x7F & current);
                }
                return (ushort)(encoded_msb_idx + remainder + 1);
            } else {
                return (ushort)(encoded_full* 8);
            }
        }

        static ushort sysex_decode(byte[] decoded, byte[] source, ushort length) {
            ushort decoded_full = (ushort)(length / 8);
            ushort i, j;

            if (length < 2)
                return 0;

            //fill out the fully encoded sections
            for (i = 0; i < decoded_full; i++) {
                ushort encoded_msb_idx = (ushort)(i * 8);
                ushort output_start_index = (ushort)(i * 7);
                for (j = 0; j < 7; j++) {
                    decoded[output_start_index + j] = (byte)(0x7F & source[encoded_msb_idx + j + 1]);
                    decoded[output_start_index + j] |= (byte)((0x80 & (source[encoded_msb_idx] << (1 + j))));
                }
            }
            byte remainder = (byte)(length % 8);
            if (remainder > 0) {
                ushort encoded_msb_idx = (ushort)(decoded_full * 8);
                ushort output_start_index = (ushort)(decoded_full * 7);
                for (j = 0; j < (remainder - 1); j++) {
                    decoded[output_start_index + j] = (byte)(0x7F & source[encoded_msb_idx + j + 1]);
                    decoded[output_start_index + j] |= (byte)((0x80 & (source[encoded_msb_idx] << (1 + j))));
                }
                return (ushort)(decoded_full * 7 + remainder - 1);
            } else {
                return (ushort)(decoded_full * 7);
            }
        }




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

        static public void setupOutput(OutputDevice output = null) {
            if (output == null)
                output = Output;
            updateOutput();
            if (!output.IsOpen) output.Open();
        }

        static public void sendBytes(byte[] bytes, OutputDevice output = null) {
            if (output == null)
                output = Output;
            setupOutput(output);
            byte[] encoded = new byte[sysex_encoded_length((ushort)bytes.Length)];
            ushort encoded_length = sysex_encode(encoded, bytes, (ushort)bytes.Length);
            byte[] message = new byte[encoded_length + 5];
            Array.Copy(encoded, 0, message, 4, encoded_length);
            message[0] = 0xF0;              // 01: F0 - specifies SysEx 
            message[1] = 0x7E;              // 02: 7E - manufacturer ID
            message[2] = 0x00;              // 03: 00 - device ID (not readable?)
            message[3] = 0x00;              // 04: 00 - model ID
            message[message.Length - 1] = 0xF7; // 0x: F7 - end of transmission
            output.SendSysEx(message);
            output.Close();

            System.Diagnostics.Debug.WriteLine("TX: " + BitConverter.ToString(message) + " (" + output.Name + ")");
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
                //System.Diagnostics.Debug.WriteLine("Opening " + device.Name);
                sendBytes(new byte[] { 0x13, 0x00 }, device);
            } catch (Exception e) {
            } finally {
                if (device.IsOpen)
                    device.Close();
            }
        }
        static public byte[] prepareHSVChunk(uint hue, uint sat, uint val) {
            return uint_to_bytes((hue << 16) | (sat << 8) | val);
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

        static uint bytes_to_uint(byte[] bytes, uint index) {
            return (uint)((bytes[index + 0] << 24) | (bytes[index + 1] << 16) | (bytes[index + 2] << 8) | bytes[index + 3]);
        }

        static byte[] uint_to_bytes(uint i) {
            return new byte[] { (byte)((i >> 24) & 0xFF), (byte)((i >> 16) & 0xFF), (byte)((i >> 8) & 0xFF), (byte)((i) & 0xFF) };
        }

        static public void ReceiveSysex(Midi.SysExMessage message) {
            System.Diagnostics.Debug.WriteLine("RX: " + BitConverter.ToString(message.Data) + " (" + message.Device.Name + ")");
            byte[] data = new byte[message.Data.Length - 5];
            Array.Copy(message.Data, 4, data, 0, message.Data.Length - 5);
            //foreach (byte thing in data)
            //    System.Diagnostics.Debug.WriteLine("0x{0:X}", thing);
            uint index = 0;
            byte[] decoded = new byte[sysex_decoded_length((ushort)data.Length)];
            ushort decoded_length = sysex_decode(decoded, data, (ushort)data.Length);

            switch (decoded[index++]) {
                case 0x00:
                    System.Diagnostics.Debug.WriteLine(message.Device.Name + " Available");
                    available_keyboards.Add(message.Device.Name);
                    updateInput();
                    updateOutput();
                    break;
                case 0x02:
                    if (Program.optionsWindow != null) {
                        for (int i = 0; i < 8; i++) {
                            if ((decoded[index] >> i & 1) == 1) {
                                Program.optionsWindow.layer_labels[i].BackColor = System.Drawing.Color.FromName("ControlLightLight");
                            } else {
                                Program.optionsWindow.layer_labels[i].BackColor = System.Drawing.Color.FromName("ControlLight");
                            }
                        }
                    }
                    break;
                case 0x03:
                    if (Program.optionsWindow != null) {
                        Program.optionsWindow.UpdateAudio(Convert.ToBoolean(decoded[index]));
                    }
                    break;
                case 0x05:
                    uint unicode = decode_uint32_chunk(data, index);
                    SendKeys.SendWait(char.ConvertFromUtf32((int)bytes_to_uint(decoded, index)).ToString());
                    break;
                case 0x06:
                    if (Program.optionsWindow != null) {
                        Program.optionsWindow.UpdateBacklight(Convert.ToBoolean(decoded[index]));
                    }
                    break;
                case 0x07:
                    if (Program.optionsWindow != null) {
                        uint colordata = bytes_to_uint(decoded, index);
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
                        uint keymap_data = decoded[index];
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
