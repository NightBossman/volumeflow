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
        // Performance — shared COM caches & published session list.
        // Optimizations by Claude (Anthropic) model `claude-opus-4-7`.
        //
        // The previous design re-created the MMDeviceEnumerator and
        // re-activated IAudioSessionManager2 / IAudioEndpointVolume on
        // EVERY single Handle* command. During a slider drag that fires
        // 30–60 IPC events/sec this added a 1–2 ms COM tax per event
        // (CoCreateInstance + GetDefaultAudioEndpoint + Activate +
        // GetSessionEnumerator + linear PID scan over all sessions).
        //
        // We now keep three shared interfaces alive across requests:
        //   * `sharedDeviceEnum`     — singleton MMDeviceEnumerator
        //   * `sharedSessionManager` — manager for the current default
        //   * `sharedEndpointVolume` — master volume/mute control
        // All three are refreshed by PeakPollingLoop on device change.
        //
        // For per-PID operations we publish the session-control RCWs
        // produced by each peak-polling cycle to `publishedSessions`,
        // so Handle{SetVolume,ToggleMute,SetBoost} can do an O(N)
        // PID lookup over an in-memory list instead of a full COM
        // enumeration. Lifetime: the previous cycle's controls are
        // released under `publishedSessionsLock` AFTER the swap, so
        // readers either see the old list (safely Release'd later)
        // or the new list — never a half-released reference.
        // ============================================================
        sealed class PerSessionRef {
            public int Pid;
            public object Control; // IAudioSessionControl RCW; cast to interfaces as needed.
        }

        static IMMDeviceEnumerator sharedDeviceEnum = null;
        static IAudioSessionManager2 sharedSessionManager = null;
        static IAudioEndpointVolume sharedEndpointVolume = null;
        static readonly object sharedComLock = new object();

        static List<PerSessionRef> publishedSessions = new List<PerSessionRef>();
        static readonly object publishedSessionsLock = new object();

        // Reusable StringBuilders for the peak/sessions JSON. Reset (Clear)
        // each cycle instead of allocating fresh — saves GC pressure at
        // 10 Hz over the bridge's lifetime.
        static readonly StringBuilder peakBuilder = new StringBuilder(2048);
        static readonly StringBuilder sessionsBuilder = new StringBuilder(2048);

        // Throttle: re-resolve the default audio endpoint at ~1 Hz instead
        // of every 100 ms peak tick. Default-device switches are rare
        // events; hitting GetDefaultAudioEndpoint 10×/sec was wasted COM.
        const int DEVICE_POLL_EVERY_N_TICKS = 10;

        static IMMDeviceEnumerator GetSharedDeviceEnum() {
            var existing = sharedDeviceEnum;
            if (existing != null) return existing;
            lock (sharedComLock) {
                if (sharedDeviceEnum == null) {
                    try {
                        sharedDeviceEnum = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
                    } catch (Exception ex) {
                        Log("Shared device enumerator init failed: " + ex.Message);
                    }
                }
                return sharedDeviceEnum;
            }
        }

        // Lazy-init the shared IAudioEndpointVolume so that master ops
        // issued before PeakPollingLoop has run its first device-switch
        // (e.g. immediately after bridge startup) still succeed.
        static IAudioEndpointVolume GetOrCreateSharedEndpointVolume() {
            var existing = sharedEndpointVolume;
            if (existing != null) return existing;

            IMMDevice device = null;
            object volObj = null;
            try {
                var de = GetSharedDeviceEnum();
                if (de == null) return null;
                if (de.GetDefaultAudioEndpoint(0, 0, out device) != 0 || device == null) return null;
                Guid iidVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                device.Activate(ref iidVol, 23, IntPtr.Zero, out volObj);
                var ep = volObj as IAudioEndpointVolume;
                if (ep == null) {
                    if (volObj != null) Marshal.ReleaseComObject(volObj);
                    return null;
                }
                lock (sharedComLock) {
                    if (sharedEndpointVolume == null) {
                        sharedEndpointVolume = ep;
                        return ep;
                    }
                    // Lost a race; use the existing instance, drop ours.
                    Marshal.ReleaseComObject(ep);
                    return sharedEndpointVolume;
                }
            } catch (Exception ex) {
                Log("Lazy init shared endpoint volume failed: " + ex.Message);
                if (volObj != null) Marshal.ReleaseComObject(volObj);
                return null;
            } finally {
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }

        // Lazy-init the shared IAudioSessionManager2 in case a per-PID
        // command races ahead of PeakPollingLoop's first device-switch.
        static IAudioSessionManager2 GetOrCreateSharedSessionManager() {
            var existing = sharedSessionManager;
            if (existing != null) return existing;

            IMMDevice device = null;
            object mObj = null;
            try {
                var de = GetSharedDeviceEnum();
                if (de == null) return null;
                if (de.GetDefaultAudioEndpoint(0, 0, out device) != 0 || device == null) return null;
                Guid iidMgr = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                device.Activate(ref iidMgr, 23, IntPtr.Zero, out mObj);
                var mgr = mObj as IAudioSessionManager2;
                if (mgr == null) {
                    if (mObj != null) Marshal.ReleaseComObject(mObj);
                    return null;
                }
                lock (sharedComLock) {
                    if (sharedSessionManager == null) {
                        sharedSessionManager = mgr;
                        return mgr;
                    }
                    Marshal.ReleaseComObject(mgr);
                    return sharedSessionManager;
                }
            } catch (Exception ex) {
                Log("Lazy init shared session manager failed: " + ex.Message);
                if (mObj != null) Marshal.ReleaseComObject(mObj);
                return null;
            } finally {
                if (device != null) Marshal.ReleaseComObject(device);
            }
        }

        static void ReleaseAllSharedCom() {
            // Drop published session controls
            List<PerSessionRef> toRelease;
            lock (publishedSessionsLock) {
                toRelease = publishedSessions;
                publishedSessions = new List<PerSessionRef>();
            }
            if (toRelease != null) {
                foreach (var s in toRelease) {
                    if (s.Control != null) {
                        try { Marshal.ReleaseComObject(s.Control); } catch {}
                    }
                }
            }
            // Drop shared COM
            lock (sharedComLock) {
                if (sharedEndpointVolume != null) {
                    try { Marshal.ReleaseComObject(sharedEndpointVolume); } catch {}
                    sharedEndpointVolume = null;
                }
                if (sharedSessionManager != null) {
                    try { Marshal.ReleaseComObject(sharedSessionManager); } catch {}
                    sharedSessionManager = null;
                }
                if (sharedDeviceEnum != null) {
                    try { Marshal.ReleaseComObject(sharedDeviceEnum); } catch {}
                    sharedDeviceEnum = null;
                }
            }
        }

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
                            // Bug fix by Claude (Anthropic) `claude-opus-4-7`:
                            // main.js (both the master-mute hotkey and the
                            // toggle-session-mute IPC handler) sends this
                            // action when the user toggles master mute, but
                            // the bridge had no case for it — every press
                            // was rejected with `error_unknown_action`.
                            case "toggle_master_mute":
                                HandleToggleMasterMute(requestId);
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
        // ============================================================
        // Refactored for perf by Claude (Anthropic) `claude-opus-4-7`.
        //   * Shared MMDeviceEnumerator (no CoCreate each cycle).
        //   * Default-device re-resolution throttled to ~1 Hz instead
        //     of 10 Hz (was wasted COM at 10 GetDefaultAudioEndpoint /
        //     GetId calls/sec for a property that almost never changes).
        //   * Single-pass over sessions: peak + trigger detection +
        //     ducking + boost + JSON all happen in one loop instead
        //     of two (the previous trigger pre-scan was a duplicate
        //     COM walk that did N GetSession + N GetProcessId calls).
        //   * Reusable StringBuilders for the peak / sessions JSON
        //     payloads — Clear() each cycle instead of allocating.
        //   * Session-control RCWs from the freshest enum cycle are
        //     published into `publishedSessions` so per-PID Handle*
        //     commands can skip their own enumeration (saves ~1 ms
        //     of COM tax per IPC during slider drag).
        // ============================================================
        static void PeakPollingLoop()
        {
            Log("Peak thread started (V1.8.1 perf — Claude Opus 4.7)");
            string lastDeviceId = null;
            IMMDevice bestDevice = null;
            IAudioSessionManager2 bestManager = null;
            IAudioMeterInformation bestMeter = null;
            int tick = 0;

            // Scratch buffers reused across cycles. Sized for ~32 sessions;
            // they grow automatically if needed (Array.Resize semantics).
            int[] scratchPids = new int[32];
            float[] scratchPeaks = new float[32];
            IAudioSessionControl[] scratchControls = new IAudioSessionControl[32];

            try
            {
                var deviceEnum = GetSharedDeviceEnum();
                if (deviceEnum == null) { Log("Peak thread: shared device enum is null, aborting"); return; }

                while (!shouldExit)
                {
                    tick++;
                    try
                    {
                        // -------- Throttled default-device check --------
                        // Re-check at startup, when we don't have a manager
                        // yet, or once a second. Skips the GetDefaultAudioEndpoint
                        // round-trip on the other 9/10 ticks.
                        bool needsDeviceCheck = (bestManager == null) || (tick % DEVICE_POLL_EVERY_N_TICKS == 0);
                        if (needsDeviceCheck) {
                            IMMDevice currentDevice = null;
                            int dr = deviceEnum.GetDefaultAudioEndpoint(0, 0, out currentDevice);
                            if (dr != 0 || currentDevice == null) {
                                Thread.Sleep(1000);
                                continue;
                            }

                            string currentId;
                            currentDevice.GetId(out currentId);

                            if (currentId != lastDeviceId) {
                                Log("Switching to device: " + currentId);
                                SwitchToDevice(currentDevice, currentId,
                                    ref bestDevice, ref bestManager, ref bestMeter, ref lastDeviceId);
                            } else {
                                Marshal.ReleaseComObject(currentDevice);
                            }
                        }

                        if (bestManager == null) {
                            Thread.Sleep(1000);
                            continue;
                        }

                        IAudioSessionEnumerator sessionEnum = null;
                        int er = bestManager.GetSessionEnumerator(out sessionEnum);
                        if (er != 0 || sessionEnum == null) {
                            Thread.Sleep(200);
                            continue;
                        }

                        List<PerSessionRef> newPublished = null;

                        try {
                            int sessionCount;
                            sessionEnum.GetCount(out sessionCount);

                            // -------- Snapshot configuration --------
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

                            // -------- Pass 1: collect peaks + PIDs + controls --------
                            // (single COM walk; the old "trigger pre-scan" was
                            // folded into this pass.)
                            if (scratchPids.Length < sessionCount) {
                                Array.Resize(ref scratchPids, sessionCount);
                                Array.Resize(ref scratchPeaks, sessionCount);
                                Array.Resize(ref scratchControls, sessionCount);
                            }

                            int collected = 0;
                            float triggerPeak = 0;
                            bool triggerSeen = false;

                            for (int s = 0; s < sessionCount; s++) {
                                IAudioSessionControl control = null;
                                bool keep = false;
                                try {
                                    sessionEnum.GetSession(s, out control);
                                    if (control == null) continue;

                                    var control2 = control as IAudioSessionControl2;
                                    if (control2 == null) continue;

                                    int pid;
                                    control2.GetProcessId(out pid);
                                    if (pid <= 0) continue;

                                    var meter = control as IAudioMeterInformation;
                                    float peak = 0;
                                    if (meter != null) meter.GetPeakValue(out peak);

                                    if (pid == dTrigger) {
                                        triggerPeak = peak;
                                        triggerSeen = true;
                                    }

                                    scratchPids[collected] = pid;
                                    scratchPeaks[collected] = peak;
                                    scratchControls[collected] = control;
                                    collected++;
                                    keep = true;
                                } catch {
                                } finally {
                                    if (!keep && control != null) Marshal.ReleaseComObject(control);
                                }
                            }

                            bool triggerAboveThreshold = dEnabled && triggerSeen && triggerPeak >= dThreshold;

                            // -------- Pass 2: apply ducking/boost, build session JSON --------
                            sessionsBuilder.Clear();
                            sessionsBuilder.Append("{\"status\":\"ok\",\"sessions\":[");

                            newPublished = new List<PerSessionRef>(collected);

                            bool firstSessJson = true;
                            for (int i = 0; i < collected; i++) {
                                int pid = scratchPids[i];
                                IAudioSessionControl control = scratchControls[i];
                                var volume = control as ISimpleAudioVolume;
                                if (volume == null) {
                                    // Transfer control to publishedSessions even if we
                                    // can't read volume — the cache is still useful for
                                    // PID→control lookups by other handlers.
                                    newPublished.Add(new PerSessionRef { Pid = pid, Control = control });
                                    continue;
                                }

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
                                    float curFade;
                                    if (!currentFades.TryGetValue(pid, out curFade)) {
                                        curFade = 1.0f;
                                        currentFades[pid] = curFade;
                                    }
                                    newFade = curFade;

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
                                    if (!baseVolumes.TryGetValue(pid, out baseVol)) {
                                        baseVol = currentVol;
                                        baseVolumes[pid] = baseVol;
                                    }

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

                                if (!firstSessJson) sessionsBuilder.Append(',');
                                firstSessJson = false;
                                sessionsBuilder.Append("{\"pid\":").Append(pid);
                                sessionsBuilder.Append(",\"name\":").Append(JsonEscape(processName));
                                sessionsBuilder.Append(",\"volume\":").Append(currentVol.ToString("F4", CultureInfo.InvariantCulture));
                                sessionsBuilder.Append(",\"muted\":").Append(muted ? "true" : "false");
                                sessionsBuilder.Append('}');

                                newPublished.Add(new PerSessionRef { Pid = pid, Control = control });
                            }

                            sessionsBuilder.Append("]}");

                            // -------- Master peak + peak JSON --------
                            float masterPeak = 0;
                            if (bestMeter != null) {
                                try { bestMeter.GetPeakValue(out masterPeak); } catch {}
                            }

                            peakBuilder.Clear();
                            peakBuilder.Append("{\"type\":\"peaks\",\"master\":")
                                       .Append(masterPeak.ToString("F4", CultureInfo.InvariantCulture));
                            peakBuilder.Append(",\"sessions\":{");
                            for (int k = 0; k < collected; k++) {
                                if (k > 0) peakBuilder.Append(',');
                                peakBuilder.Append('"').Append(scratchPids[k]).Append("\":");
                                peakBuilder.Append(scratchPeaks[k].ToString("F4", CultureInfo.InvariantCulture));
                            }
                            peakBuilder.Append("}}");

                            // Drop policy zapewnia że nie blokujemy się na zatkanym pipe
                            StdoutWriter.EnqueuePeak(peakBuilder.ToString());

                            lock (sessionsLock) {
                                lastSessionsJson = sessionsBuilder.ToString();
                            }

                            // Clear scratch control slots — ownership transferred
                            // to `newPublished` (or already released for skipped sessions).
                            for (int k = 0; k < collected; k++) scratchControls[k] = null;
                        } finally {
                            if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
                        }

                        // -------- Swap published sessions, release previous cycle's controls --------
                        if (newPublished != null) {
                            List<PerSessionRef> oldPublished;
                            lock (publishedSessionsLock) {
                                oldPublished = publishedSessions;
                                publishedSessions = newPublished;
                            }
                            if (oldPublished != null) {
                                foreach (var s in oldPublished) {
                                    if (s.Control != null) {
                                        try { Marshal.ReleaseComObject(s.Control); } catch {}
                                    }
                                }
                            }
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
                ReleaseAllSharedCom();
            }
        }

        // Activate the IAudioSessionManager2 / IAudioMeterInformation on
        // a freshly-selected default device, and refresh the SHARED
        // IAudioSessionManager2 + IAudioEndpointVolume that Handle*
        // methods consume. The shared interfaces are *separate* RCWs
        // from the peak loop's local ones — that way handlers can lock
        // and use them on the stdin thread without blocking the 10 Hz
        // peak iteration (which uses its own private references and
        // never touches sharedComLock during normal ticks).
        static void SwitchToDevice(IMMDevice currentDevice, string currentId,
            ref IMMDevice bestDevice, ref IAudioSessionManager2 bestManager,
            ref IAudioMeterInformation bestMeter, ref string lastDeviceId)
        {
            var oldMeter = bestMeter;
            var oldManager = bestManager;
            var oldDevice = bestDevice;

            bestMeter = null;
            bestManager = null;
            bestDevice = null;
            lastDeviceId = null;

            IAudioSessionManager2 newSharedManager = null;
            IAudioEndpointVolume newSharedEndpointVol = null;

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

                // Independent RCW for handler-side use
                object mObj2b = null;
                currentDevice.Activate(ref iidManager2, 23, IntPtr.Zero, out mObj2b);
                newSharedManager = mObj2b as IAudioSessionManager2;
                if (newSharedManager == null && mObj2b != null) Marshal.ReleaseComObject(mObj2b);

                // Endpoint volume — used by master get/set/toggle
                Guid iidEndpointVol = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                object epObj = null;
                currentDevice.Activate(ref iidEndpointVol, 23, IntPtr.Zero, out epObj);
                newSharedEndpointVol = epObj as IAudioEndpointVolume;
                if (newSharedEndpointVol == null && epObj != null) Marshal.ReleaseComObject(epObj);

                bestDevice = currentDevice;
                lastDeviceId = currentId;

                lock (stateLock) {
                    baseVolumes.Clear();
                    currentFades.Clear();
                }

                IAudioSessionManager2 oldSharedMgr;
                IAudioEndpointVolume oldSharedEp;
                lock (sharedComLock) {
                    oldSharedMgr = sharedSessionManager;
                    sharedSessionManager = newSharedManager;
                    oldSharedEp = sharedEndpointVolume;
                    sharedEndpointVolume = newSharedEndpointVol;
                }
                if (oldSharedMgr != null) Marshal.ReleaseComObject(oldSharedMgr);
                if (oldSharedEp != null) Marshal.ReleaseComObject(oldSharedEp);
            } catch (Exception ex) {
                Log("Device activation failed: " + ex.Message);
                if (newSharedManager != null) Marshal.ReleaseComObject(newSharedManager);
                if (newSharedEndpointVol != null) Marshal.ReleaseComObject(newSharedEndpointVol);
                Marshal.ReleaseComObject(currentDevice);
            }

            if (oldMeter != null)   Marshal.ReleaseComObject(oldMeter);
            if (oldManager != null) Marshal.ReleaseComObject(oldManager);
            if (oldDevice != null)  Marshal.ReleaseComObject(oldDevice);
        }

        // Refactored by Claude (Anthropic) `claude-opus-4-7`. Old code
        // did `Process.GetProcessById(pid)` per cached PID — that's
        // O(N) exceptions whenever a session has exited (process
        // termination is exactly the common case we run here for).
        // Exceptions are 50–100 µs each due to stack-walk + alloc.
        // Replaced with a single `Process.GetProcesses()` snapshot
        // and a HashSet membership check: O(P + N) instead of
        // O(N × exception_cost), and no exception traffic.
        static void CleanupZombiePids() {
            HashSet<int> alive;
            try {
                var procs = Process.GetProcesses();
                alive = new HashSet<int>();
                for (int i = 0; i < procs.Length; i++) {
                    try { alive.Add(procs[i].Id); } catch {}
                    try { procs[i].Dispose(); } catch {}
                }
            } catch (Exception ex) {
                Log("CleanupZombiePids: Process.GetProcesses failed: " + ex.Message);
                return;
            }

            lock (stateLock) {
                // Collect dead pids across all three caches with a single pass.
                List<int> toRemove = null;
                foreach (var pid in baseVolumes.Keys) {
                    if (!alive.Contains(pid)) {
                        if (toRemove == null) toRemove = new List<int>();
                        toRemove.Add(pid);
                    }
                }
                foreach (var pid in currentFades.Keys) {
                    if (!alive.Contains(pid) && (toRemove == null || !toRemove.Contains(pid))) {
                        if (toRemove == null) toRemove = new List<int>();
                        toRemove.Add(pid);
                    }
                }
                foreach (var pid in processNameCache.Keys) {
                    if (!alive.Contains(pid) && (toRemove == null || !toRemove.Contains(pid))) {
                        if (toRemove == null) toRemove = new List<int>();
                        toRemove.Add(pid);
                    }
                }
                if (toRemove != null) {
                    for (int i = 0; i < toRemove.Count; i++) {
                        int pid = toRemove[i];
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

        // Refactored by Claude (Anthropic) `claude-opus-4-7`:
        // hits the cached shared IAudioEndpointVolume instead of doing
        // CoCreate(MMDeviceEnumerator) + GetDefaultAudioEndpoint +
        // Activate(IAudioEndpointVolume) on every call.
        static void HandleGetMaster(string requestId) {
            var ep = GetOrCreateSharedEndpointVolume();
            if (ep == null) { WriteResponse("error_no_device", requestId); return; }
            try {
                float level;
                bool muted;
                lock (sharedComLock) {
                    ep.GetMasterVolumeLevelScalar(out level);
                    ep.GetMute(out muted);
                }
                string extra = ",\"master\":{\"volume\":" + level.ToString("F4", CultureInfo.InvariantCulture)
                             + ",\"muted\":" + (muted ? "true" : "false") + "}";
                WriteResponse("ok", requestId, extra);
            } catch (Exception ex) {
                Log("Master Error: " + ex.Message);
                WriteResponse("error", requestId);
            }
        }

        // Refactored by Claude (Anthropic) `claude-opus-4-7`. The hot
        // path during a slider drag (~60 events/sec) now skips the
        // full CoCreate+activate+enumerate sequence and just looks up
        // the target PID in publishedSessions (a snapshot maintained
        // by PeakPollingLoop). Falls back to a real enumeration if the
        // PID isn't in the snapshot yet (e.g. session just appeared).
        static void HandleSetVolume(int pid, float vol, string requestId) {
            float fade, boost;
            lock (stateLock) {
                baseVolumes[pid] = vol;
                if (!currentFades.TryGetValue(pid, out fade)) fade = 1.0f;
                boost = globalBoostActive && pid != duckSettings.TriggerPid ? boostReductionFactor : 1.0f;
            }
            float effective = Clamp01(vol * fade * boost);

            // Fast path
            if (TrySessionVolumeOnPublished(pid, effective)) {
                WriteResponse("ok", requestId);
                return;
            }
            // Slow path
            HandleSetVolumeSlow(pid, effective, requestId);
        }

        // The COM SetMasterVolume call happens INSIDE publishedSessionsLock
        // so a concurrent peak-loop swap cannot release the control
        // mid-call. The lock is held for ~µs (one COM hop) — well below
        // the 100 ms peak tick — so contention is negligible in practice.
        static bool TrySessionVolumeOnPublished(int pid, float effective) {
            lock (publishedSessionsLock) {
                for (int i = 0; i < publishedSessions.Count; i++) {
                    var s = publishedSessions[i];
                    if (s.Pid != pid) continue;
                    var sav = s.Control as ISimpleAudioVolume;
                    if (sav == null) return false;
                    try { sav.SetMasterVolume(effective, Guid.Empty); return true; }
                    catch (Exception ex) { Log("Fast-path SetMasterVolume PID " + pid + ": " + ex.Message); return false; }
                }
            }
            return false;
        }

        static void HandleSetVolumeSlow(int pid, float effective, string requestId) {
            var manager = GetOrCreateSharedSessionManager();
            if (manager == null) { WriteResponse("error_no_device", requestId); return; }

            IAudioSessionEnumerator sessionEnum = null;
            try {
                if (manager.GetSessionEnumerator(out sessionEnum) != 0 || sessionEnum == null) {
                    WriteResponse("error_no_enum", requestId);
                    return;
                }
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
                WriteResponse("ok", requestId);
            } catch (Exception ex) {
                Log("SetVolume(slow) Error: " + ex.Message);
                WriteResponse("error", requestId);
            } finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
            }
        }

        // Refactored by Claude (Anthropic) `claude-opus-4-7`:
        // reuse cached IAudioEndpointVolume.
        static void HandleSetMasterVolume(float vol, string requestId) {
            var ep = GetOrCreateSharedEndpointVolume();
            if (ep == null) { WriteResponse("error_no_device", requestId); return; }
            try {
                lock (sharedComLock) { ep.SetMasterVolumeLevelScalar(vol, Guid.Empty); }
                WriteResponse("ok", requestId);
            } catch (Exception ex) {
                Log("SetMasterVolume Error: " + ex.Message);
                WriteResponse("error", requestId);
            }
        }

        // NEW: bug-fix handler by Claude (Anthropic) `claude-opus-4-7`.
        // The main process (renderer's master-mute button + Ctrl+Alt+M
        // hotkey) has been sending this action since v1.8.1, but the
        // bridge had no case for it and was answering `error_unknown_action`
        // — i.e. master mute toggle has been silently broken for two
        // releases. Now it flips the device-level mute via the cached
        // shared IAudioEndpointVolume.
        static void HandleToggleMasterMute(string requestId) {
            var ep = GetOrCreateSharedEndpointVolume();
            if (ep == null) { WriteResponse("error_no_device", requestId); return; }
            try {
                bool muted;
                lock (sharedComLock) {
                    ep.GetMute(out muted);
                    ep.SetMute(!muted, Guid.Empty);
                }
                WriteResponse("ok", requestId);
            } catch (Exception ex) {
                Log("ToggleMasterMute Error: " + ex.Message);
                WriteResponse("error", requestId);
            }
        }

        // Refactored by Claude (Anthropic) `claude-opus-4-7`: same
        // fast-path/slow-path pattern as HandleSetVolume — published
        // session snapshot lookup first, full enumeration on cache miss.
        static void HandleToggleMute(int pid, string requestId) {
            if (TryToggleMuteOnPublished(pid)) {
                WriteResponse("ok", requestId);
                return;
            }
            HandleToggleMuteSlow(pid, requestId);
        }

        static bool TryToggleMuteOnPublished(int pid) {
            lock (publishedSessionsLock) {
                for (int i = 0; i < publishedSessions.Count; i++) {
                    var s = publishedSessions[i];
                    if (s.Pid != pid) continue;
                    var sav = s.Control as ISimpleAudioVolume;
                    if (sav == null) return false;
                    try {
                        bool current;
                        sav.GetMute(out current);
                        sav.SetMute(!current, Guid.Empty);
                        return true;
                    } catch (Exception ex) {
                        Log("Fast-path ToggleMute PID " + pid + ": " + ex.Message);
                        return false;
                    }
                }
            }
            return false;
        }

        static void HandleToggleMuteSlow(int pid, string requestId) {
            var manager = GetOrCreateSharedSessionManager();
            if (manager == null) { WriteResponse("error_no_device", requestId); return; }

            IAudioSessionEnumerator sessionEnum = null;
            try {
                if (manager.GetSessionEnumerator(out sessionEnum) != 0 || sessionEnum == null) {
                    WriteResponse("error_no_enum", requestId);
                    return;
                }
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
                WriteResponse("ok", requestId);
            } catch (Exception ex) {
                Log("ToggleMute(slow) Error: " + ex.Message);
                WriteResponse("error", requestId);
            } finally {
                if (sessionEnum != null) Marshal.ReleaseComObject(sessionEnum);
            }
        }

        // Refactored by Claude (Anthropic) `claude-opus-4-7`: walks the
        // already-published session snapshot instead of doing a fresh
        // device/manager activation and session enumeration on every
        // boost toggle. The boost ramp is only triggered on user action
        // (not on the 10 Hz tick) so cache freshness is plenty.
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

            lock (publishedSessionsLock) {
                for (int i = 0; i < publishedSessions.Count; i++) {
                    var s = publishedSessions[i];
                    int pid = s.Pid;
                    float vol, fade, boost;
                    lock (stateLock) {
                        if (!baseVolumes.TryGetValue(pid, out vol)) continue;
                        if (!currentFades.TryGetValue(pid, out fade)) fade = 1.0f;
                        boost = (active && pid != duckSettings.TriggerPid) ? boostReductionFactor : 1.0f;
                    }
                    var sav = s.Control as ISimpleAudioVolume;
                    if (sav != null) {
                        try { sav.SetMasterVolume(Clamp01(vol * fade * boost), Guid.Empty); }
                        catch (Exception ex) { Log("Boost apply PID " + pid + ": " + ex.Message); }
                    }
                }
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
