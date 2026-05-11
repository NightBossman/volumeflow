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

    // IAudioClient — Initialize i IsFormatSupported przyjmują IntPtr, dzięki czemu
    // przekazujemy oryginalny wskaźnik z GetMixFormat bez utraty rozszerzonej części
    // (WAVEFORMATEXTENSIBLE ma 40 bajtów; managed klasa WaveFormat tylko 18 — to powodowało
    // niezdefiniowane zachowanie WASAPI przy Initialize).
    [ComImport, Guid("1CB9AD4C-DBA4-4c53-9D54-6451E8F752BF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioClient {
        [PreserveSig] int Initialize(int shareMode, int streamFlags, long hnsBufferDuration, long hnsPeriodicity, IntPtr pFormat, [In] ref Guid audioSessionGuid);
        [PreserveSig] int GetBufferSize(out uint bufferSize);
        [PreserveSig] int GetStreamLatency(out long hnsLatency);
        [PreserveSig] int GetCurrentPadding(out uint numPaddingFrames);
        [PreserveSig] int IsFormatSupported(int shareMode, IntPtr pFormat, out IntPtr ppClosestMatch);
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

    // Process loopback (Windows 10 2004+) — przez ActivateAudioInterfaceAsync z Mmdevapi.dll
    [ComImport, Guid("41D949AB-9862-444A-80F6-C261334DA5EB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IActivateAudioInterfaceCompletionHandler {
        [PreserveSig] int ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation);
    }

    [ComImport, Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IActivateAudioInterfaceAsyncOperation {
        [PreserveSig] int GetActivateResult(out int activateResult, [MarshalAs(UnmanagedType.IUnknown)] out object activatedInterface);
    }

    // WaveFormat (zostawiona dla zgodności, choć nie używamy jej już bezpośrednio do Initialize)
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class WaveFormat {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct WaveFormatExtensible {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;
        public short wValidBitsPerSample;
        public int dwChannelMask;
        public Guid SubFormat;
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
        public int ProcessLoopbackMode; // 0 = INCLUDE_PROCESS_TREE, 1 = EXCLUDE_PROCESS_TREE
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPVARIANT {
        public short vt;
        public short wReserved1;
        public short wReserved2;
        public short wReserved3;
        public int cbBlob;       // dla VT_BLOB
        public IntPtr pBlobData;
        public IntPtr padding;
    }

    public static class MmDevApi {
        public const string VIRTUAL_AUDIO_DEVICE_PROCESS_LOOPBACK = "VAD\\Process_Loopback";

        [DllImport("Mmdevapi.dll", ExactSpelling = true, PreserveSig = false)]
        public static extern IActivateAudioInterfaceAsyncOperation ActivateAudioInterfaceAsync(
            [In, MarshalAs(UnmanagedType.LPWStr)] string deviceInterfacePath,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            IntPtr activationParams,
            IActivateAudioInterfaceCompletionHandler completionHandler);
    }

    public static class Kernel32 {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        public const uint WAIT_OBJECT_0 = 0;
        public const uint WAIT_TIMEOUT  = 0x102;
    }

    class ActivationHandler : IActivateAudioInterfaceCompletionHandler {
        private readonly ManualResetEventSlim _done = new ManualResetEventSlim(false);
        public int ActivateCompleted(IActivateAudioInterfaceAsyncOperation activateOperation) { _done.Set(); return 0; }
        public bool Wait(int timeoutMs) { return _done.Wait(timeoutMs); }
    }

    // ============================================================
    // StdoutWriter — producent/konsument z polityką drop dla peak frames
    // Wątek logiki (peak loop, handlery) NIGDY nie blokuje się na zatkanym pipe.
    // ============================================================
    static class StdoutWriter {
        private static readonly Queue<string> responses = new Queue<string>();
        private static string latestPeak = null;   // drop policy: zostaw tylko najnowszy peak
        private static readonly object lockObj = new object();
        private static Thread writer;
        private static volatile bool running = false;
        private static int droppedPeaks = 0;
        private static int droppedResponses = 0;
        private const int MAX_RESPONSE_QUEUE = 200;

        public static void Start() {
            running = true;
            writer = new Thread(WriteLoop);
            writer.IsBackground = true;
            writer.Name = "StdoutWriter";
            writer.Start();
        }

        public static void EnqueueResponse(string json) {
            lock (lockObj) {
                if (responses.Count >= MAX_RESPONSE_QUEUE) {
                    responses.Dequeue();
                    droppedResponses++;
                }
                responses.Enqueue(json);
                Monitor.Pulse(lockObj);
            }
        }

        public static void EnqueuePeak(string json) {
            lock (lockObj) {
                if (latestPeak != null) droppedPeaks++;
                latestPeak = json;
                Monitor.Pulse(lockObj);
            }
        }

        public static int DroppedPeaks { get { lock (lockObj) return droppedPeaks; } }
        public static int DroppedResponses { get { lock (lockObj) return droppedResponses; } }

        private static void WriteLoop() {
            while (running) {
                string msg = null;
                lock (lockObj) {
                    while (running && responses.Count == 0 && latestPeak == null) {
                        Monitor.Wait(lockObj, 500);
                    }
                    if (!running) break;
                    if (responses.Count > 0) {
                        msg = responses.Dequeue();
                    } else if (latestPeak != null) {
                        msg = latestPeak;
                        latestPeak = null;
                    }
                }
                if (msg != null) {
                    try {
                        Console.WriteLine(msg);
                        Console.Out.Flush();
                    } catch (Exception ex) {
                        AudioBridge.Log("Stdout write failed: " + ex.Message);
                    }
                }
            }

            // Drain — przy shutdown zrzuć pozostałe response'y, dropuj peaki
            try {
                lock (lockObj) {
                    while (responses.Count > 0) {
                        try { Console.WriteLine(responses.Dequeue()); } catch {}
                    }
                    try { Console.Out.Flush(); } catch {}
                }
            } catch {}
        }

        public static void Shutdown(int timeoutMs = 2000) {
            running = false;
            lock (lockObj) Monitor.PulseAll(lockObj);
            if (writer != null) writer.Join(timeoutMs);
        }
    }

    // ============================================================
    // AudioBridge
    // ============================================================

    class DuckingSettings {
        public bool Enabled = false;
        public int TriggerPid = -1;
        public float Threshold = 0.05f;
        public float Factor = 0.2f;
        public float FadeSpeed = 0.05f;
    }

    class AudioBridge
    {
        // --- stałe ---
        const int MAX_CONCURRENT_RECORDINGS = 8;
        const int ZOMBIE_CLEANUP_EVERY_N_TICKS = 50; // ~5 sekund przy 100ms
        static readonly Guid KSDATAFORMAT_SUBTYPE_PCM        = new Guid("00000001-0000-0010-8000-00aa00389b71");
        static readonly Guid KSDATAFORMAT_SUBTYPE_IEEE_FLOAT = new Guid("00000003-0000-0010-8000-00aa00389b71");

        // --- I/O state ---
        static string lastSessionsJson = "{\"status\":\"ok\",\"sessions\":[]}";
        static readonly object sessionsLock = new object();
        static readonly object logLock = new object();
        static volatile bool shouldExit = false;
        static readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        // --- shared state (synchronizowane stateLock) ---
        static readonly object stateLock = new object();
        static DuckingSettings duckSettings = new DuckingSettings();
        static Dictionary<int, float> baseVolumes = new Dictionary<int, float>();
        static Dictionary<int, float> currentFades = new Dictionary<int, float>();
        static volatile bool globalBoostActive = false;
        static float boostReductionFactor = 0.6f;
        static Dictionary<int, string> processNameCache = new Dictionary<int, string>();

        static readonly object recordingsLock = new object();
        static Dictionary<int, RecordingSession> activeRecordings = new Dictionary<int, RecordingSession>();

        // ============================================================
        // RecordingSession (event-driven + fallback timer)
        // ============================================================
        class RecordingSession {
            public int Pid;
            public string FilePath;
            private volatile bool running = false;
            private Thread thread;
            private ActivationHandler activationHandler;

            public void Start() {
                running = true;
                thread = new Thread(RecordLoop);
                thread.IsBackground = true;
                thread.Name = "Record_" + Pid;
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
            }

            public void Stop() {
                running = false;
                if (thread != null) thread.Join(3000);
            }

            private void RecordLoop() {
                IAudioClient audioClient = null;
                IAudioCaptureClient captureClient = null;
                FileStream fs = null;
                IntPtr formatPtr = IntPtr.Zero;
                IntPtr pParams = IntPtr.Zero;
                IntPtr pPropVariant = IntPtr.Zero;
                IntPtr eventHandle = IntPtr.Zero;
                bool clientStarted = false;

                try {
                    // 1. AUDIOCLIENT_ACTIVATION_PARAMS opakowane w PROPVARIANT (VT_BLOB)
                    var activationParams = new AUDIOCLIENT_ACTIVATION_PARAMS {
                        ActivationParamsType = 1
                    };
                    activationParams.ProcessLoopbackParams.TargetProcessId = (uint)Pid;
                    activationParams.ProcessLoopbackParams.ProcessLoopbackMode = 0; // INCLUDE_PROCESS_TREE

                    int paramsSize = Marshal.SizeOf(typeof(AUDIOCLIENT_ACTIVATION_PARAMS));
                    pParams = Marshal.AllocHGlobal(paramsSize);
                    Marshal.StructureToPtr(activationParams, pParams, false);

                    var pv = new PROPVARIANT {
                        vt = 65,            // VT_BLOB
                        cbBlob = paramsSize,
                        pBlobData = pParams
                    };
                    int pvSize = Marshal.SizeOf(typeof(PROPVARIANT));
                    pPropVariant = Marshal.AllocHGlobal(pvSize);
                    Marshal.StructureToPtr(pv, pPropVariant, false);

                    // 2. ActivateAudioInterfaceAsync — wymaga Windows 10 2004+
                    activationHandler = new ActivationHandler();
                    Guid iidAudioClient = new Guid("1CB9AD4C-DBA4-4c53-9D54-6451E8F752BF");
                    var asyncOp = MmDevApi.ActivateAudioInterfaceAsync(
                        MmDevApi.VIRTUAL_AUDIO_DEVICE_PROCESS_LOOPBACK,
                        iidAudioClient,
                        pPropVariant,
                        activationHandler);

                    if (!activationHandler.Wait(5000)) {
                        Log("Recording (PID " + Pid + "): activation timed out (5s).");
                        return;
                    }

                    int actHr;
                    object clientObj;
                    asyncOp.GetActivateResult(out actHr, out clientObj);
                    if (actHr < 0 || clientObj == null) {
                        Log("Recording (PID " + Pid + "): activation failed HR=0x" + actHr.ToString("X8")
                            + " (process loopback wymaga Windows 10 2004 lub nowszego)");
                        return;
                    }

                    audioClient = clientObj as IAudioClient;
                    if (audioClient == null) {
                        if (clientObj != null) Marshal.ReleaseComObject(clientObj);
                        Log("Recording (PID " + Pid + "): clientObj nie jest IAudioClient");
                        return;
                    }

                    // 3. GetMixFormat — formatPtr wskazuje na natywną pamięć CoTaskMem
                    int mfHr = audioClient.GetMixFormat(out formatPtr);
                    if (mfHr < 0 || formatPtr == IntPtr.Zero) {
                        Log("Recording (PID " + Pid + "): GetMixFormat failed HR=0x" + mfHr.ToString("X8"));
                        return;
                    }

                    var fmtEx = (WaveFormatExtensible)Marshal.PtrToStructure(formatPtr, typeof(WaveFormatExtensible));
                    bool isExtensible = (fmtEx.wFormatTag == unchecked((short)0xFFFE));
                    bool isFloat = isExtensible
                        ? fmtEx.SubFormat == KSDATAFORMAT_SUBTYPE_IEEE_FLOAT
                        : (fmtEx.wFormatTag == 3);

                    // 4. Próba event-driven init, fallback timer-driven
                    Guid sessionGuid = Guid.Empty;
                    const int FLAGS_LOOPBACK = 0x00020000;
                    const int FLAGS_EVENTCALLBACK = 0x00040000;
                    bool useEvents = true;

                    int initHr = audioClient.Initialize(
                        0,                                              // SHARED
                        FLAGS_LOOPBACK | FLAGS_EVENTCALLBACK,
                        0, 0,                                           // 0 = default device period
                        formatPtr,                                      // pełne 40 bajtów EXTENSIBLE
                        ref sessionGuid);

                    if (initHr < 0) {
                        Log("Recording (PID " + Pid + "): event-driven init failed HR=0x" + initHr.ToString("X8") + ", fallback to timer-driven");
                        useEvents = false;
                        // Po nieudanym Initialize obiekt jest unusable — trzeba zwolnić i zaktywować jeszcze raz
                        Marshal.ReleaseComObject(audioClient);
                        audioClient = null;

                        // Re-aktywacja
                        activationHandler = new ActivationHandler();
                        asyncOp = MmDevApi.ActivateAudioInterfaceAsync(
                            MmDevApi.VIRTUAL_AUDIO_DEVICE_PROCESS_LOOPBACK,
                            iidAudioClient, pPropVariant, activationHandler);
                        if (!activationHandler.Wait(5000)) { Log("Recording (PID " + Pid + "): re-activation timed out"); return; }
                        asyncOp.GetActivateResult(out actHr, out clientObj);
                        if (actHr < 0 || clientObj == null) { Log("Recording (PID " + Pid + "): re-activation failed"); return; }
                        audioClient = clientObj as IAudioClient;
                        if (audioClient == null) { Log("Recording (PID " + Pid + "): cast failed po re-activation"); return; }

                        initHr = audioClient.Initialize(0, FLAGS_LOOPBACK, 10000000, 0, formatPtr, ref sessionGuid);
                        if (initHr < 0) {
                            Log("Recording (PID " + Pid + "): timer-driven init too failed HR=0x" + initHr.ToString("X8"));
                            return;
                        }
                    }

                    if (useEvents) {
                        eventHandle = Kernel32.CreateEvent(IntPtr.Zero, false, false, null);
                        if (eventHandle == IntPtr.Zero) {
                            Log("Recording (PID " + Pid + "): CreateEvent failed, switching to polling");
                            useEvents = false;
                        } else {
                            int sehHr = audioClient.SetEventHandle(eventHandle);
                            if (sehHr < 0) {
                                Log("Recording (PID " + Pid + "): SetEventHandle failed HR=0x" + sehHr.ToString("X8") + ", switching to polling");
                                Kernel32.CloseHandle(eventHandle);
                                eventHandle = IntPtr.Zero;
                                useEvents = false;
                            }
                        }
                    }

                    uint bufferSize;
                    audioClient.GetBufferSize(out bufferSize);

                    object captureObj;
                    Guid iidCapture = new Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD19F");
                    int gsHr = audioClient.GetService(iidCapture, out captureObj);
                    if (gsHr < 0 || captureObj == null) {
                        Log("Recording (PID " + Pid + "): GetService failed HR=0x" + gsHr.ToString("X8"));
                        return;
                    }
                    captureClient = captureObj as IAudioCaptureClient;
                    if (captureClient == null) {
                        if (captureObj != null) Marshal.ReleaseComObject(captureObj);
                        return;
                    }

                    // 5. WAV file
                    string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recordings");
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    FilePath = Path.Combine(dir, "Record_" + Pid + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav");

                    fs = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    int headerSize = isExtensible ? 68 : 44;
                    fs.Write(new byte[headerSize], 0, headerSize);

                    uint totalBytes = 0;
                    int initialBufferBytes = (int)(bufferSize * fmtEx.nBlockAlign);
                    if (initialBufferBytes <= 0) initialBufferBytes = 8192;
                    byte[] buffer = new byte[initialBufferBytes];

                    int startHr = audioClient.Start();
                    if (startHr < 0) {
                        Log("Recording (PID " + Pid + "): Start failed HR=0x" + startHr.ToString("X8"));
                        return;
                    }
                    clientStarted = true;

                    Log("Recording (PID " + Pid + ") started; mode=" + (useEvents ? "event-driven" : "timer-driven")
                        + ", isExtensible=" + isExtensible + ", isFloat=" + isFloat
                        + ", channels=" + fmtEx.nChannels + ", sampleRate=" + fmtEx.nSamplesPerSec
                        + ", bits=" + fmtEx.wBitsPerSample);

                    // 6. Pętla capture
                    while (running) {
                        if (useEvents) {
                            uint w = Kernel32.WaitForSingleObject(eventHandle, 200);
                            if (w != Kernel32.WAIT_OBJECT_0 && w != Kernel32.WAIT_TIMEOUT) {
                                Log("Recording (PID " + Pid + "): WaitForSingleObject returned 0x" + w.ToString("X"));
                                break;
                            }
                        } else {
                            Thread.Sleep(10);
                        }

                        uint nextPacketSize;
                        if (captureClient.GetNextPacketSize(out nextPacketSize) < 0) continue;
                        while (nextPacketSize > 0 && running) {
                            IntPtr pData;
                            uint numFramesRead;
                            int flags;
                            long pos, qpc;
                            int gbHr = captureClient.GetBuffer(out pData, out numFramesRead, out flags, out pos, out qpc);
                            if (gbHr < 0) break;

                            if (numFramesRead > 0) {
                                int bytesRead = (int)(numFramesRead * fmtEx.nBlockAlign);
                                if (buffer.Length < bytesRead) buffer = new byte[bytesRead];
                                if ((flags & 0x2) != 0) {
                                    // AUDCLNT_BUFFERFLAGS_SILENT — zapisz ciszę
                                    Array.Clear(buffer, 0, bytesRead);
                                } else {
                                    Marshal.Copy(pData, buffer, 0, bytesRead);
                                }
                                try {
                                    fs.Write(buffer, 0, bytesRead);
                                    totalBytes += (uint)bytesRead;
                                } catch (Exception ex) {
                                    Log("Recording (PID " + Pid + "): write to file failed: " + ex.Message);
                                    captureClient.ReleaseBuffer(numFramesRead);
                                    running = false;
                                    break;
                                }
                            }
                            captureClient.ReleaseBuffer(numFramesRead);
                            if (captureClient.GetNextPacketSize(out nextPacketSize) < 0) break;
                        }
                    }

                    // 7. WAV header
                    try {
                        fs.Position = 0;
                        WriteWavHeader(fs, totalBytes, fmtEx, isExtensible, isFloat);
                        fs.Flush();
                        Log("Saved recording to: " + FilePath + " (" + totalBytes + " bytes)");
                    } catch (Exception ex) {
                        Log("Recording (PID " + Pid + "): WAV header write failed: " + ex.Message);
                    }
                } catch (Exception ex) {
                    Log("Recording Error (PID " + Pid + "): " + ex.Message);
                } finally {
                    try { if (clientStarted && audioClient != null) audioClient.Stop(); } catch {}
                    if (captureClient != null) Marshal.ReleaseComObject(captureClient);
                    if (audioClient != null)   Marshal.ReleaseComObject(audioClient);
                    if (eventHandle != IntPtr.Zero) Kernel32.CloseHandle(eventHandle);
                    if (formatPtr != IntPtr.Zero) Marshal.FreeCoTaskMem(formatPtr);
                    if (pPropVariant != IntPtr.Zero) Marshal.FreeHGlobal(pPropVariant);
                    if (pParams != IntPtr.Zero) Marshal.FreeHGlobal(pParams);
                    if (fs != null) { try { fs.Dispose(); } catch {} }
                }
            }

            private void WriteWavHeader(FileStream fs, uint dataSize, WaveFormatExtensible fmt, bool isExtensible, bool isFloat) {
                using (var writer = new BinaryWriter(fs, Encoding.ASCII, leaveOpen: true)) {
                    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                    if (isExtensible) {
                        writer.Write((uint)(60 + dataSize));
                        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                        writer.Write(Encoding.ASCII.GetBytes("fmt "));
                        writer.Write((uint)40);
                        writer.Write(fmt.wFormatTag);
                        writer.Write(fmt.nChannels);
                        writer.Write(fmt.nSamplesPerSec);
                        writer.Write(fmt.nAvgBytesPerSec);
                        writer.Write(fmt.nBlockAlign);
                        writer.Write(fmt.wBitsPerSample);
                        writer.Write((short)22);
                        writer.Write(fmt.wValidBitsPerSample);
                        writer.Write(fmt.dwChannelMask);
                        writer.Write(fmt.SubFormat.ToByteArray());
                        writer.Write(Encoding.ASCII.GetBytes("data"));
                        writer.Write(dataSize);
                    } else {
                        writer.Write((uint)(36 + dataSize));
                        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                        writer.Write(Encoding.ASCII.GetBytes("fmt "));
                        writer.Write((uint)16);
                        writer.Write(isFloat ? (short)3 : fmt.wFormatTag);
                        writer.Write(fmt.nChannels);
                        writer.Write(fmt.nSamplesPerSec);
                        writer.Write(fmt.nAvgBytesPerSec);
                        writer.Write(fmt.nBlockAlign);
                        writer.Write(fmt.wBitsPerSample);
                        writer.Write(Encoding.ASCII.GetBytes("data"));
                        writer.Write(dataSize);
                    }
                    writer.Flush();
                }
            }
        }

        // ============================================================
        // JSON helpers
        // ============================================================
        static string JsonEscape(string s) {
            if (s == null) return "\"\"";
            var sb = new StringBuilder(s.Length + 2);
            sb.Append('"');
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                switch (c) {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\b': sb.Append("\\b");  break;
                    case '\f': sb.Append("\\f");  break;
                    case '\n': sb.Append("\\n");  break;
                    case '\r': sb.Append("\\r");  break;
                    case '\t': sb.Append("\\t");  break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("X4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        static void WriteResponse(string status, string requestId, string extraJson = null) {
            // extraJson: dodatkowy fragment "...,\"key\":value..." wstawiany przed }
            var sb = new StringBuilder(64);
            sb.Append("{\"status\":").Append(JsonEscape(status));
            if (!string.IsNullOrEmpty(extraJson)) sb.Append(extraJson);
            if (requestId != null) sb.Append(",\"requestId\":").Append(JsonEscape(requestId));
            sb.Append('}');
            StdoutWriter.EnqueueResponse(sb.ToString());
        }

        // ============================================================
        // Log
        // ============================================================
        public static void Log(string msg) {
            lock (logLock) {
                try {
                    const string logFile = "bridge_debug.log";
                    if (File.Exists(logFile)) {
                        var info = new FileInfo(logFile);
                        if (info.Length > 1024 * 1024) {
                            try { File.Delete(logFile); } catch {}
                        }
                    }
                    using (var fs = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var sw = new StreamWriter(fs, new UTF8Encoding(false))) {
                        sw.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + " - " + msg);
                    }
                } catch {}
            }
        }

        // ============================================================
        // Main
        // ============================================================
        static void Main(string[] args)
        {
            Log("Bridge starting (V1.8.0 - Hardened + event-driven + stdout backpressure)...");
            Console.OutputEncoding = new UTF8Encoding(false);
            Console.InputEncoding  = new UTF8Encoding(false);

            StdoutWriter.Start();
            StdoutWriter.EnqueueResponse("{\"status\":\"ready\"}");

            var peakThread = new Thread(PeakPollingLoop);
            peakThread.SetApartmentState(ApartmentState.MTA);
            peakThread.IsBackground = true;
            peakThread.Name = "PeakPolling";
            peakThread.Start();

            string line;
            try {
                while ((line = Console.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line)) continue;
                    try {
                        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(line);
                        if (dict == null || !dict.ContainsKey("action")) continue;

                        string requestId = dict.ContainsKey("requestId") && dict["requestId"] != null
                            ? dict["requestId"].ToString() : null;
                        string action = dict["action"].ToString();

                        switch (action) {
                            case "get_sessions": HandleGetSessions(requestId); break;
                            case "get_master":   HandleGetMaster(requestId); break;
                            case "set_volume":
                                if (dict.ContainsKey("pid") && dict.ContainsKey("volume") && dict["volume"] != null) {
                                    int pid = Convert.ToInt32(dict["pid"]);
                                    float vol = Convert.ToSingle(dict["volume"], CultureInfo.InvariantCulture);
                                    HandleSetVolume(pid, Clamp01(vol), requestId);
                                } else WriteResponse("error_bad_args", requestId);
                                break;
                            case "set_master_volume":
                                if (dict.ContainsKey("volume") && dict["volume"] != null) {
                                    float mvol = Convert.ToSingle(dict["volume"], CultureInfo.InvariantCulture);
                                    HandleSetMasterVolume(Clamp01(mvol), requestId);
                                } else WriteResponse("error_bad_args", requestId);
                                break;
                            case "toggle_mute":
                                if (dict.ContainsKey("pid")) {
                                    int pidMute = Convert.ToInt32(dict["pid"]);
                                    HandleToggleMute(pidMute, requestId);
                                } else WriteResponse("error_bad_args", requestId);
                                break;
                            case "set_ducking":
                                HandleSetDucking(dict, requestId);
                                break;
                            case "start_recording":
                                if (dict.ContainsKey("pid")) {
                                    int rPid = Convert.ToInt32(dict["pid"]);
                                    HandleStartRecording(rPid, requestId);
                                } else WriteResponse("error_bad_args", requestId);
                                break;
                            case "stop_recording":
                                if (dict.ContainsKey("pid")) {
                                    int sPid = Convert.ToInt32(dict["pid"]);
                                    HandleStopRecording(sPid, requestId);
                                } else WriteResponse("error_bad_args", requestId);
                                break;
                            case "set_boost":
                                if (dict.ContainsKey("active")) {
                                    bool bActive = Convert.ToBoolean(dict["active"]);
                                    float? factor = null;
                                    if (dict.ContainsKey("factor") && dict["factor"] != null)
                                        factor = Convert.ToSingle(dict["factor"], CultureInfo.InvariantCulture);
                                    HandleSetBoost(bActive, factor, requestId);
                                } else WriteResponse("error_bad_args", requestId);
                                break;
                            case "get_stats":
                                HandleGetStats(requestId);
                                break;
                            case "ping":
                                WriteResponse("pong", requestId);
                                break;
                            case "exit":
                                shouldExit = true;
                                peakThread.Join(1500);
                                StopAllRecordings();
                                StdoutWriter.Shutdown();
                                return;
                            default:
                                WriteResponse("error_unknown_action", requestId);
                                break;
                        }
                    } catch (Exception ex) {
                        Log("Action Error: " + ex.Message + " | line: " + (line.Length > 200 ? line.Substring(0, 200) + "..." : line));
                    }
                }
            } catch (Exception ex) {
                Log("Stdin loop fatal: " + ex.Message);
            }

            Log("Stdin closed, exiting.");
            shouldExit = true;
            peakThread.Join(1500);
            StopAllRecordings();
            StdoutWriter.Shutdown();
        }

        static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }

        static void StopAllRecordings() {
            List<RecordingSession> toStop;
            lock (recordingsLock) {
                toStop = new List<RecordingSession>(activeRecordings.Values);
                activeRecordings.Clear();
            }
            foreach (var s in toStop) {
                try { s.Stop(); } catch (Exception ex) { Log("StopRec error: " + ex.Message); }
            }
        }

        static void HandleGetStats(string requestId) {
            int dp = StdoutWriter.DroppedPeaks;
            int dr = StdoutWriter.DroppedResponses;
            int rec, baseN, fadeN;
            lock (recordingsLock) rec = activeRecordings.Count;
            lock (stateLock) { baseN = baseVolumes.Count; fadeN = currentFades.Count; }
            string extra = ",\"droppedPeaks\":" + dp
                         + ",\"droppedResponses\":" + dr
                         + ",\"activeRecordings\":" + rec
                         + ",\"baseVolumesCount\":" + baseN
                         + ",\"currentFadesCount\":" + fadeN
                         + ",\"boostActive\":" + (globalBoostActive ? "true" : "false");
            WriteResponse("ok", requestId, extra);
        }

        // ============================================================
        // Ducking config
        // ============================================================
        static void HandleSetDucking(Dictionary<string, object> dict, string requestId) {
            try {
                lock (stateLock) {
                    if (dict.ContainsKey("enabled"))    duckSettings.Enabled    = Convert.ToBoolean(dict["enabled"]);
                    if (dict.ContainsKey("triggerPid")) duckSettings.TriggerPid = Convert.ToInt32(dict["triggerPid"]);
                    if (dict.ContainsKey("threshold"))  duckSettings.Threshold  = Clamp01(Convert.ToSingle(dict["threshold"], CultureInfo.InvariantCulture));
                    if (dict.ContainsKey("factor"))     duckSettings.Factor     = Clamp01(Convert.ToSingle(dict["factor"], CultureInfo.InvariantCulture));
                    if (dict.ContainsKey("fadeSpeed")) {
                        float fs = Convert.ToSingle(dict["fadeSpeed"], CultureInfo.InvariantCulture);
                        duckSettings.FadeSpeed = Math.Max(0.001f, Math.Min(1f, fs));
                    }
                }
                WriteResponse("ok", requestId);
            } catch (Exception ex) { Log("SetDucking Error: " + ex.Message); WriteResponse("error", requestId); }
        }

        // ============================================================
        // Peak polling
        // ============================================================
        static void PeakPollingLoop()
        {
            Log("Peak thread started (V1.8.0)");
            string lastDeviceId = null;
            IMMDevice bestDevice = null;
            IAudioSessionManager2 bestManager = null;
            IAudioMeterInformation bestMeter = null;
            int tick = 0;

            try
            {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                while (!shouldExit)
                {
                    tick++;
                    try
                    {
                        IMMDevice currentDevice = null;
                        int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out currentDevice);
                        if (res != 0 || currentDevice == null) {
                            Thread.Sleep(1000);
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
                                currentDevice.Activate(ref iidManager2, 23, IntPtr.Zero, out mObj2);
                                bestManager = mObj2 as IAudioSessionManager2;
                                if (bestManager == null && mObj2 != null) Marshal.ReleaseComObject(mObj2);

                                Guid iidMeter = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");
                                object meterObj = null;
                                currentDevice.Activate(ref iidMeter, 23, IntPtr.Zero, out meterObj);
                                bestMeter = meterObj as IAudioMeterInformation;
                                if (bestMeter == null && meterObj != null) Marshal.ReleaseComObject(meterObj);

                                bestDevice = currentDevice;
                                lastDeviceId = currentId;

                                lock (stateLock) {
                                    baseVolumes.Clear();
                                    currentFades.Clear();
                                }
                            } catch (Exception ex) {
                                Log("Device activation failed: " + ex.Message);
                                Marshal.ReleaseComObject(currentDevice);
                            }

                            if (oldMeter != null)   Marshal.ReleaseComObject(oldMeter);
                            if (oldManager != null) Marshal.ReleaseComObject(oldManager);
                            if (oldDevice != null)  Marshal.ReleaseComObject(oldDevice);
                        } else {
                            Marshal.ReleaseComObject(currentDevice);
                        }

                        if (bestManager == null) {
                            Thread.Sleep(1000);
                            continue;
                        }

                        IAudioSessionEnumerator sessionEnum = null;
                        res = bestManager.GetSessionEnumerator(out sessionEnum);
                        if (res != 0 || sessionEnum == null) {
                            Thread.Sleep(200);
                            continue;
                        }

                        try {
                            int sessionCount;
                            sessionEnum.GetCount(out sessionCount);

                            // Snapshot ustawień
                            bool dEnabled; int dTrigger; float dThreshold; float dFactor; float dFadeSpeed;
                            bool boostActive; float boostFactor;
                            lock (stateLock) {
                                dEnabled = duckSettings.Enabled;
                                dTrigger = duckSettings.TriggerPid;
                                dThreshold = duckSettings.Threshold;
                                dFactor = duckSettings.Factor;
                                dFadeSpeed = duckSettings.FadeSpeed;
                                boostActive = globalBoostActive;
                                boostFactor = boostReductionFactor;
                            }

                            // 1. Czy trigger przekracza próg?
                            bool triggerAboveThreshold = false;
                            if (dEnabled && dTrigger > 0) {
                                for (int s = 0; s < sessionCount && !triggerAboveThreshold; s++) {
                                    IAudioSessionControl control = null;
                                    try {
                                        sessionEnum.GetSession(s, out control);
                                        var c2 = control as IAudioSessionControl2;
                                        if (c2 != null) {
                                            int pid;
                                            c2.GetProcessId(out pid);
                                            if (pid == dTrigger) {
                                                var meter = control as IAudioMeterInformation;
                                                if (meter != null) {
                                                    float peak;
                                                    meter.GetPeakValue(out peak);
                                                    if (peak >= dThreshold) triggerAboveThreshold = true;
                                                }
                                            }
                                        }
                                    } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                                }
                            }

                            var activeSessions = new List<SessionInfo>(sessionCount);
                            var sessListJson = new List<string>(sessionCount);

                            // 2. Iteracja po wszystkich sesjach
                            for (int s = 0; s < sessionCount; s++) {
                                IAudioSessionControl control = null;
                                try {
                                    sessionEnum.GetSession(s, out control);
                                    if (control == null) continue;

                                    var control2 = control as IAudioSessionControl2;
                                    var volume = control as ISimpleAudioVolume;
                                    var sessMeter = control as IAudioMeterInformation;

                                    if (control2 == null || volume == null) continue;

                                    int pid = -1;
                                    control2.GetProcessId(out pid);
                                    if (pid <= 0) continue;

                                    float peak = 0;
                                    if (sessMeter != null) sessMeter.GetPeakValue(out peak);

                                    float currentVol;
                                    bool muted;
                                    volume.GetMasterVolume(out currentVol);
                                    volume.GetMute(out muted);

                                    float targetFade = (dEnabled && triggerAboveThreshold && pid != dTrigger)
                                        ? dFactor : 1.0f;
                                    float boostMul = (boostActive && pid != dTrigger) ? boostFactor : 1.0f;

                                    float newFade;
                                    float baseVol;
                                    bool shouldApply = false;
                                    float toApply = 0f;

                                    lock (stateLock) {
                                        if (!currentFades.ContainsKey(pid)) currentFades[pid] = 1.0f;
                                        newFade = currentFades[pid];

                                        if (newFade != targetFade) {
                                            if (newFade < targetFade)
                                                newFade = Math.Min(targetFade, newFade + dFadeSpeed);
                                            else
                                                newFade = Math.Max(targetFade, newFade - dFadeSpeed);
                                            currentFades[pid] = newFade;
                                        }

                                        // Zapis base tylko gdy NIE jesteśmy ani ducked ani boostowani.
                                        // Inaczej downward spiral: ducked currentVol byłby zapisany jako baza.
                                        if (newFade >= 0.9999f && Math.Abs(boostMul - 1.0f) < 0.0001f) {
                                            baseVolumes[pid] = currentVol;
                                        }
                                        if (!baseVolumes.ContainsKey(pid)) baseVolumes[pid] = currentVol;
                                        baseVol = baseVolumes[pid];

                                        if (!muted) {
                                            float effective = Clamp01(baseVol * newFade * boostMul);
                                            if (Math.Abs(effective - currentVol) > 0.001f) {
                                                shouldApply = true;
                                                toApply = effective;
                                            }
                                        }
                                    }

                                    if (shouldApply) {
                                        try { volume.SetMasterVolume(toApply, Guid.Empty); } catch {}
                                    }

                                    string processName = ResolveProcessName(pid);
                                    activeSessions.Add(new SessionInfo { ProcessId = pid, PeakValue = peak });

                                    var sb = new StringBuilder(96);
                                    sb.Append("{\"pid\":").Append(pid);
                                    sb.Append(",\"name\":").Append(JsonEscape(processName));
                                    sb.Append(",\"volume\":").Append(currentVol.ToString("F4", CultureInfo.InvariantCulture));
                                    sb.Append(",\"muted\":").Append(muted ? "true" : "false");
                                    sb.Append('}');
                                    sessListJson.Add(sb.ToString());
                                } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                            }

                            float masterPeak = 0;
                            if (bestMeter != null) {
                                try { bestMeter.GetPeakValue(out masterPeak); } catch {}
                            }

                            var peaksSb = new StringBuilder(128 + activeSessions.Count * 32);
                            peaksSb.Append("{\"type\":\"peaks\",\"master\":").Append(masterPeak.ToString("F4", CultureInfo.InvariantCulture));
                            peaksSb.Append(",\"sessions\":{");
                            for (int k = 0; k < activeSessions.Count; k++) {
                                if (k > 0) peaksSb.Append(',');
                                peaksSb.Append('"').Append(activeSessions[k].ProcessId).Append("\":");
                                peaksSb.Append(activeSessions[k].PeakValue.ToString("F4", CultureInfo.InvariantCulture));
                            }
                            peaksSb.Append("}}");

                            // Drop policy zapewnia że nie blokujemy się na zatkanym pipe
                            StdoutWriter.EnqueuePeak(peaksSb.ToString());

                            lock (sessionsLock) {
                                lastSessionsJson = "{\"status\":\"ok\",\"sessions\":[" + string.Join(",", sessListJson.ToArray()) + "]}";
                            }
                        } finally {
                            Marshal.ReleaseComObject(sessionEnum);
                        }

                        if (tick % ZOMBIE_CLEANUP_EVERY_N_TICKS == 0) {
                            CleanupZombiePids();
                        }

                        Thread.Sleep(100);
                    } catch (Exception ex) {
                        Log("Loop Error: " + ex.Message);
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex) { Log("Fatal Peak Error: " + ex.ToString()); }
            finally {
                if (bestMeter != null)   Marshal.ReleaseComObject(bestMeter);
                if (bestManager != null) Marshal.ReleaseComObject(bestManager);
                if (bestDevice != null)  Marshal.ReleaseComObject(bestDevice);
            }
        }

        static void CleanupZombiePids() {
            List<int> toRemove = new List<int>();
            List<int> snapshotPids;
            lock (stateLock) {
                snapshotPids = new List<int>(baseVolumes.Keys);
                foreach (var pid in currentFades.Keys) {
                    if (!snapshotPids.Contains(pid)) snapshotPids.Add(pid);
                }
                foreach (var pid in processNameCache.Keys) {
                    if (!snapshotPids.Contains(pid)) snapshotPids.Add(pid);
                }
            }
            foreach (var pid in snapshotPids) {
                try {
                    using (var p = Process.GetProcessById(pid)) {
                        if (p.HasExited) toRemove.Add(pid);
                    }
                } catch {
                    toRemove.Add(pid);
                }
            }
            if (toRemove.Count > 0) {
                lock (stateLock) {
                    foreach (var pid in toRemove) {
                        baseVolumes.Remove(pid);
                        currentFades.Remove(pid);
                        processNameCache.Remove(pid);
                    }
                }
            }
        }

        static string ResolveProcessName(int pid) {
            lock (stateLock) {
                string cached;
                if (processNameCache.TryGetValue(pid, out cached)) return cached;
            }
            string name = "Unknown";
            try {
                using (var p = Process.GetProcessById(pid)) {
                    name = p.ProcessName;
                    if (!string.IsNullOrEmpty(name))
                        name = char.ToUpper(name[0]) + name.Substring(1);
                }
            } catch {}
            lock (stateLock) {
                processNameCache[pid] = name;
            }
            return name;
        }

        // ============================================================
        // Handlers
        // ============================================================
        static void HandleGetSessions(string requestId) {
            string json;
            lock (sessionsLock) { json = lastSessionsJson; }
            if (requestId != null) json = json.Substring(0, json.Length - 1) + ",\"requestId\":" + JsonEscape(requestId) + "}";
            StdoutWriter.EnqueueResponse(json);
        }

        static void HandleGetMaster(string requestId) {
            IMMDevice device = null;
            object volObj = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) { WriteResponse("error_no_device", requestId); return; }

                Guid iidVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidVol, 23, IntPtr.Zero, out volObj);
                var volume = volObj as IAudioEndpointVolume;
                if (volume == null) {
                    if (volObj != null) { Marshal.ReleaseComObject(volObj); volObj = null; }
                    WriteResponse("error_no_endpoint", requestId);
                    return;
                }
                float level;
                bool muted;
                volume.GetMasterVolumeLevelScalar(out level);
                volume.GetMute(out muted);

                string extra = ",\"master\":{\"volume\":" + level.ToString("F4", CultureInfo.InvariantCulture)
                             + ",\"muted\":" + (muted ? "true" : "false") + "}";
                WriteResponse("ok", requestId, extra);
            } catch (Exception ex) { Log("Master Error: " + ex.Message); WriteResponse("error", requestId); }
            finally {
                if (volObj != null) Marshal.ReleaseComObject(volObj);
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }

        static void HandleSetVolume(int pid, float vol, string requestId) {
            float fade, boost;
            lock (stateLock) {
                baseVolumes[pid] = vol;
                fade = currentFades.ContainsKey(pid) ? currentFades[pid] : 1.0f;
                boost = globalBoostActive && pid != duckSettings.TriggerPid ? boostReductionFactor : 1.0f;
            }
            float effective = Clamp01(vol * fade * boost);

            IMMDevice device = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnum = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) { WriteResponse("error_no_device", requestId); return; }

                Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                object mObj2;
                device.Activate(ref iidManager2, 23, IntPtr.Zero, out mObj2);
                manager = mObj2 as IAudioSessionManager2;
                if (manager == null && mObj2 != null) Marshal.ReleaseComObject(mObj2);

                if (manager != null && manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                    int sessionCount;
                    sessionEnum.GetCount(out sessionCount);
                    for (int s = 0; s < sessionCount; s++) {
                        IAudioSessionControl control = null;
                        try {
                            sessionEnum.GetSession(s, out control);
                            if (control == null) continue;
                            var control2 = control as IAudioSessionControl2;
                            if (control2 == null) continue;
                            int cPid;
                            control2.GetProcessId(out cPid);
                            if (cPid != pid) continue;
                            var simpleVol = control as ISimpleAudioVolume;
                            if (simpleVol != null) simpleVol.SetMasterVolume(effective, Guid.Empty);
                        } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                    }
                }
                WriteResponse("ok", requestId);
            } catch (Exception ex) { Log("SetVolume Error: " + ex.Message); WriteResponse("error", requestId); }
            finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                if (manager != null)     Marshal.ReleaseComObject(manager);
                if (device != null)      Marshal.ReleaseComObject(device);
            }
        }

        static void HandleSetMasterVolume(float vol, string requestId) {
            IMMDevice device = null;
            object volObj = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                int res = deviceEnum.GetDefaultAudioEndpoint(0, 0, out device);
                if (res != 0 || device == null) { WriteResponse("error_no_device", requestId); return; }
                Guid iidVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidVol, 23, IntPtr.Zero, out volObj);
                var volume = volObj as IAudioEndpointVolume;
                if (volume == null) {
                    if (volObj != null) { Marshal.ReleaseComObject(volObj); volObj = null; }
                    WriteResponse("error_no_endpoint", requestId);
                    return;
                }
                volume.SetMasterVolumeLevelScalar(vol, Guid.Empty);
                WriteResponse("ok", requestId);
            } catch (Exception ex) { Log("SetMasterVolume Error: " + ex.Message); WriteResponse("error", requestId); }
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
                if (res != 0 || device == null) { WriteResponse("error_no_device", requestId); return; }
                Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                object mObj2;
                device.Activate(ref iidManager2, 23, IntPtr.Zero, out mObj2);
                manager = mObj2 as IAudioSessionManager2;
                if (manager == null && mObj2 != null) Marshal.ReleaseComObject(mObj2);

                if (manager != null && manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                    int sessionCount;
                    sessionEnum.GetCount(out sessionCount);
                    for (int s = 0; s < sessionCount; s++) {
                        IAudioSessionControl control = null;
                        try {
                            sessionEnum.GetSession(s, out control);
                            if (control == null) continue;
                            var control2 = control as IAudioSessionControl2;
                            if (control2 == null) continue;
                            int cPid;
                            control2.GetProcessId(out cPid);
                            if (cPid != pid) continue;
                            var volume = control as ISimpleAudioVolume;
                            if (volume != null) {
                                bool currentMute;
                                volume.GetMute(out currentMute);
                                volume.SetMute(!currentMute, Guid.Empty);
                            }
                        } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                    }
                }
                WriteResponse("ok", requestId);
            } catch (Exception ex) { Log("ToggleMute Error: " + ex.Message); WriteResponse("error", requestId); }
            finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                if (manager != null)     Marshal.ReleaseComObject(manager);
                if (device != null)      Marshal.ReleaseComObject(device);
            }
        }

        static void HandleSetBoost(bool active, float? factorOverride, string requestId) {
            float effectiveFactor;
            lock (stateLock) {
                globalBoostActive = active;
                if (factorOverride.HasValue) {
                    float f = factorOverride.Value;
                    boostReductionFactor = Math.Max(0f, Math.Min(1f, f));
                }
                effectiveFactor = boostReductionFactor;
            }
            Log("Smart Overdrive " + (active ? "Enabled" : "Disabled") + " (factor=" + effectiveFactor.ToString("F2", CultureInfo.InvariantCulture) + ")");

            IMMDevice device = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnum = null;
            try {
                var deviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                if (deviceEnum.GetDefaultAudioEndpoint(0, 0, out device) == 0 && device != null) {
                    Guid iidManager2 = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                    object mObj2;
                    device.Activate(ref iidManager2, 23, IntPtr.Zero, out mObj2);
                    manager = mObj2 as IAudioSessionManager2;
                    if (manager == null && mObj2 != null) Marshal.ReleaseComObject(mObj2);

                    if (manager != null && manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null) {
                        int count;
                        sessionEnum.GetCount(out count);
                        for (int i = 0; i < count; i++) {
                            IAudioSessionControl control = null;
                            try {
                                sessionEnum.GetSession(i, out control);
                                if (control == null) continue;
                                var control2 = control as IAudioSessionControl2;
                                if (control2 == null) continue;
                                int pid;
                                control2.GetProcessId(out pid);

                                float vol, fade, boost;
                                lock (stateLock) {
                                    if (!baseVolumes.ContainsKey(pid)) continue;
                                    vol = baseVolumes[pid];
                                    fade = currentFades.ContainsKey(pid) ? currentFades[pid] : 1.0f;
                                    boost = (active && pid != duckSettings.TriggerPid) ? boostReductionFactor : 1.0f;
                                }
                                var sav = control as ISimpleAudioVolume;
                                if (sav != null) sav.SetMasterVolume(Clamp01(vol * fade * boost), Guid.Empty);
                            } catch {} finally { if (control != null) Marshal.ReleaseComObject(control); }
                        }
                    }
                }
            } catch (Exception ex) { Log("Boost Apply Error: " + ex.Message); }
            finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                if (manager != null)     Marshal.ReleaseComObject(manager);
                if (device != null)      Marshal.ReleaseComObject(device);
            }

            WriteResponse("ok", requestId);
        }

        static void HandleStartRecording(int pid, string requestId) {
            lock (recordingsLock) {
                if (activeRecordings.Count >= MAX_CONCURRENT_RECORDINGS && !activeRecordings.ContainsKey(pid)) {
                    Log("Recording rejected: limit reached (" + MAX_CONCURRENT_RECORDINGS + ")");
                    WriteResponse("error_limit", requestId);
                    return;
                }
                if (activeRecordings.ContainsKey(pid)) {
                    activeRecordings[pid].Stop();
                    activeRecordings.Remove(pid);
                }
                var session = new RecordingSession { Pid = pid };
                activeRecordings[pid] = session;
                session.Start();
                Log("Recording started for PID " + pid);
            }
            WriteResponse("ok", requestId);
        }

        static void HandleStopRecording(int pid, string requestId) {
            RecordingSession session = null;
            lock (recordingsLock) {
                if (activeRecordings.ContainsKey(pid)) {
                    session = activeRecordings[pid];
                    activeRecordings.Remove(pid);
                }
            }
            if (session != null) {
                session.Stop();
                Log("Recording stopped for PID " + pid);
            }
            WriteResponse("ok", requestId);
        }
    }
}
