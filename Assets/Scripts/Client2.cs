using System;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using MemoryPack;
using Random = UnityEngine.Random;

namespace RTC
{
    
     public static class WebRTCSettings
     {
         public const int DefaultStreamWidth = 1280;
         public const int DefaultStreamHeight = 720;

         private static Vector2Int s_StreamSize = new Vector2Int(DefaultStreamWidth, DefaultStreamHeight);
         private static RTCRtpCodecCapability s_useVideoCodec = null;

         public static Vector2Int StreamSize
         {
             get { return s_StreamSize; }
             set { s_StreamSize = value; }
         }

         public static RTCRtpCodecCapability UseVideoCodec
         {
             get { return s_useVideoCodec; }
             set { s_useVideoCodec = value; }
         }
     }
    public class Client2 : MonoBehaviour
    {
        [Header("Configuration")]
        public string socketUri = "http://localhost:5001";
        public RawImage receivedVideo;
        public AudioSource receivedAudio;
        public AudioSource sendingAudioSource;
        public bool useMic;
        public Transform senderObject;
        public Transform receiverObject;
        public GameObject audioReceiverPrefab;
        private RTCDataChannel dataChannel, remoteDataChannel;
        
        [SerializeField] private Camera cam;
        private WebCamTexture webCamTexture;
        private Texture2D webcamCopyTexture;
        private Coroutine coroutineConvertFrame;
        
        [Header("Status")]
        public string currentDest;
        public string socketId;
        public string[] others;
        bool isOfferSent = false; // true after sending an offer
        bool isAnswerSent = false; // true after sending an answer
        
        private SocketIOUnity socket;
        private RTCPeerConnection pc;
        private Dictionary<string, RTCPeerConnection> pcs;

        private VideoStreamTrack videoStreamTrack;
        private AudioStreamTrack audioStreamTrack;

        [Header("Development")]
        public Color debugColor;
        void Start()
        {
            
            // IMPORTANT
            StartCoroutine(WebRTC.Update());
            pcs = new Dictionary<string, RTCPeerConnection>();
            // sendingAudioSource = gameObject.GetComponent<AudioSource>();
            
            OnCall();
        }

