// using System.Collections; 
// using System.Collections.Generic; 
// using UnityEngine; 
// using Photon.Pun;
// using Oculus.Avatar2; 
// using System;

// public class NetworkAvatar : OvrAvatarEntity
// {
//     [SerializeField]
//     // int m_avatarToUseInZipFolder = 2;
//     PhotonView m_photonView; 
//     List<byte[]> m_streamedDataList = new List<byte[]>(); 
//     int m_maxBytesToLog = 15;
//     [SerializeField] 
//     int m_instantiationData; 
//     float m_cycleStartTime = 0; 
//     float m_intervalToSendData = 0.08f;
//     bool isSkeletonLoaded = false;

//     [System.Serializable]
//     private struct AssetData
//     {
//         public AssetSource source;
//         public string path;
//     }

//     [Tooltip("Adds an underscore between the path and the postfix.")]
//     [SerializeField]
//     private bool _underscorePostfix = true;

//     [Tooltip("Filename Postfix (WARNING: Typically the postfix is Platform specific, such as \"_rift.glb\")")]
//     [SerializeField]
//     private string _overridePostfix = String.Empty;


//     protected override void Awake()
//     {
//         ConfigureAvatarEntity();
//         base.Awake();
//     }

//     private void Start()
//     {
//         m_instantiationData = GetUserIdFromPhotonInstantiationData();
//         // StartCoroutine(TryToLoadUser());
//         LoadLocalAvatar();


//         m_photonView = transform.parent.GetComponent<PhotonView>();
//         if (m_photonView.IsMine)
//         {
//             SetActiveView(CAPI.ovrAvatar2EntityViewFlags.FirstPerson);
//             Debug.Log(GetActiveView());
//         }
//         else
//         {
//             SetActiveView(CAPI.ovrAvatar2EntityViewFlags.ThirdPerson);
//             Debug.Log(GetActiveView());
//         }


    
//     }
    

//     public enum AssetSource
//     {
//         /// Load from one of the preloaded .zip files
//         Zip,

//         /// Load a loose glb file directly from StreamingAssets
//         StreamingAssets,
//     }
    



//     /// this is from `SampleAvatarentity.cs`
//     private void LoadLocalAvatar()
//     {
//         // Zip asset paths are relative to the inside of the zip.
//         // Zips can be loaded from the OvrAvatarManager at startup or by calling OvrAvatarManager.Instance.AddZipSource
//         // Assets can also be loaded individually from Streaming assets
//         var path = new string[1];
        
//         AssetData asset = new AssetData { source = AssetSource.Zip, path = $"{m_instantiationData}" };

//         string assetPostfix = (_underscorePostfix ? "_" : "")
//             + OvrAvatarManager.Instance.GetPlatformGLBPostfix(true)
//             + OvrAvatarManager.Instance.GetPlatformGLBVersion(_creationInfo.renderFilters.highQualityFlags != CAPI.ovrAvatar2EntityHighQualityFlags.None, true)
//             + OvrAvatarManager.Instance.GetPlatformGLBExtension(true);
//         if (!String.IsNullOrEmpty(_overridePostfix))
//         {
//             assetPostfix = _overridePostfix;
//         }

//         path[0] = asset.path + assetPostfix;
//         Debug.Log(path[0]);
//         LoadAssetsFromZipSource(path);
//     }

//     protected override void OnSkeletonLoaded()
//     {
//         base.OnSkeletonLoaded();
//         isSkeletonLoaded = true;
//     }

//     void ConfigureAvatarEntity()
//     {
//         m_photonView = transform.parent.GetComponent<PhotonView>();
//         if (m_photonView.IsMine)
//         {
//             SetIsLocal(true);
//             // _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Default;
//             // SampleInputManager sampleInputManager = OvrAvatarManager.Instance.gameObject.GetComponent<SampleInputManager>();
//             // SetBodyTracking(sampleInputManager);
//             // OvrAvatarLipSyncContext lipSyncInput = GameObject.FindObjectOfType<OvrAvatarLipSyncContext>();
//             // SetLipSync(lipSyncInput);
//             // SampleEyePoseBehavior sampleEye = OvrAvatarManager.Instance.gameObject.GetComponent<SampleEyePoseBehavior>();
//             // SetEyePoseProvider(sampleEye);
//             // SampleFacePoseBehavior sampleFace = OvrAvatarManager.Instance.gameObject.GetComponent<SampleFacePoseBehavior>();
//             // SetFacePoseProvider(sampleFace);
            
//             gameObject.name = "MyAvatar";
//         }
//         else
//         {
//             SetIsLocal(false);
//             // SetActiveView(CAPI.ovrAvatar2EntityViewFlags.ThirdPerson);
//             // _creationInfo.features = Oculus.Avatar2.CAPI.ovrAvatar2EntityFeatures.Preset_Remote;
//             // SampleInputManager sampleInputManager = OvrAvatarManager.Instance.gameObject.GetComponent<SampleInputManager>();
//             // SetBodyTracking(sampleInputManager);
//             // OvrAvatarLipSyncContext lipSyncInput = GameObject.FindObjectOfType<OvrAvatarLipSyncContext>();
//             // SetLipSync(lipSyncInput);
//             gameObject.name = "OtherAvatar";
//         }
//     }

//     IEnumerator TryToLoadUser()
//     {
        
//         var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
//         while (hasAvatarRequest.IsCompleted == false)
//         {
//             yield return null;
//         }
//         LoadUser();
//     }


    
//     private void LateUpdate()
//     {
//         float elapsedTime = Time.time - m_cycleStartTime;
//         if (elapsedTime > m_intervalToSendData)
//         {
//             RecordAndSendStreamDataIfMine();
//             m_cycleStartTime = Time.time;
//         }

//     }

//     void RecordAndSendStreamDataIfMine()
//     {
//         if (m_photonView.IsMine && isSkeletonLoaded)
//         {
//             byte[] bytes = RecordStreamData(activeStreamLod);
//             m_photonView.RPC("RecieveStreamData", RpcTarget.Others, bytes);
//         }
//     }

//     [PunRPC]
//     public void RecieveStreamData(byte[] bytes)
//     {
//         m_streamedDataList.Add(bytes);
//     }

//     void LogFirstFewBytesOf(byte[] bytes)
//     {
//         for (int i = 0; i < m_maxBytesToLog; i++)
//         {
//             string bytesString = Convert.ToString(bytes[i], 2).PadLeft(8, '0');
//         }
//     }

//     private void Update()
//     {
//         if (m_streamedDataList.Count > 0)
//         {
//             if (IsLocal == false)
//             {
//                 byte[] firstBytesInList = m_streamedDataList[0];
//                 if (firstBytesInList != null)
//                 {
//                     ApplyStreamData(firstBytesInList);
//                 }
//                 m_streamedDataList.RemoveAt(0);
//             }
//         }
//     }

//     int GetUserIdFromPhotonInstantiationData()
//     {
//         PhotonView photonView = transform.parent.GetComponent<PhotonView>();
//         object[] instantiationData = photonView.InstantiationData;
//         int data_as_int = (int)instantiationData[0];
//         return data_as_int;
//     }
// }
