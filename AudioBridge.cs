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

    class DuckingSettings {
        public bool Enabled = false;
        public int TriggerPid = -1;
        public float Threshold = 0.05f;
        public float Factor = 0.2f;
        public float FadeSpeed = 0.05f; // Per poll (100ms)
    }

    class AudioBridge
    {
        static string lastSessionsJson = "{\"status\":\"ok\",\"sessions\":[]}";
        static readonly object sessionsLock = new object();
        static readonly object stdoutLock = new object();
        static volatile bool shouldExit = false;

        static DuckingSettings duckSettings = new DuckingSettings();
        static Dictionary<int, float> baseVolumes = new Dictionary<int, float>();
        static Dictionary<int, float> currentFades = new Dictionary<int, float>(); // 1.0 = normal, <1.0 = ducked

        static void Log(string msg) {
            try {
                string logFile = "bridge_debug.log";
                if (System.IO.File.Exists(logFile)) {
                    var info = new System.IO.FileInfo(logFile);
                    if (info.Length > 1024 * 1024) { // 1MB Limit
                        System.IO.File.Delete(logFile);
                    }
                }
                System.IO.File.AppendAllText(logFile, DateTime.Now.ToString("HH:mm:ss.fff") + " - " + msg + "\r\n");
            } catch {}
        }

        static void Main(string[] args)
        {
            Log("Bridge starting (V1.5.1 - Auto-Duck Support)...");
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
                    
                    string requestId = dict.ContainsKey("requestId") ? dict["requestId"].ToString() : null;
                    string action = dict["action"].ToString();

                    switch (action) {
                        case "get_sessions": HandleGetSessions(requestId); break;
                        case "get_master": HandleGetMaster(requestId); break;
                        case "set_volume":
                            if (dict.ContainsKey("pid") && dict.ContainsKey("volume")) {
                                int pid = Convert.ToInt32(dict["pid"]);
                                if (dict["volume"] != null) {
                                    float vol = Convert.ToSingle(dict["volume"], CultureInfo.InvariantCulture);
                                    HandleSetVolume(pid, vol, requestId);
                                }
                            }
                            break;
                        case "set_master_volume":
                            if (dict.ContainsKey("volume") && dict["volume"] != null) {
                                float mvol = Convert.ToSingle(dict["volume"], CultureInfo.InvariantCulture);
                                HandleSetMasterVolume(mvol, requestId);
                            }
                            break;
                        case "toggle_mute":
                            if (dict.ContainsKey("pid")) {
                                int pidMute = Convert.ToInt32(dict["pid"]);
                                HandleToggleMute(pidMute, requestId);
                            }
                            break;
                        case "set_ducking":
                            HandleSetDucking(dict, requestId);
                            break;
                        case "ping": 
                            lock (stdoutLock) {
                                string resp = "{\"status\":\"pong\"" + (requestId != null ? ",\"requestId\":\"" + requestId + "\"" : "") + "}";
                                Console.WriteLine(resp); 
                                Console.Out.Flush(); 
                            }
                            break;
                        case "exit": 
                            shouldExit = true;
                            if (peakThread.Join(1500)) Log("Peak thread joined.");
                            return;
                    }
                } catch (Exception ex) {
                    Log("Action Error: " + ex.Message);
                }
            }
            Log("Stdin closed, exiting.");
            shouldExit = true;
            peakThread.Join(1000);
        }

        static void HandleSetDucking(Dictionary<string, object> dict, string requestId) {
            try {
                if (dict.ContainsKey("enabled")) duckSettings.Enabled = Convert.ToBoolean(dict["enabled"]);
                if (dict.ContainsKey("triggerPid")) duckSettings.TriggerPid = Convert.ToInt32(dict["triggerPid"]);
                if (dict.ContainsKey("threshold")) duckSettings.Threshold = Convert.ToSingle(dict["threshold"], CultureInfo.InvariantCulture);
                if (dict.ContainsKey("factor")) duckSettings.Factor = Convert.ToSingle(dict["factor"], CultureInfo.InvariantCulture);
                
                if (requestId != null) {
                    lock (stdoutLock) {
                        Console.WriteLine("{\"status\":\"ok\",\"requestId\":\"" + requestId + "\"}");
                        Console.Out.Flush();
                    }
                }
            } catch (Exception ex) { Log("SetDucking Error: " + ex.Message); }
        }

        static void PeakPollingLoop()
        {
            Log("Peak thread started (V1.5.1)");
            string lastDeviceId = null;
            IMMDevice bestDevice = null;
            IAudioSessionManager2 bestManager = null;
            IAudioMeterInformation bestMeter = null;

            try
            {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                while (!shouldExit)
                {
                    try
                    {
                        IMMDevice currentDevice = null;
                        int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out currentDevice);
                        if (res != 0 || currentDevice == null) {
                            System.Threading.Thread.Sleep(1000);
                            continue;
                        }

                        string currentId;
                        currentDevice.GetId(out currentId);

                        if (currentId != lastDeviceId) {
                            Log("Switching to device: " + currentId);
                            
                            var oldMeter = bestMeter;
                            var oldManager = bestManager;
                            var oldDevice = bestDevice;

                            bestMeter = null;
                            bestManager = null;
                            bestDevice = null;
                            lastDeviceId = null; 

                            try {
                                Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                                object mObj2 = null;
                                currentDevice.Activate(ref iidManager2, 7, IntPtr.Zero, out mObj2);
                                bestManager = mObj2 as IAudioSessionManager2;

                                Guid iidMeter = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");
                                object meterObj = null;
                                currentDevice.Activate(ref iidMeter, 7, IntPtr.Zero, out meterObj);
                                bestMeter = meterObj as IAudioMeterInformation;

                                bestDevice = currentDevice;
                                lastDeviceId = currentId;
                            } catch (Exception ex) {
                                Log("Device activation failed: " + ex.Message);
                                Marshal.ReleaseComObject(currentDevice);
                            }

                            if (oldMeter != null) Marshal.ReleaseComObject(oldMeter);
                            if (oldManager != null) Marshal.ReleaseComObject(oldManager);
                            if (oldDevice != null) Marshal.ReleaseComObject(oldDevice);
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
                            try {
                                int sessionCount;
                                sessionEnum.GetCount(out sessionCount);
                                
                                var activeSessions = new List<SessionInfo>();
                                var sessListJson = new List<string>();
                                
                                // 1. Find if Trigger is active
                                bool triggerAboveThreshold = false;
                                if (duckSettings.Enabled && duckSettings.TriggerPid > 0) {
                                    for (int s = 0; s < sessionCount; s++) {
                                        IAudioSessionControl control = null;
                                        try {
                                            sessionEnum.GetSession(s, out control);
                                            IAudioSessionControl2 c2 = control as IAudioSessionControl2;
                                            if (c2 != null) {
                                                int pid;
                                                c2.GetProcessId(out pid);
                                                if (pid == duckSettings.TriggerPid) {
                                                    IAudioMeterInformation meter = control as IAudioMeterInformation;
                                                    if (meter != null) {
                                                        float peak;
                                                        meter.GetPeakValue(out peak);
                                                        if (peak >= duckSettings.Threshold) triggerAboveThreshold = true;
                                                    }
                                                }
                                            }
                                        } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                                        if (triggerAboveThreshold) break;
                                    }
                                }

                                // 2. Process all sessions
                                for (int s = 0; s < sessionCount; s++) {
                                    IAudioSessionControl control = null;
                                    try {
                                        sessionEnum.GetSession(s, out control);
                                        if (control == null) continue;

                                        IAudioSessionControl2 control2 = control as IAudioSessionControl2;
                                        ISimpleAudioVolume volume = control as ISimpleAudioVolume;
                                        IAudioMeterInformation sessMeter = control as IAudioMeterInformation;

                                        if (control2 != null && volume != null) {
                                            int pid = -1;
                                            control2.GetProcessId(out pid);
                                            if (pid > 0) {
                                                float peak = 0;
                                                if (sessMeter != null) sessMeter.GetPeakValue(out peak);
                                                
                                                float currentVol;
                                                bool muted;
                                                volume.GetMasterVolume(out currentVol);
                                                volume.GetMute(out muted);

                                                // Update base volume (only if we are NOT ducking or if user changed it)
                                                if (!currentFades.ContainsKey(pid)) currentFades[pid] = 1.0f;
                                                
                                                float targetFade = (duckSettings.Enabled && triggerAboveThreshold && pid != duckSettings.TriggerPid) 
                                                    ? duckSettings.Factor 
                                                    : 1.0f;

                                                // Smooth fade logic
                                                if (currentFades[pid] != targetFade) {
                                                    if (currentFades[pid] < targetFade) 
                                                        currentFades[pid] = Math.Min(targetFade, currentFades[pid] + duckSettings.FadeSpeed);
                                                    else 
                                                        currentFades[pid] = Math.Max(targetFade, currentFades[pid] - duckSettings.FadeSpeed);

                                                    // Apply volume if not muted
                                                    if (!muted) {
                                                        if (targetFade == 1.0f && currentFades[pid] == 1.0f) {
                                                            baseVolumes[pid] = currentVol;
                                                        }
                                                        
                                                        float baseVol = baseVolumes.ContainsKey(pid) ? baseVolumes[pid] : currentVol;
                                                        volume.SetMasterVolume(baseVol * currentFades[pid], Guid.Empty);
                                                    }
                                                } else if (targetFade == 1.0f) {
                                                    baseVolumes[pid] = currentVol;
                                                }

                                                string processName = "Unknown";
                                                try { 
                                                    using (var p = Process.GetProcessById(pid)) {
                                                        processName = p.ProcessName;
                                                        if (!string.IsNullOrEmpty(processName)) processName = char.ToUpper(processName[0]) + processName.Substring(1);
                                                    }
                                                } catch {}

                                                activeSessions.Add(new SessionInfo { ProcessId = pid, PeakValue = peak });
                                                var serializer = new JavaScriptSerializer();
                                                sessListJson.Add("{\"pid\":" + pid + ",\"name\":" + serializer.Serialize(processName) + ",\"volume\":" + currentVol.ToString("F4", CultureInfo.InvariantCulture) + ",\"muted\":" + (muted?"true":"false") + "}");
                                            }
                                        }
                                    } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
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
                            } finally {
                                Marshal.ReleaseComObject(sessionEnum);
                            }
                        }

                        System.Threading.Thread.Sleep(100);
                    } catch (Exception ex) {
                        Log("Loop Error: " + ex.Message);
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex) { Log("Fatal Peak Error: " + ex.ToString()); }
            finally {
                if (bestMeter != null) Marshal.ReleaseComObject(bestMeter);
                if (bestManager != null) Marshal.ReleaseComObject(bestManager);
                if (bestDevice != null) Marshal.ReleaseComObject(bestDevice);
            }
        }

        static void HandleGetSessions(string requestId) {
            lock (sessionsLock) {
                lock (stdoutLock) {
                    string json = lastSessionsJson;
                    if (requestId != null) json = json.Substring(0, json.Length - 1) + ",\"requestId\":\"" + requestId + "\"}";
                    Console.WriteLine(json);
                    Console.Out.Flush();
                }
            }
        }

        static void HandleGetMaster(string requestId) {
            IMMDevice device = null;
            object volObj = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;
                
                Guid iidVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidVol, 7, IntPtr.Zero, out volObj);
                var volume = (IAudioEndpointVolume)volObj;
                float level;
                bool muted;
                volume.GetMasterVolumeLevelScalar(out level);
                volume.GetMute(out muted);
                lock (stdoutLock) {
                    string resp = "{\"status\":\"ok\",\"master\":{\"volume\":" + level.ToString("F4", CultureInfo.InvariantCulture) + ",\"muted\":" + (muted?"true":"false") + "}";
                    if (requestId != null) resp += ",\"requestId\":\"" + requestId + "\"";
                    resp += "}";
                    Console.WriteLine(resp);
                    Console.Out.Flush();
                }
            } catch (Exception ex) { Log("Master Error: " + ex.Message); } 
            finally {
                if (volObj != null) Marshal.ReleaseComObject(volObj);
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }

        static void HandleSetVolume(int pid, float vol, string requestId) {
            baseVolumes[pid] = vol;
            IMMDevice device = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnum = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;
                Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                object mObj2;
                device.Activate(ref iidManager2, 7, IntPtr.Zero, out mObj2);
                manager = mObj2 as IAudioSessionManager2;
                if (manager != null) {
                    if (manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                        int sessionCount;
                        sessionEnum.GetCount(out sessionCount);
                        for (int s = 0; s < sessionCount; s++) {
                            IAudioSessionControl control = null;
                            try {
                                sessionEnum.GetSession(s, out control);
                                IAudioSessionControl2 control2 = control as IAudioSessionControl2;
                                if (control2 != null) {
                                    int cPid;
                                    control2.GetProcessId(out cPid);
                                    if (cPid == pid) {
                                        ISimpleAudioVolume volume = control as ISimpleAudioVolume;
                                        if (volume != null) {
                                            float fade = currentFades.ContainsKey(pid) ? currentFades[pid] : 1.0f;
                                            volume.SetMasterVolume(vol * fade, Guid.Empty);
                                        }
                                    }
                                }
                            } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                        }
                    }
                }
                if (requestId != null) {
                    lock (stdoutLock) {
                        Console.WriteLine("{\"status\":\"ok\",\"requestId\":\"" + requestId + "\"}");
                        Console.Out.Flush();
                    }
                }
            } catch (Exception ex) { Log("SetVolume Error: " + ex.Message); }
            finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                if (manager != null) Marshal.ReleaseComObject(manager);
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }

        static void HandleSetMasterVolume(float vol, string requestId) {
            IMMDevice device = null;
            object volObj = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;
                Guid iidVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidVol, 7, IntPtr.Zero, out volObj);
                var volume = (IAudioEndpointVolume)volObj;
                volume.SetMasterVolumeLevelScalar(vol, Guid.Empty);
                if (requestId != null) {
                    lock (stdoutLock) {
                        Console.WriteLine("{\"status\":\"ok\",\"requestId\":\"" + requestId + "\"}");
                        Console.Out.Flush();
                    }
                }
            } catch {}
            finally {
                if (volObj != null) Marshal.ReleaseComObject(volObj);
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }

        static void HandleToggleMute(int pid, string requestId) {
            IMMDevice device = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnum = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) return;
                Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                object mObj2;
                device.Activate(ref iidManager2, 7, IntPtr.Zero, out mObj2);
                manager = mObj2 as IAudioSessionManager2;
                if (manager != null) {
                    if (manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                        int sessionCount;
                        sessionEnum.GetCount(out sessionCount);
                        for (int s = 0; s < sessionCount; s++) {
                            IAudioSessionControl control = null;
                            try {
                                sessionEnum.GetSession(s, out control);
                                IAudioSessionControl2 control2 = control as IAudioSessionControl2;
                                if (control2 != null) {
                                    int cPid;
                                    control2.GetProcessId(out cPid);
                                    if (cPid == pid) {
                                        ISimpleAudioVolume volume = control as ISimpleAudioVolume;
                                        if (volume != null) {
                                            bool currentMute;
                                            volume.GetMute(out currentMute);
                                            volume.SetMute(!currentMute, Guid.Empty);
                                        }
                                    }
                                }
                            } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                        }
                    }
                }
                if (requestId != null) {
                    lock (stdoutLock) {
                        Console.WriteLine("{\"status\":\"ok\",\"requestId\":\"" + requestId + "\"}");
                        Console.Out.Flush();
                    }
                }
            } catch (Exception ex) { Log("ToggleMute Error: " + ex.Message); }
            finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                if (manager != null) Marshal.ReleaseComObject(manager);
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }
    }
}
