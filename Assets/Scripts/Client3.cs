using System;
using System.Collections;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using Unity.WebRTC;
using MemoryPack;
using Oculus.Avatar2;
using Unity.Collections;
using UnityEngine.Serialization;

namespace RTC
{

    [MemoryPackable]
    public partial class AvatarPack
        {
            public bool isValidFaceExpressions;
            public Vector3 position;
            public Quaternion rotation;
            public Dictionary<string, float> faceExpressions;
            public Dictionary<string, float> eyeData;
            public byte[] pose;
        }
        
    public class Client3 : MonoBehaviour
    {

        [Header("Configuration")]
        public string socketUri = "http://localhost:5001";
        public AudioSource receivedAudio;
        public AudioSource sendingAudioSource;
        public bool useMic;
        public Transform senderObject;
        public Transform receiverObject;
        public GameObject audioReceiverPrefab;
        // private RTCDataChannel dataChannel, remoteDataChannel;
        private Dictionary<string, RTCDataChannel> dataChannels;
        private Dictionary<string, RTCDataChannel> remoteDataChannels;
        private Dictionary<string, AudioSource> receivedAudios;
        private Dictionary<string, int> avatarIndices;
        
        [Header("Status")]
        public string currentDest;
        public string socketId;
        public string[] others;
        // bool isOfferSent = false; // true after sending an offer
        // bool isAnswerSent = false; // true after sending an answer
        private Dictionary<string, bool> isOfferSents;
        private Dictionary<string, bool> isAnswerSents;
        
        private SocketIOUnity socket;
        // private RTCPeerConnection pc;
        private Dictionary<string, RTCPeerConnection> pcs;
        
        private AudioStreamTrack audioStreamTrack;
        private Dictionary<string, AudioStreamTrack> AudioStreamTracks;
        

        [Header("Development")]
        public Color debugColor;

        private Dictionary<string, SampleAvatarEntity[]> remoteAvatars;
        // SampleAvatarEntity remoteAvatar;
        public SampleAvatarEntity localAvatar;
        public GameObject AvatarReceiverPrefab;
        public bool useAvatar = true;
        public OVRFaceExpressions faceExpressions;
        public AvatarPack currentAvatarState;
        public EyeTrackingWithSdf[] eyeTrackingWithSdfs;
        public SelfRecorder selfRecorder;
        void Start()
        {
            currentAvatarState = new AvatarPack();
            avatarIndices = new Dictionary<string, int>();
            receivedAudios = new Dictionary<string, AudioSource>();
            // IMPORTANT
            StartCoroutine(WebRTC.Update());
            pcs = new Dictionary<string, RTCPeerConnection>();
            isOfferSents = new Dictionary<string, bool>();
            isAnswerSents = new Dictionary<string, bool>();
            remoteAvatars = new Dictionary<string, SampleAvatarEntity[]>();
            dataChannels = new Dictionary<string, RTCDataChannel>();
            remoteDataChannels = new Dictionary<string, RTCDataChannel>();
            AudioStreamTracks = new Dictionary<string, AudioStreamTrack>();
            
            CaptureAudioStart();
            OnCall();
            
            //
            eyeTrackingWithSdfs = FindObjectsOfType<EyeTrackingWithSdf>();
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
            SendSignal();
                
            if (useAvatar)
            {
                // Debug.Log("useAvatar");
                SendAvatarPose();
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
            

            socket.OnDisconnected += (sender, e) => { Log(" Disconnected: " + e); };
            
            socket.OnReconnectAttempt += (sender, e) => { Log($" {DateTime.Now} Reconnecting: attempt = {e}"); };
            
            return socket;
        }

        private void SendSignal()
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                socket.Emit("start", "null");
                Debug.Log("Recording is started.");
            }

            if (Input.GetKeyUp(KeyCode.Q))
            {
                socket.Emit("end", "null");
            Debug.Log("Recording is done.");
                
            }
            
            if (Input.GetKeyUp(KeyCode.Alpha3))
            {
                socket.Emit("evaluation", "null");
            }
        }
        
