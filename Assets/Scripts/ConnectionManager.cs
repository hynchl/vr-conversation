// using System;
// using System.Collections;
// using System.Collections.Generic;
// using SocketIOClient;
// using SocketIOClient.Newtonsoft.Json;
// using System.Linq;
// using UnityEngine;
// using UnityEngine.UI;
// using Unity.WebRTC;
// using Oculus.Avatar2;
// using StreamLOD = Oculus.Avatar2.OvrAvatarEntity.StreamLOD;
// using Unity.Collections;
// using UnityEngine.Serialization;
//
// namespace RTC
// {

//     
//     public class ConnectionManager : MonoBehaviour
//     {
//
//         [Header("Connection")]
//         public string socketUri = "http://localhost:5001";
//         public string socketId;
//         string currentDest;
//         public bool isWaitingAnswer = false; // true after sending an offer
//         public bool isWaitingEnd = false; // true after sending an answer
//         
//         public SocketIOUnity socket;
//         
//         [Header("Participants")] 
//         public Client self;
//         private Dictionary<string, RTCPeerConnection> pcs;
//         public Dictionary<string, Client> remotes;
//         public GameObject remotePrefab;
//         
//         void Start()
//         {
//             StartCoroutine(WebRTC.Update());
//             
//             pcs = new Dictionary<string, RTCPeerConnection>();
//             remotes = new Dictionary<string, Client>();
//             
//             if(self.useMic)
//                 self.CaptureAudioStart();
//             
//             OnCall();
//             
//             // ad-hoc
//             // audioRecorder = gameObject.GetComponent<AudioSourceRecorder>();
//         }
//         
//         private void OnDestroy()
//         {
//             socket.Dispose();
//         }
//         
//
//         
//         
//         #region connection
//
//         
//         void OnCall()
//         
//         {
//             Debug.Log("[WebRTC Client] Connecting to the socket...", gameObject);
//             SocketIOUnity _socket = InitializeSocket();
//             socket = AddSocketEvents(_socket);
//             socket.Connect();
//         }
//         
//         
//         private SocketIOUnity InitializeSocket()
//         {
//             var uri = new Uri(socketUri);
//             SocketIOUnity socket = new SocketIOUnity(uri, new SocketIOOptions
//             {
//                 Query = new Dictionary<string, string>
//                 {
//                     { "app", "UNITY" }
//                 },
//                 EIO = 4,
//                 Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
//             });
//             socket.JsonSerializer = new NewtonsoftJsonSerializer(); 
//             
//             socket.OnConnected += (sender, e) =>
//             {
//                 socketId = socket.Id;
//                 Debug.Log($"[WebRTC Client] Successfully connected to the socket! My socket ID is ... <b>{socketId}</b>");
//                 
//             };
//             
//             socket.OnPing += (sender, e) => { Debug.Log("[WebRTC Client] Ping"); };
//             
//             socket.OnPong += (sender, e) => { Debug.Log("[WebRTC Client] Pong: " + e.TotalMilliseconds); };
//             
//             socket.OnDisconnected += (sender, e) => { Debug.Log("[WebRTC Client] Disconnected: " + e); };
//             
//             socket.OnReconnectAttempt += (sender, e) => { Debug.Log($"[WebRTC Client] {DateTime.Now} Reconnecting: attempt = {e}"); };
//             
//             return socket;
//         }
//         
//         private SocketIOUnity AddSocketEvents(SocketIOUnity socket)
//         {
//             socket.OnUnityThread("client", (response) =>
//             {
//                 if (isWaitingAnswer) return;
//                 string [] others = response.GetValue<string[]>();
//                 
//                 for (int i = 0; i < others.Length; i++)
//                 {
//                     GameObject remote = GameObject.Instantiate(remotePrefab);
//                     remotes[others[i]] = remote.GetComponent<Client>();
//                     remotes[others[i]].pcName = others[i];
//                     remotes[others[i]].connectionManager = this;
//                     remotes[others[i]].self = self;
//                     remotes[others[i]].pc = remotes[others[i]].CreatePeerConnection();
//                     
//                     pcs[others[i]] = remotes[others[i]].pc;
//                 }
//                 
//             }); 
//             
//             socket.OnUnityThread("offer", (response) =>
//             {
//                 // This is for creating and sending 
//                 if (isWaitingAnswer) return;
//                 
//                 var offerData = response.GetValue<OfferData>();
//                 currentDest = offerData.offerSocketId;
//                 GameObject remote = GameObject.Instantiate(remotePrefab);
//                 remotes[currentDest] = remote.GetComponent<Client>();
//                 remotes[currentDest].pcName = currentDest;
//                 remotes[currentDest].connectionManager = this;
//                 remotes[currentDest].self = self;
//                 remotes[currentDest].pc = remotes[currentDest].CreatePeerConnection();
//                 pcs[currentDest] = remotes[currentDest].pc;
//                 
//                 StartCoroutine(CreateAnswer(pcs[currentDest], offerData.offer));
//                 isWaitingEnd = true;
//
//             });
//             
//             socket.OnUnityThread("answer", (response) =>
//             {
//                 if (isWaitingEnd) return; 
//                 var answerData = response.GetValue<AnswerData>();
//                 var pc = remotes[answerData.answerSocketId].pc;
//                 StartCoroutine(ProcessAnswer(pc, answerData.answer));
//             });
//        
//             socket.OnUnityThread("candidate", (response) =>
//             {
//                 CandidateData cd = response.GetValue<CandidateData>();
//                 RTCIceCandidate cand = cd.candidate.GetRTCIceCandidate();
//                 string other = "";
//
//                 if (socketId != cd.destSocketId)
//                 {
//                     other = cd.destSocketId;
//                 }   
//                 if (socketId != cd.fromSocketId)
//                 {
//                     other = cd.fromSocketId;
//                 }
//                 
//                 
//                 if (pcs[other] != null)
//                 {
//                     pcs[other].AddIceCandidate(cand); 
//                 }
//             });
//
//             return socket;
//         }
//
//         
//         IEnumerator CreateAnswer(RTCPeerConnection pc, RTCSessionDescription desc) {
//         
//             var op1 = pc.SetRemoteDescription(ref desc);
//             yield return op1; // necessary
//             if (op1.IsError) {
//                 var error = op1.Error;
//                 Debug.LogError($"Error Detail Type: {error.message}");
//             }
//         
//             var op2 = pc.CreateAnswer();
//             yield return op2; // necessary
//             if (!op2.IsError) {
//                 yield return OnCreateAnswerSuccess(pc, op2.Desc);
//             }
//         }
//         
//         IEnumerator ProcessAnswer(RTCPeerConnection pc, RTCSessionDescription desc) {
//         
//             var op1 = pc.SetRemoteDescription(ref desc);
//             yield return op1; // necessary
//             
//             if (op1.IsError) {
//                 var error = op1.Error;
//                 Debug.LogError($"[WebRTC Client] Error Detail Type: {error.message}", gameObject);
//             }
//             
//             Debug.Log("[WebRTC Client] Answer was processed", gameObject);
//             
//             yield return new WaitForSeconds(1.0f);
//             
//             Debug.Log($"[WebRTC Client] Signaling state is <b>{pc.SignalingState}</b>", gameObject);
//             Debug.Log($"[WebRTC Client] Connection state is <b> {pc.ConnectionState}</b>", gameObject);
//         }
//         
//         IEnumerator OnCreateAnswerSuccess(RTCPeerConnection pc, RTCSessionDescription desc) {
//             var op1 = pc.SetLocalDescription(ref desc);
//             yield return op1;
//             if (op1.IsError) {
//                 var error = op1.Error;
//                 Debug.LogError($"[WebRTC Client] Error Detail Type: {error.message}", gameObject);
//             }
//
//             var answer = new AnswerData()
//             {
//                 offerSocketId = currentDest,
//                 answerSocketId = socketId,
//                 answer = desc
//             };
//             
//             socket.Emit("answer", answer);
//             yield return null;
//         }
//         
//         #endregion
//         
//         
//     }
// }