using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.IO;
using System.Threading;

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

    [ComImport, Guid("1CB9AD4C-DBA4-4c53-9D54-6451E8F752BF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioClient {
        [PreserveSig] int Initialize(int shareMode, int streamFlags, long hnsBufferDuration, long hnsPeriodicity, [In] WaveFormat pFormat, [In] ref Guid audioSessionGuid);
        [PreserveSig] int GetBufferSize(out uint bufferSize);
        [PreserveSig] int GetStreamLatency(out long hnsLatency);
        [PreserveSig] int GetCurrentPadding(out uint numPaddingFrames);
        [PreserveSig] int IsFormatSupported(int shareMode, [In] WaveFormat pFormat, out IntPtr ppClosestMatch);
        [PreserveSig] int GetMixFormat(out IntPtr ppDeviceFormat);
        [PreserveSig] int GetDevicePeriod(out long phnsDefaultDevicePeriod, out long phnsMinimumDevicePeriod);
        [PreserveSig] int Start();
        [PreserveSig] int Stop();
        [PreserveSig] int Reset();
        [PreserveSig] int SetEventHandle(IntPtr eventHandle);
        [PreserveSig] int GetService([In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
    }

    [ComImport, Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD19F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioCaptureClient {
        [PreserveSig] int GetBuffer(out IntPtr ppData, out uint pNumFramesToRead, out int pdwFlags, out long pu64DevicePosition, out long pu64QPCPosition);
        [PreserveSig] int ReleaseBuffer(uint numFramesRead);
        [PreserveSig] int GetNextPacketSize(out uint pNumFramesInNextPacket);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class WaveFormat {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;

        public static WaveFormat CreateIeeeFloat(int sampleRate, int channels) {
            WaveFormat wf = new WaveFormat();
            wf.wFormatTag = 3; // WAVE_FORMAT_IEEE_FLOAT
            wf.nChannels = (short)channels;
            wf.nSamplesPerSec = sampleRate;
            wf.wBitsPerSample = 32;
            wf.nBlockAlign = (short)(channels * 4);
            wf.nAvgBytesPerSec = wf.nSamplesPerSec * wf.nBlockAlign;
            wf.cbSize = 0;
            return wf;
        }
    }

    class SessionInfo {
        public int ProcessId;
        public float PeakValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AUDIOCLIENT_ACTIVATION_PARAMS {
        public int ActivationParamsType; // 1 = Process Loopback
        public AUDIOCLIENT_PROCESS_LOOPBACK_PARAMS ProcessLoopbackParams;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AUDIOCLIENT_PROCESS_LOOPBACK_PARAMS {
        public uint TargetProcessId;
        public int ProcessLoopbackMode; // 0 = Include, 1 = Exclude
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
        static bool globalBoostActive = false;
        static float boostReductionFactor = 0.6f; // Reduce others to 60% when boost is on

        static Dictionary<int, RecordingSession> activeRecordings = new Dictionary<int, RecordingSession>();

        class RecordingSession {
            public int Pid;
            public string FilePath;
            private volatile bool running = false;
            private System.Threading.Thread thread;

            public void Start() {
                running = true;
                thread = new System.Threading.Thread(RecordLoop);
                thread.SetApartmentState(System.Threading.ApartmentState.MTA);
                thread.Start();
            }

            public void Stop() {
                running = false;
                if (thread != null) thread.Join(1000);
            }

            private void RecordLoop() {
                IMMDevice device = null;
                IAudioClient audioClient = null;
                IAudioCaptureClient captureClient = null;
                System.IO.FileStream fs = null;
                IntPtr formatPtr = IntPtr.Zero;

                try {
                    var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                    deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                    if (device == null) return;

                    // Activate with Process Loopback
                    var activationParams = new AUDIOCLIENT_ACTIVATION_PARAMS();
                    activationParams.ActivationParamsType = 1; // PROCESS_LOOPBACK
                    activationParams.ProcessLoopbackParams.TargetProcessId = (uint)Pid;
                    activationParams.ProcessLoopbackParams.ProcessLoopbackMode = 0; // INCLUDE

                    IntPtr pParams = Marshal.AllocHGlobal(Marshal.SizeOf(activationParams));
                    Marshal.StructureToPtr(activationParams, pParams, false);

                    Guid iidAudioClient = new Guid("1CB9AD4C-DBA4-4c53-9D54-6451E8F752BF");
                    object clientObj;
                    // Note: Use 0x01 (CLSCTX_ALL) and pass pParams
                    device.Activate(ref iidAudioClient, 1, pParams, out clientObj);
                    Marshal.FreeHGlobal(pParams);

                    audioClient = clientObj as IAudioClient;
                    if (audioClient == null) return;

                    audioClient.GetMixFormat(out formatPtr);
                    WaveFormat format = (WaveFormat)Marshal.PtrToStructure(formatPtr, typeof(WaveFormat));

                    // Initialize in Loopback mode (0x00020000 = AUDCLNT_STREAMFLAGS_LOOPBACK)
                    Guid sessionGuid = Guid.Empty;
                    audioClient.Initialize(0, 0x00020000, 10000000, 0, format, ref sessionGuid);

                    uint bufferSize;
                    audioClient.GetBufferSize(out bufferSize);

                    object captureObj;
                    Guid iidCapture = new Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD19F");
                    audioClient.GetService(iidCapture, out captureObj);
                    captureClient = captureObj as IAudioCaptureClient;

                    // Prepare WAV file
                    string dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recordings");
                    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                    FilePath = System.IO.Path.Combine(dir, "Record_" + Pid + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav");
                    
                    fs = new System.IO.FileStream(FilePath, System.IO.FileMode.Create);
                    // Placeholder for WAV header (44 bytes)
                    byte[] emptyHeader = new byte[44];
                    fs.Write(emptyHeader, 0, 44);

                    uint totalBytes = 0;
                    audioClient.Start();

                    while (running) {
                        uint nextPacketSize;
                        captureClient.GetNextPacketSize(out nextPacketSize);
                        while (nextPacketSize > 0) {
                            IntPtr pData;
                            uint numFramesRead;
                            int flags;
                            long pos, qpc;
                            captureClient.GetBuffer(out pData, out numFramesRead, out flags, out pos, out qpc);
                            
                            if (numFramesRead > 0) {
                                int bytesRead = (int)(numFramesRead * format.nBlockAlign);
                                byte[] buffer = new byte[bytesRead];
                                Marshal.Copy(pData, buffer, 0, bytesRead);
                                fs.Write(buffer, 0, bytesRead);
                                totalBytes += (uint)bytesRead;
                            }
                            captureClient.ReleaseBuffer(numFramesRead);
                            captureClient.GetNextPacketSize(out nextPacketSize);
                        }
                        System.Threading.Thread.Sleep(10);
                    }

                    audioClient.Stop();

                    // Update WAV header
                    fs.Position = 0;
                    WriteWavHeader(fs, totalBytes, format);
                    fs.Close();

                    Log("Saved recording to: " + FilePath);
                } catch (Exception ex) {
                    Log("Recording Error (PID " + Pid + "): " + ex.Message);
                } finally {
                    if (formatPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(formatPtr);
                    if (captureClient != null) Marshal.ReleaseComObject(captureClient);
                    if (audioClient != null) Marshal.ReleaseComObject(audioClient);
                    if (device != null) Marshal.ReleaseComObject(device);
                    if (fs != null) fs.Dispose();
                }
            }

            private void WriteWavHeader(System.IO.FileStream fs, uint dataSize, WaveFormat format) {
                var writer = new System.IO.BinaryWriter(fs);
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(dataSize + 36);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // subchunk1size
                writer.Write(format.wFormatTag);
                writer.Write(format.nChannels);
                writer.Write(format.nSamplesPerSec);
                writer.Write(format.nAvgBytesPerSec);
                writer.Write(format.nBlockAlign);
                writer.Write(format.wBitsPerSample);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);
            }
        }

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
                        case "start_recording":
                            if (dict.ContainsKey("pid")) {
                                int rPid = Convert.ToInt32(dict["pid"]);
                                HandleStartRecording(rPid, requestId);
                            }
                            break;
                        case "stop_recording":
                            if (dict.ContainsKey("pid")) {
                                int sPid = Convert.ToInt32(dict["pid"]);
                                HandleStopRecording(sPid, requestId);
                            }
                            break;
                        case "set_boost":
                            if (dict.ContainsKey("active")) {
                                bool bActive = Convert.ToBoolean(dict["active"]);
                                HandleSetBoost(bActive, requestId);
                            }
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
                                                        // We need the "unfaded" volume to apply the fade correctly.
                                                        // For now, we assume currentVol is what it is, but this can cause downward spirals.
                                                        // Better: only store baseVolumes when targetFade is 1.0
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
            // Update base volume even if ducking
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
                                if (control == null) continue;
                                IAudioSessionControl2 control2 = control as IAudioSessionControl2;
                                if (control2 != null) {
                                    int cPid;
                                    control2.GetProcessId(out cPid);
                                    if (cPid == pid) {
                                        ISimpleAudioVolume volume = control as ISimpleAudioVolume;
                                        if (volume != null) {
                                            // Apply actual volume based on current fade and boost
                                            float fade = currentFades.ContainsKey(pid) ? currentFades[pid] : 1.0f;
                                            float boost = globalBoostActive ? boostReductionFactor : 1.0f;
                                            volume.SetMasterVolume(vol * fade * boost, Guid.Empty);
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

        static void HandleSetBoost(bool active, string requestId) {
            globalBoostActive = active;
            Log("Smart Overdrive " + (active ? "Enabled" : "Disabled"));

            // Refresh all volumes immediately
            IMMDevice device = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnum = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                if (deviceEnum.GetDefaultAudioEndpoint(0, 0, out device) == 0 && device != null) {
                    Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                    object mObj2;
                    device.Activate(ref iidManager2, 7, IntPtr.Zero, out mObj2);
                    manager = mObj2 as IAudioSessionManager2;
                    if (manager != null && manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                        int count;
                        sessionEnum.GetCount(out count);
                        for (int i = 0; i < count; i++) {
                            IAudioSessionControl control = null;
                            try {
                                sessionEnum.GetSession(i, out control);
                                var control2 = control as IAudioSessionControl2;
                                if (control2 != null) {
                                    int pid;
                                    control2.GetProcessId(out pid);
                                    if (baseVolumes.ContainsKey(pid)) {
                                        float vol = baseVolumes[pid];
                                        float fade = currentFades.ContainsKey(pid) ? currentFades[pid] : 1.0f;
                                        float boost = globalBoostActive ? boostReductionFactor : 1.0f;
                                        ISimpleAudioVolume sav = control as ISimpleAudioVolume;
                                        if (sav != null) {
                                            sav.SetMasterVolume(vol * fade * boost, Guid.Empty);
                                        }
                                    }
                                }
                            } finally { if (control != null) Marshal.ReleaseComObject(control); }
                        }
                    }
                }
            } catch (Exception ex) { Log("Boost Apply Error: " + ex.Message); }
            finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                if (manager != null) Marshal.ReleaseComObject(manager);
                if (device != null) Marshal.ReleaseComObject(device);
            }

            if (requestId != null) {
                lock (stdoutLock) {
                    Console.WriteLine("{\"status\":\"ok\",\"requestId\":\"" + requestId + "\"}");
                    Console.Out.Flush();
                }
            }
        }

        static void HandleStartRecording(int pid, string requestId) {
            lock (activeRecordings) {
                if (activeRecordings.ContainsKey(pid)) {
                    activeRecordings[pid].Stop();
                    activeRecordings.Remove(pid);
                }
                var session = new RecordingSession { Pid = pid };
                activeRecordings[pid] = session;
                session.Start();
                Log("Recording started for PID " + pid);
            }
            if (requestId != null) {
                lock (stdoutLock) {
                    Console.WriteLine("{\"status\":\"ok\",\"requestId\":\"" + requestId + "\"}");
                    Console.Out.Flush();
                }
            }
        }

        static void HandleStopRecording(int pid, string requestId) {
            lock (activeRecordings) {
                if (activeRecordings.ContainsKey(pid)) {
                    activeRecordings[pid].Stop();
                    activeRecordings.Remove(pid);
                    Log("Recording stopped for PID " + pid);
                }
            }
            if (requestId != null) {
                lock (stdoutLock) {
                    Console.WriteLine("{\"status\":\"ok\",\"requestId\":\"" + requestId + "\"}");
                    Console.Out.Flush();
                }
            }
        }
    }
}