        private void AddSocketEvents()
        {
            socket.OnUnityThread("start", (response) =>
            {
                Debug.Log("start-yes");
                GameManager.instance.expOperator.SetActive(true);
            }); 
            
            socket.OnUnityThread("end", (response) =>
            {
                Debug.Log("end-yes");
                GameManager.instance.expOperator.SetActive(false);
            }); 
            
            socket.OnUnityThread("evaluation", (response) =>
            {
                Debug.Log("evaluation-yes");
                GameManager.instance.StartEvaluation();
            }); 

            
            socket.OnUnityThread("client", (response) =>
            {

                others = response.GetValue<string[]>();
                
                for (int i = 0; i < others.Length; i++)
                {
                    
                    bool value;
                    isOfferSents.TryGetValue(others[i], out value);
                    if (value) continue;
                    
                    RTCPeerConnection pc = CreatePeerConnection(others[i]);
                    pcs[others[i]] = pc;
                }
            }); 
            
            socket.OnUnityThread("offer", (response) =>
            {
                // This is for creating and sending 
                
                Log("An offer was received");
                var offerData = response.GetValue<OfferData>();
                currentDest = offerData.offerSocketId;
                avatarIndices[currentDest] = offerData.avatarIndex;
                
                bool value = false;
                isOfferSents.TryGetValue(currentDest, out value);
                if (value) return;
                
                pcs[currentDest] = CreatePeerConnection(currentDest);
                StartCoroutine(CreateAnswer(pcs[currentDest], offerData.offer));
                // isAnswerSent = true;
                isAnswerSents[currentDest] = true;
            });
            
            socket.OnUnityThread("answer", (response) =>
            {
                // if (isAnswerSent) return;

                Log($"The answer was received");
                var answerData = response.GetValue<AnswerData>();
                avatarIndices[answerData.answerSocketId] = answerData.avatarIndex;
                
                bool value = false;
                isAnswerSents.TryGetValue(answerData.answerSocketId, out value);
                if (value) return;
                
                var pc = pcs[answerData.answerSocketId];
                StartCoroutine(ProcessAnswer(pc, answerData.answer));
                
            });
       
            socket.OnUnityThread("candidate", (response) =>
            {
                // 왜 이걸 플래그로 사용한ㄴ지 모호함
                // if (isAnswerSent) return;
                
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
                
                bool value = false;
                isAnswerSents.TryGetValue(other, out value);
                if (value) return;
                
                // TEMPORARY FOR DEBUGGING
                Log(this.gameObject.name);
                RTCPeerConnection _pc;
                if (pcs.TryGetValue(other, out _pc))
                {
                    _pc.AddIceCandidate(cand);
                }
                
                // pcs[other].AddIceCandidate(cand); 
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
                    
                    // Remove RTC client receier objects
                    Log(pcName);
                    GameObject _go = receivedAudios[pcName].gameObject;
                    receivedAudios[pcName] = null;
                    Destroy(_go);
                    
                    _go = remoteAvatars[pcName][0].transform.parent.gameObject;
                    remoteAvatars[pcName] = null;
                    Destroy(_go);
                    
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
            pc.OnIceConnectionChange = state => { OnIceConnectionChange(pc, state);};
            // pc.OnIceConnectionChange = state => { Log($"The current ICEConnection state is <b>{state}</b>"); };
            pc.OnConnectionStateChange = state => { Log($"The current connection state is <b>{state}</b>"); };

            pc.OnTrack = e =>
            {
                // if (e.Track is VideoStreamTrack video)
                // {
                //     // This part is executed once when the video track is added
                //     video.OnVideoReceived += tex =>
                //     {
                //         // this part is executed every frame from the track
                //         receivedVideo.texture = tex;
                //     };
                //     Log("A video track was added.", receivedVideo.gameObject); 
                // }

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

                    receivedAudios[dest] = go.GetComponent<AudioSource>();
                }
            };

            
            // var _remoteAvatar = remoteAvatars[dest];
            
            pc.OnDataChannel = channel =>
            {
                Log($"On Data Channel ===> {dest}");
                remoteDataChannels[dest] = channel;
                
                if (avatarIndices[dest] == -1) return;
                
                remoteAvatars[dest] = Instantiate(AvatarReceiverPrefab).GetComponentsInChildren<SampleAvatarEntity>();
                remoteAvatars[dest][0]._assets[0] = new SampleAvatarEntity.AssetData() { path = avatarIndices[dest].ToString(), source = OvrAvatarEntity.AssetSource.Zip};
                remoteAvatars[dest][1]._assets[0] = new SampleAvatarEntity.AssetData() { path = avatarIndices[dest].ToString(), source = OvrAvatarEntity.AssetSource.Zip};
                
                remoteAvatars[dest][0].transform.parent.gameObject.name = $"remote avatar {socketId} | {dest}";
                remoteAvatars[dest][0].transform.parent.GetComponent<RemoteRecorder>().fileName = $"remote_{dest}";
                remoteDataChannels[dest].OnMessage = (bytes) =>
                {
                    AvatarPack ap = MemoryPackSerializer.Deserialize<AvatarPack>(bytes);
                    remoteAvatars[dest][0].transform.parent.GetComponent<RemoteRecorder>().UpdateRemoteInfo(ap);
                    NativeArray<byte> _pose = new NativeArray<byte>(ap.pose, Allocator.Temp);
                    
                    if (remoteAvatars[dest] != null)
                    {
                        for (int i = 0; i < remoteAvatars[dest].Length; i++)
                        {
                            if (remoteAvatars[dest][i].isLoaded)
                            {
                                remoteAvatars[dest][i].transform.position = ap.position;
                                remoteAvatars[dest][i].transform.rotation = ap.rotation;
                                remoteAvatars[dest][i].ApplyStreamData(_pose);
                            }
                        }
                    }
                };
            };
            
            pc.OnNegotiationNeeded = () =>
            {
                bool value = false;
                isAnswerSents.TryGetValue(dest, out value);
                if (!value)
                    StartCoroutine(CreateOffer(pc, dest));
            };
            

            RTCDataChannelInit conf = new RTCDataChannelInit();
            // dataChannel = pc.CreateDataChannel("data", conf);
            dataChannels[dest] = pc.CreateDataChannel("data", conf);
            // StartCoroutine(CaptureVideoStart()); 
            
            // videoStreamTrack = cam.CaptureStreamTrack(WebRTCSettings.StreamSize.x, WebRTCSettings.StreamSize.y); 
            // var videoSender = pc.AddTrack(videoStreamTrack); 
            // AudioStreamTracks[dest] = new AudioStreamTrack(sendingAudioSource);
            pc.AddTrack(audioStreamTrack); 
            
            // if (WebRTCSettings.UseVideoCodec != null)
            // {
            //     var codecs = new[] { WebRTCSettings.UseVideoCodec };
            //     var transceiver = pc.GetTransceivers().First(t => t.Sender == videoSender);
            //     transceiver.SetCodecPreferences(codecs);
            // }
            
            return pc;
        }

        // public DelegateOnDataChannel OnDataChannel()
        // {
        //     
        // }

        // public void SendMsg()
        // {
        //
        //     var v = new Vec3()
        //     {
        //         x = senderObject.position.x + 50,
        //         y = senderObject.position.y - 50,
        //         z = senderObject.position.z
        //     };
        //     var bin = MemoryPackSerializer.Serialize(v);
        //     dataChannel.Send(bin);
        // }

        public Dictionary<string, float> GetFaceExpressionWeights()
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            result["timestamp.FACE"] = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (faceExpressions.FaceTrackingEnabled && faceExpressions.ValidExpressions)
            {
                for (int i = 0; i < 70; i++)
                {
                    OVRFaceExpressions.FaceExpression fe = (OVRFaceExpressions.FaceExpression)i;
                    result["FACE." + fe.ToString()] = faceExpressions.GetWeight(fe);
                }
            }

            return result;
        }

