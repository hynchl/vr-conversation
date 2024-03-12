using System;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;
using UnityEngine.Experimental.Rendering;
using MemoryPack;
using Oculus.Avatar2;
using StreamLOD = Oculus.Avatar2.OvrAvatarEntity.StreamLOD;
using Unity.Collections;

namespace RTC
{


    public class RTCClient2 : MonoBehaviour
    {

        [Header("Configuration")]
        public string socketUri = "http://localhost:5001";
        public RawImage receivedVideo;
        public AudioSource receivedAudio;
        public AudioSource sendingAudioSource;
        public bool useAudioTrack;
        public bool useVideoTrack;
        public bool useDataTrack;
        public bool useMic;
        public Transform senderObject;
        public Transform receiverObject;
        
        private RTCDataChannel dataChannel, remoteDataChannel;
        
        [SerializeField] private Camera cam;
        private WebCamTexture webCamTexture;
        private Texture2D webcamCopyTexture;
        private Coroutine coroutineConvertFrame;
        
        [Header("Status")]
        public string currentDest;
        public string socketId;
        public string[] others;
        public bool isWaitingAnswer = false; // true after sending an offer
        public bool isWaitingEnd = false; // true after sending an answer
        
        public bool isDest = false;
        private SocketIOUnity socket;
        private RTCPeerConnection pc;
        private Dictionary<string, RTCPeerConnection> pcs;

        private VideoStreamTrack videoStreamTrack;
        private AudioStreamTrack audioStreamTrack;
        public AudioSourceRecorder audioRecorder;

        public SampleRemoteLoopbackManager remoteLoopbackManager;
        public SampleAvatarEntity remoteAvatar;
        public SampleAvatarEntity localAvatar;

        public bool useAvatar = true;

        
        
        void Start()
        {
            // IMPORTANT
            StartCoroutine(WebRTC.Update());
            pcs = new Dictionary<string, RTCPeerConnection>();
            OnCall();
            audioRecorder = gameObject.GetComponent<AudioSourceRecorder>();
            
            
            // ad-hoc
            sendingAudioSource = gameObject.GetComponent<AudioSource>();

            

        }

        void OnCall()
        {
            Debug.Log("[WebRTC Client] Connecting to the socket...", gameObject);
            socket = InitializeSocket();
            AddSocketEvents();
            socket.Connect();
        }

        private void Update()
        {
            if (dataChannel != null)
            {
                if (dataChannel.ReadyState == RTCDataChannelState.Open)
                {
                    // SendMsg();
                    if (useAvatar)
                    {
                        Debug.Log("useAvatar");
                        SendAvatarPose();
                    }
                }
            }
        }
        private float[] audioData;
        private int audioDataLength = 0;
        private bool isRecording = false;    
        private AudioSource audioSource;
        private int channelNum;
        public bool isReceived = false;

        
        private void StartRecording()
        {
            isRecording = true;
            audioDataLength = 0;
            channelNum = 1;

            Debug.Log("recording started");
        }

  


        private SocketIOUnity InitializeSocket()
        {
            var uri = new Uri(socketUri);
            SocketIOUnity socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                Query = new Dictionary<string, string>
                {
                    { "app", "UNITY" }
                },
                EIO = 4,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });
            socket.JsonSerializer = new NewtonsoftJsonSerializer(); 
            
            socket.OnConnected += (sender, e) =>
            {
                socketId = socket.Id;
                Debug.Log($"[WebRTC Client] Successfully connected to the socket! My socket ID is ... <b>{socketId}</b>");
                
            };
            
            socket.OnPing += (sender, e) => { Debug.Log("[WebRTC Client] Ping"); };
            
            socket.OnPong += (sender, e) => { Debug.Log("[WebRTC Client] Pong: " + e.TotalMilliseconds); };
            
            socket.OnDisconnected += (sender, e) => { Debug.Log("[WebRTC Client] Disconnected: " + e); };
            
            socket.OnReconnectAttempt += (sender, e) => { Debug.Log($"[WebRTC Client] {DateTime.Now} Reconnecting: attempt = {e}"); };
            
