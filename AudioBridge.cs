using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace VolumeFlow
{
    // ============================================================
    // COM Interfaces - Windows Core Audio API
    // ============================================================

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    class MMDeviceEnumeratorComObject { }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection deviceCollection);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice device);
        [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
        [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr client);
        [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr client);
    }

    [ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection {
        [PreserveSig] int GetCount(out int count);
        [PreserveSig] int Item(int index, out IMMDevice device);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDevice {
        [PreserveSig] int Activate(ref Guid iid, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object interfacePtr);
        [PreserveSig] int OpenPropertyStore(int stgmAccess, out object properties);
        [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        [PreserveSig] int GetState(out int state);
    }

    [ComImport, Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionManager {
        [PreserveSig] int GetAudioSessionControl([MarshalAs(UnmanagedType.LPStruct)] Guid sessionId, int flags, out object sessionControl);
        [PreserveSig] int GetSimpleAudioVolume([MarshalAs(UnmanagedType.LPStruct)] Guid sessionId, int flags, out object simpleAudioVolume);
    }

    [ComImport, Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionManager2 {
        [PreserveSig] int GetAudioSessionControl([MarshalAs(UnmanagedType.LPStruct)] Guid sessionId, int flags, out object sessionControl);
        [PreserveSig] int GetSimpleAudioVolume([MarshalAs(UnmanagedType.LPStruct)] Guid sessionId, int flags, out object simpleAudioVolume);
        [PreserveSig] int GetSessionEnumerator(out IAudioSessionEnumerator enumerator);
        [PreserveSig] int RegisterSessionNotification(IntPtr client);
        [PreserveSig] int UnregisterSessionNotification(IntPtr client);
        [PreserveSig] int RegisterDuckNotification(string sessionId, IntPtr client);
        [PreserveSig] int UnregisterDuckNotification(IntPtr client);
    }

    [ComImport, Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionEnumerator {
        [PreserveSig] int GetCount(out int count);
        [PreserveSig] int GetSession(int index, out IAudioSessionControl session);
    }

    [ComImport, Guid("249e05f2-9844-4861-8400-53412579b29e"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionEvents {
        [PreserveSig] int OnDisplayNameChanged([MarshalAs(UnmanagedType.LPWStr)] string displayName, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int OnIconPathChanged([MarshalAs(UnmanagedType.LPWStr)] string iconPath, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int OnSimpleVolumeChanged(float volume, bool isMuted, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int OnChannelVolumeChanged(int channelCount, IntPtr newChannelVolumes, int channelIndex, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int OnGroupingParamChanged([MarshalAs(UnmanagedType.LPStruct)] Guid groupingParam, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int OnStateChanged(int state);
        [PreserveSig] int OnSessionDisconnected(int disconnectReason);
    }

    [ComImport, Guid("f4b1a599-7266-4319-a8ca-e70acb11e8cd"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionControl {
        [PreserveSig] int GetState(out int state);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string displayName);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string displayName, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string iconPath);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string iconPath, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetGroupingParam(out Guid groupingParam);
        [PreserveSig] int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid groupingParam, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IAudioSessionEvents client);
        [PreserveSig] int UnregisterAudioSessionNotification(IAudioSessionEvents client);
    }

    [ComImport, Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioSessionControl2 {
        [PreserveSig] int GetState(out int state);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string displayName);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string displayName, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string iconPath);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string iconPath, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetGroupingParam(out Guid groupingParam);
        [PreserveSig] int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid groupingParam, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IAudioSessionEvents client);
        [PreserveSig] int UnregisterAudioSessionNotification(IAudioSessionEvents client);
        [PreserveSig] int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string id);
        [PreserveSig] int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string id);
        [PreserveSig] int GetProcessId(out int pid);
        [PreserveSig] int IsSystemSoundsSession();
        [PreserveSig] int SetDuckingPreference(bool optOut);
    }

    [ComImport, Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISimpleAudioVolume {
        [PreserveSig] int SetMasterVolume(float level, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetMasterVolume(out float level);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
    }

    [ComImport, Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioMeterInformation {
        [PreserveSig] int GetPeakValue(out float peak);
        [PreserveSig] int GetMeteringChannelCount(out int channelCount);
        [PreserveSig] int GetChannelsPeakValues(int channelCount, [Out] float[] peakValues);
        [PreserveSig] int QueryHardwareSupport(out int hardwareSupportMask);
    }

    [ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolume {
        [PreserveSig] int RegisterControlChangeNotify(IntPtr client);
        [PreserveSig] int UnregisterControlChangeNotify(IntPtr client);
        [PreserveSig] int GetChannelCount(out int channelCount);
        [PreserveSig] int SetMasterVolumeLevel(float level, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int SetMasterVolumeLevelScalar(float level, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetMasterVolumeLevel(out float level);
        [PreserveSig] int GetMasterVolumeLevelScalar(out float level);
        [PreserveSig] int SetChannelVolumeLevel(int channel, float level, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int SetChannelVolumeLevelScalar(int channel, float level, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetChannelVolumeLevel(int channel, out float level);
        [PreserveSig] int GetChannelVolumeLevelScalar(int channel, out float level);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, [MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
        [PreserveSig] int GetVolumeStepInfo(out int step, out int stepCount);
        [PreserveSig] int VolumeStepUp([MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int VolumeStepDown([MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);
        [PreserveSig] int QueryHardwareSupport(out int hardwareSupportMask);
        [PreserveSig] int GetVolumeRange(out float minLevel, out float maxLevel, out float increment);
    }

    class SessionInfo {
        public int ProcessId;
        public float PeakValue;
    }

    // ============================================================
    // Audio Bridge
    // ============================================================

    class AudioBridge
    {
        static string lastSessionsJson = "{\"status\":\"ok\",\"sessions\":[]}";
        static readonly object sessionsLock = new object();
        static readonly object stdoutLock = new object();

        static void Log(string msg) {
            try { System.IO.File.AppendAllText("bridge_debug.log", DateTime.Now.ToString("HH:mm:ss.fff") + " - " + msg + "\r\n"); } catch {}
        }

        static void Main(string[] args)
        {
            Log("Bridge starting (V1.3.1 - MTA Fix)...");
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            lock (stdoutLock) {
                Console.WriteLine("{\"status\":\"ready\"}");
                Console.Out.Flush();
            }

            var peakThread = new System.Threading.Thread(PeakPollingLoop);
            peakThread.SetApartmentState(System.Threading.ApartmentState.MTA);
            peakThread.IsBackground = true;
            peakThread.Start();

            string line;
            while ((line = Console.ReadLine()) != null)
            {
                line = line.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                try {
                    var serializer = new JavaScriptSerializer();
                    var dict = serializer.Deserialize<Dictionary<string, object>>(line);
                    if (dict == null || !dict.ContainsKey("action")) continue;
                    
                    string action = dict["action"].ToString();
                    switch (action) {
                        case "get_sessions": HandleGetSessions(); break;
                        case "get_master": HandleGetMaster(); break;
                        case "set_volume":
                            if (dict.ContainsKey("pid") && dict.ContainsKey("volume")) {
                                uint pid = Convert.ToUInt32(dict["pid"]);
                                float vol = Convert.ToSingle(dict["volume"], CultureInfo.InvariantCulture);
                                HandleSetVolume(pid, vol);
                            }
                            break;
                        case "set_master_volume":
                            if (dict.ContainsKey("volume")) {
                                float mvol = Convert.ToSingle(dict["volume"], CultureInfo.InvariantCulture);
                                HandleSetMasterVolume(mvol);
                            }
                            break;
                        case "toggle_mute":
                            if (dict.ContainsKey("pid")) {
                                uint pidMute = Convert.ToUInt32(dict["pid"]);
                                HandleToggleMute(pidMute);
                            }
                            break;
                        case "ping": 
                            lock (stdoutLock) {
                                Console.WriteLine("{\"status\":\"pong\"}"); 
                                Console.Out.Flush(); 
                            }
                            break;
                        case "exit": return;
                    }
                } catch (Exception ex) {
                    Log("Action Error: " + ex.Message);
                }
            }
        }

        static void PeakPollingLoop()
        {
            Log("Peak thread started (V1.4.0 - Dynamic Device)");
            string lastDeviceId = null;
            IMMDevice bestDevice = null;
            IAudioSessionManager2 bestManager = null;
            IAudioMeterInformation bestMeter = null;

            try
            {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                while (true)
                {
                    try
                    {
                        IMMDevice currentDevice = null;
                        int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out currentDevice); // eRender, eConsole
                        if (res != 0 || currentDevice == null) {
                            Log("No default device found.");
                            System.Threading.Thread.Sleep(1000);
                            continue;
                        }

                        string currentId;
                        currentDevice.GetId(out currentId);

                        if (currentId != lastDeviceId) {
                            Log("Switching to device: " + currentId);
                            if (bestMeter != null) Marshal.ReleaseComObject(bestMeter);
                            if (bestManager != null) Marshal.ReleaseComObject(bestManager);
                            if (bestDevice != null) Marshal.ReleaseComObject(bestDevice);

                            bestDevice = currentDevice;
                            lastDeviceId = currentId;

                            Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                            object mObj2 = null;
                            bestDevice.Activate(ref iidManager2, 7, IntPtr.Zero, out mObj2);
                            bestManager = mObj2 as IAudioSessionManager2;

                            Guid iidMeter = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");
                            object meterObj = null;
                            bestDevice.Activate(ref iidMeter, 7, IntPtr.Zero, out meterObj);
                            bestMeter = meterObj as IAudioMeterInformation;
                            
                            Log(string.Format("  -> Device ready. Manager: {0}, Meter: {1}", bestManager != null, bestMeter != null));
                        } else {
                            Marshal.ReleaseComObject(currentDevice);
                        }

                        if (bestManager == null) {
                            System.Threading.Thread.Sleep(1000);
                            continue;
                        }

                        IAudioSessionEnumerator sessionEnum = null;
                        res = bestManager.GetSessionEnumerator(out sessionEnum);
                        if (res == 0 && sessionEnum != null) {
                            int sessionCount;
                            sessionEnum.GetCount(out sessionCount);
                            
                            var activeSessions = new List<SessionInfo>();
                            var sessListJson = new List<string>();

                            for (int s = 0; s < sessionCount; s++) {
                                IAudioSessionControl control = null;
                                sessionEnum.GetSession(s, out control);
                                if (control == null) continue;

                                Guid iidControl2 = new Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d");
                                Guid iidVolume = new Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8");
                                Guid iidMeter = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");

                                IntPtr pUnk = Marshal.GetIUnknownForObject(control);
                                
                                IAudioSessionControl2 control2 = null;
                                ISimpleAudioVolume volume = null;
                                IAudioMeterInformation meter = null;

                                IntPtr pIid2;
                                if (Marshal.QueryInterface(pUnk, ref iidControl2, out pIid2) == 0) {
                                    control2 = Marshal.GetObjectForIUnknown(pIid2) as IAudioSessionControl2;
                                    Marshal.Release(pIid2);
                                }
                                
                                IntPtr pIidVol;
                                if (Marshal.QueryInterface(pUnk, ref iidVolume, out pIidVol) == 0) {
                                    volume = Marshal.GetObjectForIUnknown(pIidVol) as ISimpleAudioVolume;
                                    Marshal.Release(pIidVol);
                                }

                                IntPtr pIidMeter;
                                if (Marshal.QueryInterface(pUnk, ref iidMeter, out pIidMeter) == 0) {
                                    meter = Marshal.GetObjectForIUnknown(pIidMeter) as IAudioMeterInformation;
                                    Marshal.Release(pIidMeter);
                                }

                                Marshal.Release(pUnk);

                                if (control2 != null) {
                                    int pid = -1;
                                    control2.GetProcessId(out pid);
                                    
                                    if (pid > 0) {
                                        float peak = 0;
                                        if (meter != null) meter.GetPeakValue(out peak);
                                        
                                        float vol = 0;
                                        bool muted = false;
                                        if (volume != null) {
                                            volume.GetMasterVolume(out vol);
                                            volume.GetMute(out muted);
                                        }

                                        string processName = "Unknown";
                                        try { 
                                            var p = Process.GetProcessById(pid);
                                            processName = p.ProcessName; 
                                        } catch {}

                                        activeSessions.Add(new SessionInfo { ProcessId = pid, PeakValue = peak });
                                        
                                        sessListJson.Add("{\"pid\":" + pid + ",\"name\":\"" + EscapeJson(processName) + "\",\"volume\":" + vol.ToString("F4", CultureInfo.InvariantCulture) + ",\"muted\":" + (muted?"true":"false") + "}");
                                    }
                                }
                                if (control2 != null) Marshal.ReleaseComObject(control2);
                                if (volume != null) Marshal.ReleaseComObject(volume);
                                if (meter != null) Marshal.ReleaseComObject(meter);
                                Marshal.ReleaseComObject(control);
                            }
                            
                            float masterPeak = 0;
                            if (bestMeter != null) bestMeter.GetPeakValue(out masterPeak);

                            StringBuilder peaksSb = new StringBuilder();
                            peaksSb.Append("{\"type\":\"peaks\",\"master\":" + masterPeak.ToString("F4", CultureInfo.InvariantCulture));
                            peaksSb.Append(",\"sessions\":{");
                            for (int k=0; k<activeSessions.Count; k++) {
                                if (k > 0) peaksSb.Append(",");
                                peaksSb.Append("\"" + activeSessions[k].ProcessId + "\":" + activeSessions[k].PeakValue.ToString("F4", CultureInfo.InvariantCulture));
                            }
                            peaksSb.Append("}}");
                            lock (stdoutLock) {
                                Console.WriteLine(peaksSb.ToString());
                                Console.Out.Flush();
                            }

                            lock(sessionsLock) {
                                lastSessionsJson = "{\"status\":\"ok\",\"sessions\":[" + string.Join(",", sessListJson.ToArray()) + "]}";
                            }
                            
                            Marshal.ReleaseComObject(sessionEnum);
                        }

                        System.Threading.Thread.Sleep(100);
                    } catch (Exception ex) {
                        Log("Loop Error: " + ex.Message);
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Fatal Peak Error: " + ex.ToString());
            }
        }

        static void HandleGetSessions() {
            lock (sessionsLock) {
                lock (stdoutLock) {
                    Console.WriteLine(lastSessionsJson);
                    Console.Out.Flush();
                }
            }
        }

        static void HandleGetMaster() {
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;
                
                object volObj;
                Guid iidVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidVol, 7, IntPtr.Zero, out volObj);
                var volume = (IAudioEndpointVolume)volObj;
                float level;
                bool muted;
                volume.GetMasterVolumeLevelScalar(out level);
                volume.GetMute(out muted);
                lock (stdoutLock) {
                    Console.WriteLine("{\"status\":\"ok\",\"master\":{\"volume\":" + level.ToString("F4", CultureInfo.InvariantCulture) + ",\"muted\":" + (muted?"true":"false") + "}}");
                    Console.Out.Flush();
                }
                
                if (volObj != null) Marshal.ReleaseComObject(volObj);
                if (device != null) Marshal.ReleaseComObject(device);
            } catch (Exception ex) { Log("Master Error: " + ex.Message); }
        }

        static void HandleSetVolume(uint pid, float vol) {
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;

                Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                object mObj2;
                device.Activate(ref iidManager2, 7, IntPtr.Zero, out mObj2);
                IAudioSessionManager2 manager = mObj2 as IAudioSessionManager2;
                
                if (manager != null) {
                    IAudioSessionEnumerator sessionEnum;
                    if (manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                        int sessionCount;
                        sessionEnum.GetCount(out sessionCount);
                        for (int s = 0; s < sessionCount; s++) {
                            IAudioSessionControl control;
                            sessionEnum.GetSession(s, out control);
                            if (control == null) continue;

                            IntPtr pUnk = Marshal.GetIUnknownForObject(control);
                            Guid iidControl2 = new Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d");
                            IntPtr pIid2;
                            if (Marshal.QueryInterface(pUnk, ref iidControl2, out pIid2) == 0) {
                                var control2 = Marshal.GetObjectForIUnknown(pIid2) as IAudioSessionControl2;
                                Marshal.Release(pIid2);
                                if (control2 != null) {
                                    int cPid;
                                    control2.GetProcessId(out cPid);
                                    if (cPid == pid) {
                                        Guid iidVolume = new Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8");
                                        IntPtr pIidVol;
                                        if (Marshal.QueryInterface(pUnk, ref iidVolume, out pIidVol) == 0) {
                                            var volume = Marshal.GetObjectForIUnknown(pIidVol) as ISimpleAudioVolume;
                                            Marshal.Release(pIidVol);
                                            if (volume != null) {
                                                volume.SetMasterVolume(vol, Guid.Empty);
                                                Marshal.ReleaseComObject(volume);
                                            }
                                        }
                                    }
                                    Marshal.ReleaseComObject(control2);
                                }
                            }
                            Marshal.Release(pUnk);
                            Marshal.ReleaseComObject(control);
                        }
                        Marshal.ReleaseComObject(sessionEnum);
                    }
                    Marshal.ReleaseComObject(manager);
                }
                Marshal.ReleaseComObject(device);
            } catch (Exception ex) { Log("SetVolume Error: " + ex.Message); }
        }

        static void HandleSetMasterVolume(float vol) {
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;

                object volObj;
                Guid iidVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidVol, 7, IntPtr.Zero, out volObj);
                var volume = (IAudioEndpointVolume)volObj;
                volume.SetMasterVolumeLevelScalar(vol, Guid.Empty);

                if (volObj != null) Marshal.ReleaseComObject(volObj);
                if (device != null) Marshal.ReleaseComObject(device);
            } catch {}
        }

        static void HandleToggleMute(uint pid) {
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                IMMDevice device;
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;

                Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                object mObj2;
                device.Activate(ref iidManager2, 7, IntPtr.Zero, out mObj2);
                IAudioSessionManager2 manager = mObj2 as IAudioSessionManager2;
                
                if (manager != null) {
                    IAudioSessionEnumerator sessionEnum;
                    if (manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                        int sessionCount;
                        sessionEnum.GetCount(out sessionCount);
                        for (int s = 0; s < sessionCount; s++) {
                            IAudioSessionControl control;
                            sessionEnum.GetSession(s, out control);
                            if (control == null) continue;

                            IntPtr pUnk = Marshal.GetIUnknownForObject(control);
                            Guid iidControl2 = new Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d");
                            IntPtr pIid2;
                            if (Marshal.QueryInterface(pUnk, ref iidControl2, out pIid2) == 0) {
                                var control2 = Marshal.GetObjectForIUnknown(pIid2) as IAudioSessionControl2;
                                Marshal.Release(pIid2);
                                if (control2 != null) {
                                    int cPid;
                                    control2.GetProcessId(out cPid);
                                    if (cPid == pid) {
                                        Guid iidVolume = new Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8");
                                        IntPtr pIidVol;
                                        if (Marshal.QueryInterface(pUnk, ref iidVolume, out pIidVol) == 0) {
                                            var volume = Marshal.GetObjectForIUnknown(pIidVol) as ISimpleAudioVolume;
                                            Marshal.Release(pIidVol);
                                            if (volume != null) {
                                                bool currentMute;
                                                volume.GetMute(out currentMute);
                                                volume.SetMute(!currentMute, Guid.Empty);
                                                Marshal.ReleaseComObject(volume);
                                            }
                                        }
                                    }
                                    Marshal.ReleaseComObject(control2);
                                }
                            }
                            Marshal.Release(pUnk);
                            Marshal.ReleaseComObject(control);
                        }
                        Marshal.ReleaseComObject(sessionEnum);
                    }
                    Marshal.ReleaseComObject(manager);
                }
                Marshal.ReleaseComObject(device);
            } catch (Exception ex) { Log("ToggleMute Error: " + ex.Message); }
        }

        static string EscapeJson(string s) {
            if (s == null) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