        public Dictionary<string, float> GetEyeTrackingData()
        {
            Dictionary<string, float> result = new Dictionary<string, float>();
            result["timestamp.EYE"] = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            // 가져오기
            foreach (EyeTrackingWithSdf e in eyeTrackingWithSdfs)
            {
                string name = e.gameObject.name;
                result[$"EYE.{name}.position.x"] = e.transform.position.x;
                result[$"EYE.{name}.position.y"] = e.transform.position.x;
                result[$"EYE.{name}.position.z"] = e.transform.position.x;
                result[$"EYE.{name}.rotation.x"] = e.transform.rotation.x;
                result[$"EYE.{name}.rotation.y"] = e.transform.rotation.y;
                result[$"EYE.{name}.rotation.z"] = e.transform.rotation.z;
                result[$"EYE.{name}.confidence.x"] = e.GetComponent<OVREyeGaze>().Confidence;
                result[$"EYE.{name}.distance"] = e.value;
            }

            return result;
        }
        
        public void SendAvatarPose()
        {
            if (!localAvatar.IsCreated || !localAvatar.isLoaded) return;
            
            OvrAvatarEntity.StreamLOD lod = OvrAvatarEntity.StreamLOD.High; //localAvatar.activeStreamLod;
            
            // Debug.Log($"sent avatar packets {lod}");
            NativeArray<byte> data = new NativeArray<byte>();
            


            AvatarPack ap = new AvatarPack() { 
                isValidFaceExpressions = faceExpressions.FaceTrackingEnabled && faceExpressions.ValidExpressions,
                faceExpressions = GetFaceExpressionWeights(),
                eyeData = GetEyeTrackingData(),
                position = localAvatar.transform.position,
                rotation = localAvatar.transform.rotation,
            };
            UInt32 dataByteCount = localAvatar.RecordStreamData_AutoBuffer(lod, ref data);
            ap.pose = data.ToArray();
            
            Debug.Assert(dataByteCount > 0);
            var bin = MemoryPackSerializer.Serialize(ap);
            
            foreach (KeyValuePair<string, RTCDataChannel> dc in dataChannels)
            {
                if (dc.Value.ReadyState == RTCDataChannelState.Open)
                {
                    dc.Value.Send(bin);
                }
            }

            selfRecorder.avatarpack = ap;
            // dataChannel.Send(data);
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
                answer = desc,
                avatarIndex = useAvatar?GameManager.instance.selectedAvatar:-1
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
                enableDataChannel = true,
                avatarIndex = useAvatar?GameManager.instance.selectedAvatar:-1
            };
            
            socket.Emit("offer", offerData); 
            Log("An offer was sent");
            // isOfferSent = true;
            isOfferSents[answerSocektId] = true;
        }
        
        void OnHangUp()
        {
            // 1. Peer Connection Dispose
            // 2. Stream/ Track Dispose
            // 3. Receiver Dispose
            
            audioStreamTrack?.Dispose();
            audioStreamTrack = null;

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