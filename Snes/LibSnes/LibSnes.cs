using System;
using System.Text;
using System.Threading.Tasks;
using Nall;

namespace Snes
{
    public enum Port { One = 0, Two = 1 }
    public enum Device { None = 0, Joypad = 1, Multitap = 2, Mouse = 3, SuperScope = 4, Justifier = 5, Justifiers = 6 }
    public enum Joypad { B = 1 << 0, Y = 1 << 1, Select = 1 << 2, Start = 1 << 3, Up = 1 << 4, Down = 1 << 5, Left = 1 << 6, Right = 1 << 7, A = 1 << 8, X = 1 << 9, L = 1 << 10, R = 1 << 11 }
    public enum Mouse { X = 1 << 0, Y = 1 << 1, Left = 1 << 2, Right = 1 << 3 }
    public enum SuperScope { X = 1 << 0, Y = 1 << 1, Trigger = 1 << 2, Cursor = 1 << 3, Turbo = 1 << 4, Pause = 1 << 5 }
    public enum Justifier { X = 1 << 0, Y = 1 << 1, Trigger = 1 << 2, Start = 1 << 3 }
    public enum Region { NTSC = 0, PAL = 1 }
    public enum MemoryType { RAM = 0, RTC = 1, BSXRAM = 2, BSXPRAM = 3, SufamiTurboARAM = 4, SufamiTurboBRAM = 5, GameBoyRAM = 6, GameBoyRTC = 7 }

    public static class LibSnes
    {
        public static uint snes_library_revision_major()
        {
            return 1;
        }

        public static uint snes_library_revision_minor()
        {
            return 1;
        }

        public static event EventHandler<VideoRefreshEventArgs> VideoRefresh = null;
        public static event EventHandler<AudioRefreshEventArgs> AudioRefresh = null;
        public static event EventHandler InputPoll = null;
        public static event EventHandler<InputStateEventArgs> InputState = null;

        static LibSnes()
        {
            LibSnesInterface.Inter.pvideo_refresh += new EventHandler<VideoRefreshEventArgs>(inter_pvideo_refresh);
            LibSnesInterface.Inter.paudio_sample += new EventHandler<AudioRefreshEventArgs>(inter_paudio_sample);
            LibSnesInterface.Inter.pinput_poll += new EventHandler(inter_pinput_poll);
            LibSnesInterface.Inter.pinput_state += new EventHandler<InputStateEventArgs>(inter_pinput_state);
        }

        static void inter_pvideo_refresh(object sender, VideoRefreshEventArgs e)
        {
            if (!ReferenceEquals(VideoRefresh, null))
            {
                VideoRefresh(null, e);
            }
        }

        static void inter_paudio_sample(object sender, AudioRefreshEventArgs e)
        {
            if (!ReferenceEquals(AudioRefresh, null))
            {
                AudioRefresh(null, e);
            }
        }

        static void inter_pinput_poll(object sender, EventArgs e)
        {
            if (!ReferenceEquals(InputPoll, null))
            {
                InputPoll(null, e);
            }
        }

        static void inter_pinput_state(object sender, InputStateEventArgs e)
        {
            if (!ReferenceEquals(InputState, null))
            {
                InputState(null, e);
            }
        }

        public static void SetControllerPortDevice(Port port, Device device)
        {
            Input.input.port_set_device(Convert.ToBoolean(port), (Input.Device)device);
        }

        public static void SetCartridgeBasename(string basename)
        {
            Cartridge.cartridge.basename = basename;
        }

        public static void Init()
        {
            System.system.init(LibSnesInterface.Inter);
            Input.input.port_set_device(Convert.ToBoolean(0), Input.Device.Joypad);
            Input.input.port_set_device(Convert.ToBoolean(1), Input.Device.Joypad);
        }

        public static void Term()
        {
            System.system.term();
        }

        public static async Task Power()
        {
            await System.system.power();
        }

        public static async Task Reset()
        {
			await System.system.reset();
        }

        public static async Task Run()
        {
            await System.system.run();
        }

        public static void Exit()
        {
           Libco.Exit();
        }

        public static uint SerializeSize()
        {
            return System.system.serialize_size;
        }

        public static async Task<bool> Serialize(byte[] data, uint size)
        {
            await System.system.runtosave();
            Serializer s = System.system.serialize();
            if (s.size() > size)
            {
                return false;
            }
            Array.Copy(s.data(), data, (int)s.size());
            return true;
        }

        public static async Task<bool> Unserialize(byte[] data, uint size)
        {
            Serializer s = new Serializer(data, size);
            return await System.system.unserialize(s);
        }