            return socket;
        }

        private void AddSocketEvents()
        {
            socket.OnUnityThread("client", (response) =>
            {
                others = response.GetValue<string[]>();

                if (!isWaitingAnswer)
                {
                    for (int i = 0; i < others.Length; i++)
                    {
                        RTCPeerConnection pc = CreatePeerConnection(others[i]);
                        pcs[others[i]] = pc;
                    }
                }
            }); 
            
            socket.OnUnityThread("offer", (response) =>
            {
                // This is for creating and sending 
                if (isWaitingAnswer) return;
                var offerData = response.GetValue<OfferData>();
                currentDest = offerData.offerSocketId;
                pcs[currentDest] = CreatePeerConnection(currentDest);
                StartCoroutine(CreateAnswer(pcs[currentDest], offerData.offer));
                isWaitingEnd = true;

            });
            
            socket.OnUnityThread("answer", (response) =>
            {
                if (isWaitingEnd) return; 
                var answerData = response.GetValue<AnswerData>();
                var pc = pcs[answerData.answerSocketId];
                StartCoroutine(ProcessAnswer(pc, answerData.answer));
            });
       
            socket.OnUnityThread("candidate", (response) =>
            {
                CandidateData cd = response.GetValue<CandidateData>();
                RTCIceCandidate cand = cd.candidate.GetRTCIceCandidate();
                string other = "";
                if (Array.IndexOf(others, cd.destSocketId)!=-1)
                {
                    other = cd.destSocketId;
                }   
                if (Array.IndexOf(others, cd.fromSocketId)!=-1)
                {
                    other = cd.fromSocketId;
                }

                if (other == "") return;
                
                if (pcs[other] != null)
                {
                    pcs[other].AddIceCandidate(cand); 
                }
            }); 

            // TODO
        }
        
        private void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
        {
            string pcName = "";
            foreach (KeyValuePair<string, RTCPeerConnection> _pc in pcs)
            {
                if (pc == _pc.Value) pcName = _pc.Key;
            }    
            
            switch (state)
            {
                case RTCIceConnectionState.New:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: New");
                    break;
                case RTCIceConnectionState.Checking:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: Checking");
                    break;
                case RTCIceConnectionState.Closed:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: Closed");
                    break;
                case RTCIceConnectionState.Completed:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: Completed");
                    break;
                case RTCIceConnectionState.Connected:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: Connected");
                    break;
                case RTCIceConnectionState.Disconnected:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: Disconnected");
                    break;
                case RTCIceConnectionState.Failed:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: Failed");
                    break;
                case RTCIceConnectionState.Max:
                    Debug.Log($"[RTC Client] {pcName} IceConnectionState: Max");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        

        RTCPeerConnection CreatePeerConnection(string dest)
        {
            var stream = new MediaStream();
            audioStreamTrack = new AudioStreamTrack(receivedAudio);
            audioStreamTrack.Loopback = true;

            CaptureAudioStart();
            
            
            // Create Peer Connection
            var configuration = GetSelectedSdpSemantics();
            RTCPeerConnection pc = new RTCPeerConnection(ref configuration);

            pc.OnIceCandidate = (candidate) =>
            {
                // When the ICECandidate of PC is created, send the information of the ICECandidate.
                CandidateData c = new CandidateData()
                {
                    destSocketId = dest,
                    fromSocketId = socketId,
                    candidate = ReceivedIceCandidateInfo.Extract(candidate) 
                };
                    
                socket.Emit("candidate", c);
            };
            pc.OnIceConnectionChange = state => { OnIceConnectionChange(pc, state);};
            pc.OnTrack = e =>
            {
                if (e.Track is VideoStreamTrack video)
                {
                    // This part is executed once when the video track is added
                    video.OnVideoReceived += tex =>
                    {
                        // this part is executed every frame from the track
                        receivedVideo.texture = tex;
                    };
                }

                if (e.Track is AudioStreamTrack audioTrack)
                {
                    Debug.Log("[RTC Client] AudioTrack is added.", receivedAudio.gameObject); 
                    // This part is executed once when the audio track is added
                    
                    audioTrack.Loopback = true;
                    receivedAudio.loop = true;
                    receivedAudio.SetTrack(audioTrack);
                    receivedAudio.Play();
                    
                }
            };
            
            // 데이터가 들어왔을 때
            pc.OnDataChannel = channel =>
            {
                remoteDataChannel = channel;
                remoteDataChannel.OnMessage = (bytes) =>
                {
                    // Test
                    // Vec3 val = MemoryPackSerializer.Deserialize<Vec3>(bytes);
                    // receiverObject.position = new Vector3(val.x, val.y, val.z);
                    // packet 데이터를 내 아바타에 적용
                    
                    if (useAvatar)
                    {
                        if (remoteAvatar.isLoaded)
                        {
                            remoteAvatar.ApplyStreamData(bytes);
                        }
                            
                    }
                };
            };
            
            pc.OnNegotiationNeeded = () => StartCoroutine(CreateOffer(pc, dest));
            

            RTCDataChannelInit conf = new RTCDataChannelInit();
            dataChannel = pc.CreateDataChannel("data", conf);
            
            // StartCoroutine(CaptureVideoStart()); 
            videoStreamTrack = cam.CaptureStreamTrack(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y); 
            var videoSender = pc.AddTrack(videoStreamTrack); 
            pc.AddTrack(audioStreamTrack); 
            
            if (WebRTCSettings.UseVideoCodec != null)
            {
                var codecs = new[] { WebRTCSettings.UseVideoCodec };
                var transceiver = pc.GetTransceivers().First(t => t.Sender == videoSender);
                transceiver.SetCodecPreferences(codecs);
            }
            
            return pc;
        }

        public void SendMsg()
        {

            var v = new Vec3()
            {
                x = senderObject.position.x + 50,
                y = senderObject.position.y - 50,
                z = senderObject.position.z
            };
            var bin = MemoryPackSerializer.Serialize(v);
            dataChannel.Send(bin);
        }

        public void SendAvatarPose()
        {
            if (!localAvatar.IsCreated || !localAvatar.isLoaded) return;
            
            Debug.Log("sent avatar packets");
            OvrAvatarEntity.StreamLOD lod = localAvatar.activeStreamLod;
            NativeArray<byte> data = new NativeArray<byte>();
            UInt32 dataByteCount = localAvatar.RecordStreamData_AutoBuffer(lod, ref data);
            Debug.Assert(dataByteCount > 0);

            dataChannel.Send(data);
        }
        
        private void CaptureAudioStart()
        {
            if (!useMic)
            {
                sendingAudioSource.loop = true;
                sendingAudioSource.Play();
                audioStreamTrack = new AudioStreamTrack(sendingAudioSource);
                return;
            }
            
            var deviceName = Microphone.devices[0]; //ad-hoc
            Microphone.GetDeviceCaps(deviceName, out int minFreq, out int maxFreq);
            Debug.Log($"{deviceName} : {minFreq} .... {maxFreq}");
            var micClip = Microphone.Start(deviceName, true, 1, minFreq);

            // set the latency to “0” samples before the audio starts to play.
            while (!(Microphone.GetPosition(deviceName) > 0)) { }

            sendingAudioSource.clip = micClip;
            sendingAudioSource.loop = true;
            sendingAudioSource.Play();
            audioStreamTrack = new AudioStreamTrack(sendingAudioSource);
            audioStreamTrack.Loopback = true;
        }


        
        private IEnumerator CaptureVideoStart()
        {
            videoStreamTrack = cam.CaptureStreamTrack(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y);
            receivedVideo.texture = cam.targetTexture;

            // //
            // if (WebCamTexture.devices.Length == 0)
            // {
            //     Debug.LogFormat("WebCam device not found");
            //     yield break;
            // }
            //
            // yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Debug.LogFormat("authorization for using the device is denied");
                yield break;
            }

            int width = WebRTCSettings.StreamSize.x;
            int height =  WebRTCSettings.StreamSize.y;
            const int fps = 30;
            WebCamDevice userCameraDevice = WebCamTexture.devices[0];
            webCamTexture = new WebCamTexture(userCameraDevice.name, height, height, fps);
            webCamTexture.Play();
            yield return new WaitUntil(() => webCamTexture.didUpdateThisFrame);

            /// Convert texture if the graphicsFormat is not supported.
            /// Since Unity 2022.1, WebCamTexture.graphicsFormat returns R8G8B8A8_SRGB on Android Vulkan.
            /// WebRTC doesn't support the graphics format when using Vulkan, and throw exception.
            var supportedFormat = WebRTC.GetSupportedGraphicsFormat(SystemInfo.graphicsDeviceType);
            if (webCamTexture.graphicsFormat != supportedFormat)
            {
                webcamCopyTexture = new Texture2D(width, height, supportedFormat, TextureCreationFlags.None);
                videoStreamTrack = new VideoStreamTrack(webcamCopyTexture);
                coroutineConvertFrame = StartCoroutine(ConvertFrame());
            }
            else
            {
                videoStreamTrack = new VideoStreamTrack(webCamTexture);
            }

            receivedVideo.texture = webCamTexture;
        }
        
        IEnumerator ConvertFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                Graphics.ConvertTexture(webCamTexture, webcamCopyTexture);
            }
        }
        
        RTCConfiguration GetSelectedSdpSemantics() {
            RTCConfiguration config = default;
            config.iceTransportPolicy = RTCIceTransportPolicy.All;
            config.iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302", "stun:stun1.l.google.com:19302", "stun:stun2.l.google.com:19302", "stun:stun3.l.google.com:19302", "stun:stun4.l.google.com:19302" } } };
            return config;
        }
        
        IEnumerator CreateAnswer(RTCPeerConnection pc, RTCSessionDescription desc) {
        
            var op1 = pc.SetRemoteDescription(ref desc);
            yield return op1; // necessary
            if (op1.IsError) {
                var error = op1.Error;
                Debug.LogError($"Error Detail Type: {error.message}");
            }
        
            var op2 = pc.CreateAnswer();
            yield return op2; // necessary
            if (!op2.IsError) {
                yield return OnCreateAnswerSuccess(pc, op2.Desc);
            }
        }
        
        IEnumerator ProcessAnswer(RTCPeerConnection pc, RTCSessionDescription desc) {
        
            var op1 = pc.SetRemoteDescription(ref desc);
            yield return op1; // necessary
            
            if (op1.IsError) {
                var error = op1.Error;
                Debug.LogError($"[WebRTC Client] Error Detail Type: {error.message}", gameObject);
            }
            
            Debug.Log("[WebRTC Client] Answer was processed", gameObject);
            
            yield return new WaitForSeconds(1.0f);
            
            Debug.Log($"[WebRTC Client] Signaling state is <b>{pc.SignalingState}</b>", gameObject);
            Debug.Log($"[WebRTC Client] Connection state is <b> {pc.ConnectionState}</b>", gameObject);
        }
        
        IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc) {
            var op1 = pc.SetLocalDescription(ref desc);
            yield return op1;
            if (op1.IsError) {
                var error = op1.Error;
                Debug.LogError($"[WebRTC Client] Error Detail Type: {error.message}", gameObject);
            }

            var answer = new AnswerData()
            {
                offerSocketId = currentDest,
                answerSocketId = socketId,
                answer = desc
            };
            
            socket.Emit("answer", answer);
            yield return null;
        }

        IEnumerator CreateOffer (RTCPeerConnection pc, string answerSocektId)
        {
            var op1 = pc.CreateOffer();
            yield return op1;
            
            StartCoroutine(OnCreateOfferSuccess(pc, answerSocektId, op1.Desc));
        }

        IEnumerator OnCreateOfferSuccess(RTCPeerConnection pc, string answerSocektId, RTCSessionDescription desc)
        {
            var op2 = pc.SetLocalDescription(ref desc);

            yield return op2; 
            
            OfferData offerData = new OfferData()
            {
                answerSocketId = answerSocektId,
                offer = desc,
                offerSocketId = socketId,
                enableMediaStream = true, // ad-hoc
                enableDataChannel = false // ad-hoc
            };
            
            socket.Emit("offer", offerData); 
            isWaitingAnswer = true;
        }
        
        void OnHangUp()
        {
            // TODO : dispose things
            audioStreamTrack?.Dispose();
            audioStreamTrack = null;
            videoStreamTrack?.Dispose();
            videoStreamTrack = null;

            foreach (KeyValuePair<string, RTCPeerConnection> entry in pcs)
                entry.Value.Dispose();

            pcs = null;
            
            // 1. Peer Connection Dispose
            // 2. Stream/ Track Dispose
            // 3. Data 반영되는 요소 Dispose
            
        }

        private void OnDestroy()
        {
            // socket.Disconnect();
            socket.Dispose();
        }
    }
}