        void OnCall()
        {
            Log(" Connecting to the socket...", gameObject);
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
                    SendMsg();
                }
            }

        }

        private void Log(string str, GameObject go=null)
        {
            Debug.Log($"[RTC Client] <color=#{debugColor.ToHexString().Substring(0,6)}> {str} </color>", go);
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
                Log($" Successfully connected to the socket! My socket ID is ... <b>{socketId}</b>");
                
            };
            
            // socket.OnPing += (sender, e) => { Log(" Ping"); };
            //
            // socket.OnPong += (sender, e) => { Log(" Pong: " + e.TotalMilliseconds); };
            //
            socket.OnDisconnected += (sender, e) => { Log(" Disconnected: " + e); };
            
            socket.OnReconnectAttempt += (sender, e) => { Log($" {DateTime.Now} Reconnecting: attempt = {e}"); };
            
            return socket;
        }

        private void AddSocketEvents()
        {
            socket.OnUnityThread("client", (response) =>
            {
                others = response.GetValue<string[]>();
                
                for (int i = 0; i < others.Length; i++)
                {
                    RTCPeerConnection pc = CreatePeerConnection(others[i]);
                    pcs[others[i]] = pc;
                }
            }); 
            
            socket.OnUnityThread("offer", (response) =>
            {
                // This is for creating and sending 
                if (isOfferSent) return;
                
                Log("An offer was received");
                var offerData = response.GetValue<OfferData>();
                currentDest = offerData.offerSocketId;
                pcs[currentDest] = CreatePeerConnection(currentDest);
                StartCoroutine(CreateAnswer(pcs[currentDest], offerData.offer));
                isAnswerSent = true;

            });
            
            socket.OnUnityThread("answer", (response) =>
            {
                if (isAnswerSent) return; 
                Log($"The answer was received");
                var answerData = response.GetValue<AnswerData>();
                var pc = pcs[answerData.answerSocketId];
                StartCoroutine(ProcessAnswer(pc, answerData.answer));
                
            });
       
            socket.OnUnityThread("candidate", (response) =>
            {
                if (isAnswerSent) return;
                
                Log("The ICECandidate Information was received");
                CandidateData cd = response.GetValue<CandidateData>();
                RTCIceCandidate cand = cd.candidate.GetRTCIceCandidate();
                string other = "";
                if (socketId != cd.destSocketId)
                {
                    other = cd.destSocketId;
                }   
                if (socketId != cd.fromSocketId)
                {
                    other = cd.fromSocketId;
                }
                
                pcs[other].AddIceCandidate(cand); 
            }); 
            
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
                    Log($"IceConnectionState: New");
                    break;
                case RTCIceConnectionState.Checking:
                    Log($"IceConnectionState: Checking");
                    break;
                case RTCIceConnectionState.Closed:
                    Log($"IceConnectionState: Closed");
                    break;
                case RTCIceConnectionState.Completed:
                    Log($"IceConnectionState: Completed");
                    break;
                case RTCIceConnectionState.Connected:
                    Log($"IceConnectionState: Connected");
                    break;
                case RTCIceConnectionState.Disconnected:
                    Log($"IceConnectionState: Disconnected");
                    break;
                case RTCIceConnectionState.Failed:
                    Log($"IceConnectionState: Failed");
                    break;
                case RTCIceConnectionState.Max:
                    Log($"IceConnectionState: Max");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
        

        RTCPeerConnection CreatePeerConnection(string dest)
        {
            // var stream = new MediaStream();
            // audioStreamTrack = new AudioStreamTrack(receivedAudio);
            // audioStreamTrack.Loopback = true;

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
                
                
                Log("The ICECandidate information was sent");
                socket.Emit("candidate", c);
            };
            
            // for debugging
            // pc.OnIceConnectionChange = state => { OnIceConnectionChange(pc, state);};
            pc.OnIceConnectionChange = state => { Log($"The current ICEConnection state is <b>{state}</b>"); };
            pc.OnConnectionStateChange = state => { Log($"The current connection state is <b>{state}</b>"); };

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
                    Log("A video track was added.", receivedVideo.gameObject); 
                }

                if (e.Track is AudioStreamTrack audioTrack)
                {
                    
                    // This part is executed once when the audio track is added
                    // receivedAudio.SetTrack(audioTrack);
                    // receivedAudio.loop = true;
                    // receivedAudio.Play();
                    
                    // change
                    // AudioSource as = Instantiate(audioReceiverPrefab).GetComponent<AudioSource>();
//                     as.SetTrack(audioTrack);
//                     as.loop = true;
//                     as.Play();
// }
                    GameObject go = Instantiate(audioReceiverPrefab);
                    go.name = gameObject.name + " - received audio";
                    go.GetComponent<AudioSource>().SetTrack(audioTrack);
                    go.GetComponent<AudioSource>().loop = true;
                    go.GetComponent<AudioSource>().Play();
                    Log("An audioTrack was added.", go); 


                }
            };
            
            
            pc.OnDataChannel = channel =>
            {
                // This part is executed once when the audio track is added
                remoteDataChannel = channel;
                remoteDataChannel.OnMessage = (bytes) =>
                {
                    Vec3 val = MemoryPackSerializer.Deserialize<Vec3>(bytes);
                    receiverObject.position = new Vector3(val.x, val.y, val.z);
                    
                };
            };
            
            pc.OnNegotiationNeeded = () =>
            {
                if (!isAnswerSent)
                    StartCoroutine(CreateOffer(pc, dest));
            };
            

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
        
        private void CaptureAudioStart()
        {
            if (!useMic)
            {
                //sendingAudioSource.clip = clip;
                sendingAudioSource.loop = true;
                sendingAudioSource.Play();
                audioStreamTrack = new AudioStreamTrack(sendingAudioSource);
                return;
            }

            foreach (var mic in Microphone.devices)
            {
                Debug.Log(mic);
            }
            
#if UNITY_EDITOR
            //var deviceName = "마이크 배열 (Realtek(R) Audio)"; //Microphone.devices[0]; //ad-hoc
            var deviceName = "Headset Microphone (Oculus Virtual Audio Device)";
#else   
            var deviceName = Microphone.devices[0];
#endif 
            // ad-hoc
            Microphone.GetDeviceCaps(deviceName, out int minFreq, out int maxFreq);
            Log($"Mic name : {deviceName} ({minFreq}, {maxFreq})");
            var micClip = Microphone.Start(deviceName, true, 10, maxFreq);

            // set the latency to “0” samples before the audio starts to play.
            while (!(Microphone.GetPosition(deviceName) > 0)) { }

            sendingAudioSource.clip = micClip;
            sendingAudioSource.loop = true;
            sendingAudioSource.Play();
            audioStreamTrack = new AudioStreamTrack(sendingAudioSource);
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
                Debug.LogError($" Error Detail Type: {error.message}", gameObject);
            }
            
            Log("The answer was processed", gameObject);
        }
        
        IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc) {
            var op1 = pc.SetLocalDescription(ref desc);
            yield return op1;
            if (op1.IsError) {
                var error = op1.Error;
                Debug.LogError($" Error Detail Type: {error.message}", gameObject);
            }

            var answer = new AnswerData()
            {
                offerSocketId = currentDest,
                answerSocketId = socketId,
                answer = desc
            };
            
            socket.Emit("answer", answer);
            Log("The answer was sent");
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
                enableMediaStream = true, 
                enableDataChannel = true 
            };
            
            socket.Emit("offer", offerData); 
            Log("An offer was sent");
            isOfferSent = true;
        }
        
        void OnHangUp()
        {
            // 1. Peer Connection Dispose
            // 2. Stream/ Track Dispose
            // 3. Receiver Dispose
            
            audioStreamTrack?.Dispose();
            audioStreamTrack = null;
            videoStreamTrack?.Dispose();
            videoStreamTrack = null;

            foreach (KeyValuePair<string, RTCPeerConnection> entry in pcs)
                entry.Value.Dispose();

            pcs = null;
        }

        private void OnDestroy()
        {
            
            socket.Dispose();
            OnHangUp();
        }
    }
}