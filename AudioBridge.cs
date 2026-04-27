using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Globalization;

namespace VolumeFlow
{
    // ============================================================
    // COM Interfaces - Windows Core Audio API
    // ============================================================

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumeratorComObject { }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(int dataFlow, int dwStateMask, out IMMDeviceCollection ppDevices);
        [PreserveSig]
        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
    }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceCollection
    {
        [PreserveSig]
        int GetCount(out int pcDevices);
        [PreserveSig]
        int Item(int nDevice, out IMMDevice ppDevice);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        [PreserveSig]
        int OpenPropertyStore(int stgmAccess, [MarshalAs(UnmanagedType.IUnknown)] out object ppProperties);
        [PreserveSig]
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        [PreserveSig]
        int GetState(out int pdwState);
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionManager2
    {
        // IAudioSessionManager methods
        [PreserveSig]
        int GetAudioSessionControl(ref Guid AudioSessionGuid, int StreamFlags,
            [MarshalAs(UnmanagedType.IUnknown)] out object SessionControl);
        [PreserveSig]
        int GetSimpleAudioVolume(ref Guid AudioSessionGuid, int StreamFlags,
            [MarshalAs(UnmanagedType.IUnknown)] out object AudioVolume);
        // IAudioSessionManager2 methods
        [PreserveSig]
        int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionEnumerator
    {
        [PreserveSig]
        int GetCount(out int SessionCount);
        [PreserveSig]
        int GetSession(int SessionCount, out IAudioSessionControl Session);
    }

    [Guid("F4B1A599-7266-4319-A8C4-E743B7601C54"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionControl
    {
        [PreserveSig]
        int GetState(out int pRetVal);
        [PreserveSig]
        int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig]
        int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig]
        int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig]
        int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig]
        int GetGroupingParam(out Guid pRetVal);
        [PreserveSig]
        int SetGroupingParam(ref Guid Override, ref Guid EventContext);
        [PreserveSig]
        int RegisterAudioSessionNotification([MarshalAs(UnmanagedType.IUnknown)] object NewNotifications);
        [PreserveSig]
        int UnregisterAudioSessionNotification([MarshalAs(UnmanagedType.IUnknown)] object NewNotifications);
    }

    [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionControl2
    {
        // IAudioSessionControl methods (must be repeated in vtable order)
        [PreserveSig]
        int GetState(out int pRetVal);
        [PreserveSig]
        int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig]
        int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig]
        int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig]
        int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, ref Guid EventContext);
        [PreserveSig]
        int GetGroupingParam(out Guid pRetVal);
        [PreserveSig]
        int SetGroupingParam(ref Guid Override, ref Guid EventContext);
        [PreserveSig]
        int RegisterAudioSessionNotification([MarshalAs(UnmanagedType.IUnknown)] object NewNotifications);
        [PreserveSig]
        int UnregisterAudioSessionNotification([MarshalAs(UnmanagedType.IUnknown)] object NewNotifications);

        // IAudioSessionControl2 methods
        [PreserveSig]
        int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig]
        int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);
        [PreserveSig]
        int GetProcessId(out uint pRetVal);
        [PreserveSig]
        int IsSystemSoundsSession();
        [PreserveSig]
        int SetDuckingPreference(bool optOut);
    }

    [Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioMeterInformation
    {
        [PreserveSig]
        int GetPeakValue(out float pfPeak);
        [PreserveSig]
        int GetChannelsPeakValues(int u32ChannelCount, [Out] float[] afPeakValues);
        [PreserveSig]
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fLevel, ref Guid EventContext);
        [PreserveSig]
        int GetMasterVolume(out float pfLevel);
        [PreserveSig]
        int SetMute(bool bMute, ref Guid EventContext);
        [PreserveSig]
        int GetMute(out bool pbMute);
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioEndpointVolume
    {
        [PreserveSig]
        int RegisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig]
        int UnregisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig]
        int GetChannelCount(out uint pnChannelCount);
        [PreserveSig]
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig]
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        [PreserveSig]
        int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig]
        int GetMasterVolumeLevelScalar(out float pfLevel);
        [PreserveSig]
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig]
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
        [PreserveSig]
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
        [PreserveSig]
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
        [PreserveSig]
        int SetMute(bool bMute, ref Guid pguidEventContext);
        [PreserveSig]
        int GetMute(out bool pbMute);
        [PreserveSig]
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        [PreserveSig]
        int VolumeStepUp(ref Guid pguidEventContext);
        [PreserveSig]
        int VolumeStepDown(ref Guid pguidEventContext);
        [PreserveSig]
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        [PreserveSig]
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    // ============================================================
    // Audio Bridge - Persistent stdin/stdout JSON bridge
    // ============================================================

    class AudioBridge
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("{\"status\":\"ready\"}");
            Console.Out.Flush();

            // Start peak polling thread
            var peakThread = new System.Threading.Thread(PeakPollingLoop);
            peakThread.IsBackground = true;
            peakThread.Start();

            string line;
            while ((line = Console.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    string action = ExtractJsonString(line, "action");

                    switch (action)
                    {
                        case "get_sessions":
                            HandleGetSessions();
                            break;
                        case "get_master":
                            HandleGetMaster();
                            break;
                        case "set_volume":
                            {
                                uint pid = uint.Parse(ExtractJsonString(line, "pid"));
                                float vol = float.Parse(ExtractJsonString(line, "volume"), CultureInfo.InvariantCulture);
                                HandleSetVolume(pid, vol);
                            }
                            break;
                        case "set_master_volume":
                            {
                                float vol = float.Parse(ExtractJsonString(line, "volume"), CultureInfo.InvariantCulture);
                                HandleSetMasterVolume(vol);
                            }
                            break;
                        case "toggle_mute":
                            {
                                uint pid = uint.Parse(ExtractJsonString(line, "pid"));
                                HandleToggleMute(pid);
                            }
                            break;
                        case "ping":
                            Console.WriteLine("{\"status\":\"pong\"}");
                            Console.Out.Flush();
                            break;
                        case "exit":
                            return;
                        default:
                            Console.WriteLine("{\"error\":\"unknown_action\"}");
                            Console.Out.Flush();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{\"error\":\"exception\",\"detail\":\"" + EscapeJson(ex.Message) + "\"}");
                    Console.Out.Flush();
                }
            }
        }

        static void HandleGetSessions()
        {
            var sb = new StringBuilder();
            sb.Append("{\"sessions\":[");

            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                enumerator.GetDefaultAudioEndpoint(0, 1, out device);

                object objMgr;
                Guid iidMgr = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                device.Activate(ref iidMgr, 1, IntPtr.Zero, out objMgr);
                var mgr = (IAudioSessionManager2)objMgr;

                IAudioSessionEnumerator sessionEnum;
                mgr.GetSessionEnumerator(out sessionEnum);

                int count;
                sessionEnum.GetCount(out count);

                bool first = true;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        IAudioSessionControl sessionCtl;
                        sessionEnum.GetSession(i, out sessionCtl);

                        var session2 = sessionCtl as IAudioSessionControl2;
                        if (session2 == null) continue;

                        uint pid;
                        session2.GetProcessId(out pid);
                        if (pid == 0) continue;

                        string processName = "Unknown";
                        string processPath = "";
                        try
                        {
                            var proc = Process.GetProcessById((int)pid);
                            processName = proc.ProcessName;
                            try { processPath = proc.MainModule.FileName; } catch { }
                        }
                        catch { continue; }

                        var vol = sessionCtl as ISimpleAudioVolume;
                        if (vol == null) continue;

                        float level;
                        vol.GetMasterVolume(out level);

                        bool muted;
                        vol.GetMute(out muted);

                        if (!first) sb.Append(",");
                        sb.Append("{");
                        sb.Append("\"pid\":" + pid);
                        sb.Append(",\"name\":\"" + EscapeJson(processName) + "\"");
                        sb.Append(",\"path\":\"" + EscapeJson(processPath) + "\"");
                        sb.Append(",\"volume\":" + level.ToString("F4", CultureInfo.InvariantCulture));
                        sb.Append(",\"muted\":" + (muted ? "true" : "false"));
                        sb.Append("}");
                        first = false;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                sb.Clear();
                sb.Append("{\"sessions\":[],\"error\":\"" + EscapeJson(ex.Message) + "\"");
            }

            sb.Append("]}");
            Console.WriteLine(sb.ToString());
            Console.Out.Flush();
        }

        static void HandleGetMaster()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                enumerator.GetDefaultAudioEndpoint(0, 1, out device);

                object objVol;
                Guid iidEpv = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidEpv, 1, IntPtr.Zero, out objVol);
                var endpointVol = (IAudioEndpointVolume)objVol;

                float level;
                endpointVol.GetMasterVolumeLevelScalar(out level);
                bool muted;
                endpointVol.GetMute(out muted);

                Console.WriteLine("{\"master\":{\"volume\":" +
                    (level * 100).ToString("F1", CultureInfo.InvariantCulture) +
                    ",\"muted\":" + (muted ? "true" : "false") + "}}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("{\"master\":{\"volume\":50,\"muted\":false},\"error\":\"" + EscapeJson(ex.Message) + "\"}");
            }
            Console.Out.Flush();
        }

        static void HandleSetVolume(uint targetPid, float level)
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                enumerator.GetDefaultAudioEndpoint(0, 1, out device);

                object objMgr;
                Guid iidMgr = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                device.Activate(ref iidMgr, 1, IntPtr.Zero, out objMgr);
                var mgr = (IAudioSessionManager2)objMgr;

                IAudioSessionEnumerator sessionEnum;
                mgr.GetSessionEnumerator(out sessionEnum);
                int count;
                sessionEnum.GetCount(out count);

                bool found = false;
                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl sessionCtl;
                    sessionEnum.GetSession(i, out sessionCtl);
                    var session2 = sessionCtl as IAudioSessionControl2;
                    if (session2 == null) continue;

                    uint pid;
                    session2.GetProcessId(out pid);
                    if (pid == targetPid)
                    {
                        var vol = sessionCtl as ISimpleAudioVolume;
                        Guid g = Guid.Empty;
                        vol.SetMasterVolume(level, ref g);
                        found = true;
                    }
                }

                Console.WriteLine("{\"ok\":true,\"found\":" + (found ? "true" : "false") + "}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("{\"ok\":false,\"error\":\"" + EscapeJson(ex.Message) + "\"}");
            }
            Console.Out.Flush();
        }

        static void HandleSetMasterVolume(float levelPercent)
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                enumerator.GetDefaultAudioEndpoint(0, 1, out device);

                object objVol;
                Guid iidEpv = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidEpv, 1, IntPtr.Zero, out objVol);
                var endpointVol = (IAudioEndpointVolume)objVol;

                float scalar = Math.Max(0f, Math.Min(1f, levelPercent / 100f));
                Guid g = Guid.Empty;
                endpointVol.SetMasterVolumeLevelScalar(scalar, ref g);

                Console.WriteLine("{\"ok\":true}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("{\"ok\":false,\"error\":\"" + EscapeJson(ex.Message) + "\"}");
            }
            Console.Out.Flush();
        }

        static void HandleToggleMute(uint targetPid)
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                enumerator.GetDefaultAudioEndpoint(0, 1, out device);

                object objMgr;
                Guid iidMgr = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                device.Activate(ref iidMgr, 1, IntPtr.Zero, out objMgr);
                var mgr = (IAudioSessionManager2)objMgr;

                IAudioSessionEnumerator sessionEnum;
                mgr.GetSessionEnumerator(out sessionEnum);
                int count;
                sessionEnum.GetCount(out count);

                bool found = false;
                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl sessionCtl;
                    sessionEnum.GetSession(i, out sessionCtl);
                    var session2 = sessionCtl as IAudioSessionControl2;
                    if (session2 == null) continue;

                    uint pid;
                    session2.GetProcessId(out pid);
                    if (pid == targetPid)
                    {
                        var vol = sessionCtl as ISimpleAudioVolume;
                        bool muted;
                        vol.GetMute(out muted);
                        Guid g = Guid.Empty;
                        vol.SetMute(!muted, ref g);
                        found = true;
                    }
                }

                Console.WriteLine("{\"ok\":true,\"found\":" + (found ? "true" : "false") + "}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("{\"ok\":false,\"error\":\"" + EscapeJson(ex.Message) + "\"}");
            }
            Console.Out.Flush();
        }

        static void PeakPollingLoop()
        {
            while (true)
            {
                try
                {
                    var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                    IMMDevice device;
                    if (enumerator.GetDefaultAudioEndpoint(0, 1, out device) == 0)
                    {
                        object objMgr;
                        Guid iidMgr = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                        device.Activate(ref iidMgr, 1, IntPtr.Zero, out objMgr);
                        var mgr = (IAudioSessionManager2)objMgr;

                        IAudioSessionEnumerator sessionEnum;
                        mgr.GetSessionEnumerator(out sessionEnum);

                        int count;
                        sessionEnum.GetCount(out count);

                        var sb = new StringBuilder();
                        sb.Append("{\"type\":\"peaks\",\"peaks\":{");
                        bool first = true;

                        for (int i = 0; i < count; i++)
                        {
                            try
                            {
                                IAudioSessionControl sessionCtl;
                                sessionEnum.GetSession(i, out sessionCtl);
                                var session2 = sessionCtl as IAudioSessionControl2;
                                if (session2 == null) continue;

                                uint pid;
                                session2.GetProcessId(out pid);
                                if (pid == 0) continue;

                                var meter = sessionCtl as IAudioMeterInformation;
                                if (meter != null)
                                {
                                    float peak;
                                    meter.GetPeakValue(out peak);

                                    if (peak > 0.0001f) // Only send if actually playing something
                                    {
                                        if (!first) sb.Append(",");
                                        sb.Append("\"" + pid + "\":" + peak.ToString("F4", CultureInfo.InvariantCulture));
                                        first = false;
                                    }
                                }
                            }
                            catch { }
                        }

                        sb.Append("}}");
                        if (!first) // Only send if there are any active peaks
                        {
                            Console.WriteLine(sb.ToString());
                            Console.Out.Flush();
                        }
                    }
                }
                catch { }
                System.Threading.Thread.Sleep(50); // ~20 FPS
            }
        }

        // ============================================================
        // Helpers
        // ============================================================

        static string ExtractJsonString(string json, string key)
        {
            string search = "\"" + key + "\":";
            int idx = json.IndexOf(search);
            if (idx < 0) return "";
            int valStart = idx + search.Length;
            while (valStart < json.Length && json[valStart] == ' ') valStart++;
            if (valStart >= json.Length) return "";

            if (json[valStart] == '"')
            {
                int end = json.IndexOf('"', valStart + 1);
                if (end < 0) return "";
                return json.Substring(valStart + 1, end - valStart - 1);
            }
            else
            {
                int end = valStart;
                while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != ' ')
                    end++;
                return json.Substring(valStart, end - valStart);
            }
        }

        static string EscapeJson(string s)
        {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
    }
}