        public static void CheatReset()
        {
            Cheat.cheat.Clear();
            Cheat.cheat.synchronize();
        }

        public static void CheatSet(uint index, bool enabled, string code)
        {
            Cheat.cheat[(int)index].Assign(code);
            Cheat.cheat[(int)index].enabled = enabled;
            Cheat.cheat.synchronize();
        }

        public static async Task<bool> LoadCartridgeNormal(byte[] rom_xml, byte[] rom_data, uint rom_size)
        {
            CheatReset();
            if (!ReferenceEquals(rom_data, null))
            {
                MappedRAM.cartrom.copy(rom_data, rom_size);
            }
            string xmlrom = (!ReferenceEquals(rom_xml, null)) ? new UTF8Encoding().GetString(rom_xml, 0, rom_xml.Length) : new SnesInformation(rom_data, rom_size).xml_memory_map;
            Cartridge.cartridge.load(Cartridge.Mode.Normal, new string[] { xmlrom });
            await System.system.power();
            return true;
        }

        public static async Task<bool> LoadCartridgeBsxSlotted(byte[] rom_xml, byte[] rom_data, uint rom_size, byte[] bsx_xml, byte[] bsx_data, uint bsx_size)
        {
            CheatReset();
            if (!ReferenceEquals(rom_data, null))
            {
                MappedRAM.cartrom.copy(rom_data, rom_size);
            }
            string xmlrom = (!ReferenceEquals(rom_xml, null)) ? new UTF8Encoding().GetString(rom_xml, 0, rom_xml.Length) : new SnesInformation(rom_data, rom_size).xml_memory_map;
            if (!ReferenceEquals(bsx_data, null))
            {
                MappedRAM.bsxflash.copy(bsx_data, bsx_size);
            }
            string xmlbsx = (!ReferenceEquals(bsx_xml, null)) ? new UTF8Encoding().GetString(bsx_xml, 0, bsx_xml.Length) : new SnesInformation(bsx_data, bsx_size).xml_memory_map;
            Cartridge.cartridge.load(Cartridge.Mode.BsxSlotted, new string[] { xmlrom, xmlbsx });
            await System.system.power();
            return true;
        }

        public static async Task<bool> LoadCartridgeBsx(byte[] rom_xml, byte[] rom_data, uint rom_size, byte[] bsx_xml, byte[] bsx_data, uint bsx_size)
        {
            CheatReset();
            if (!ReferenceEquals(rom_data, null))
            {
                MappedRAM.cartrom.copy(rom_data, rom_size);
            }
            string xmlrom = (!ReferenceEquals(rom_xml, null)) ? new UTF8Encoding().GetString(rom_xml, 0, rom_xml.Length) : new SnesInformation(rom_data, rom_size).xml_memory_map;
            if (!ReferenceEquals(bsx_data, null))
            {
                MappedRAM.bsxflash.copy(bsx_data, bsx_size);
            }
            string xmlbsx = (!ReferenceEquals(bsx_xml, null)) ? new UTF8Encoding().GetString(bsx_xml, 0, bsx_xml.Length) : new SnesInformation(bsx_data, bsx_size).xml_memory_map;
            Cartridge.cartridge.load(Cartridge.Mode.Bsx, new string[] { xmlrom, xmlbsx });
            await System.system.power();
            return true;
        }

        public static async Task<bool> LoadCartridgeSufamiTurbo(byte[] rom_xml, byte[] rom_data, uint rom_size, byte[] sta_xml, byte[] sta_data, uint sta_size, byte[] stb_xml, byte[] stb_data, uint stb_size)
        {
            CheatReset();
            if (!ReferenceEquals(rom_data, null))
            {
                MappedRAM.cartrom.copy(rom_data, rom_size);
            }
            string xmlrom = (!ReferenceEquals(rom_xml, null)) ? new UTF8Encoding().GetString(rom_xml, 0, rom_xml.Length) : new SnesInformation(rom_data, rom_size).xml_memory_map;
            if (!ReferenceEquals(sta_data, null))
            {
                MappedRAM.stArom.copy(sta_data, sta_size);
            }
            string xmlsta = (!ReferenceEquals(sta_xml, null)) ? new UTF8Encoding().GetString(sta_xml, 0, sta_xml.Length) : new SnesInformation(sta_data, sta_size).xml_memory_map;
            if (!ReferenceEquals(stb_data, null))
            {
                MappedRAM.stBrom.copy(stb_data, stb_size);
            }
            string xmlstb = (!ReferenceEquals(stb_xml, null)) ? new UTF8Encoding().GetString(stb_xml, 0, stb_xml.Length) : new SnesInformation(stb_data, stb_size).xml_memory_map;
            Cartridge.cartridge.load(Cartridge.Mode.SufamiTurbo, new string[] { xmlrom, xmlsta, xmlstb });
            await System.system.power();
            return true;
        }

        public static async Task<bool> LoadCartridgeGameBoy(byte[] rom_xml, byte[] rom_data, uint rom_size, byte[] dmg_xml, byte[] dmg_data, uint dmg_size)
        {
            CheatReset();
            if (!ReferenceEquals(rom_data, null))
            {
                MappedRAM.cartrom.copy(rom_data, rom_size);
            }
            string xmlrom = (!ReferenceEquals(rom_xml, null)) ? new UTF8Encoding().GetString(rom_xml, 0, rom_xml.Length) : new SnesInformation(rom_data, rom_size).xml_memory_map;
            if (!ReferenceEquals(dmg_data, null))
            {
                MappedRAM.gbrom.copy(dmg_data, dmg_size);
            }
            string xmldmg = (!ReferenceEquals(dmg_xml, null)) ? new UTF8Encoding().GetString(dmg_xml, 0, dmg_xml.Length) : new SnesInformation(dmg_data, dmg_size).xml_memory_map;
            Cartridge.cartridge.load(Cartridge.Mode.SuperGameBoy, new string[] { xmlrom, xmldmg });
            await System.system.power();
            return true;
        }

        public static void UnloadCartridge()
        {
            Cartridge.cartridge.unload();
        }

        public static Region Region
        {
            get
            {
                return (Region)Convert.ToInt32(System.system.region == System.Region.NTSC ? Convert.ToBoolean(0) : Convert.ToBoolean(1));
            }
        }

        public static byte[] GetMemoryData(uint id)
        {
            if (Cartridge.cartridge.loaded == false)
            {
                return null;
            }

            switch ((MemoryType)id)
            {
                case MemoryType.RAM:
                    return MappedRAM.cartram.data();
                case MemoryType.RTC:
                    return MappedRAM.cartrtc.data();
                case MemoryType.BSXRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.Bsx)
                    {
                        break;
                    }
                    return MappedRAM.bsxram.data();
                case MemoryType.BSXPRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.Bsx)
                    {
                        break;
                    }
                    return MappedRAM.bsxpram.data();
                case MemoryType.SufamiTurboARAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SufamiTurbo)
                    {
                        break;
                    }
                    return MappedRAM.stAram.data();
                case MemoryType.SufamiTurboBRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SufamiTurbo)
                    {
                        break;
                    }
                    return MappedRAM.stBram.data();
                case MemoryType.GameBoyRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SuperGameBoy)
                    {
                        break;
                    }
                    SuperGameBoy.supergameboy.save();
                    return MappedRAM.gbram.data();
                case MemoryType.GameBoyRTC:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SuperGameBoy)
                    {
                        break;
                    }
                    SuperGameBoy.supergameboy.save();
                    return MappedRAM.gbrtc.data();
            }

            return null;
        }

        public static uint GetMemorySize(uint id)
        {
            if (Cartridge.cartridge.loaded == false)
            {
                return 0;
            }
            uint size = 0;

            switch ((MemoryType)id)
            {
                case MemoryType.RAM:
                    size = MappedRAM.cartram.size();
                    break;
                case MemoryType.RTC:
                    size = MappedRAM.cartrtc.size();
                    break;
                case MemoryType.BSXRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.Bsx)
                    {
                        break;
                    }
                    size = MappedRAM.bsxram.size();
                    break;
                case MemoryType.BSXPRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.Bsx)
                    {
                        break;
                    }
                    size = MappedRAM.bsxpram.size();
                    break;
                case MemoryType.SufamiTurboARAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SufamiTurbo)
                    {
                        break;
                    }
                    size = MappedRAM.stAram.size();
                    break;
                case MemoryType.SufamiTurboBRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SufamiTurbo)
                    {
                        break;
                    }
                    size = MappedRAM.stBram.size();
                    break;
                case MemoryType.GameBoyRAM:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SuperGameBoy)
                    {
                        break;
                    }
                    size = MappedRAM.gbram.size();
                    break;
                case MemoryType.GameBoyRTC:
                    if (Cartridge.cartridge.mode != Cartridge.Mode.SuperGameBoy)
                    {
                        break;
                    }
                    size = MappedRAM.gbrtc.size();
                    break;
            }

            if (size == Bit.ToUint32(-1))
            {
                size = 0;
            }
            return size;
        }
    }
}
