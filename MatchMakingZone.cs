using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun; // 유니티용 포톤 컴포넌트
using Photon.Realtime; // 포톤 서비스 관련 라이브러리

public class MatchMakingZone : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1.0";
    public MFBInputManager playerInput;
    public GameObject PlayerPrefab;

    private void Start()
    {
        //CreatePlayer();
    }

    private void Update()
    {            
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ConnectToMasterServer();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && playerInput.interact)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    void CreatePlayer()
    {
        Vector3 spawnPosition = Random.insideUnitSphere * 5f;
        spawnPosition.y = 0f;

        PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPosition, Quaternion.identity);
    }

    // 마스터서버 접속
    public void ConnectToMasterServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Try Connect to Master Server");
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // 마스터 서버에 접속 성공했을 때 자동으로 호출되는 콜백 함수
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
    }

    // 랜덤 룸 접속
    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Join Random Room");
            PhotonNetwork.JoinRandomRoom(); // 무작위로 추출된 룸으로 입장
        }
        else
        {
            Debug.Log("Offline : Retry Connect to Master Server");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // 마스터 서버에 있는 룸 조인에 성공하면 씬 전환
    public override void OnJoinedRoom()
    {
        Debug.Log("Enter Room");
        
        // 카운트 다운 시작후 게임 진입 씬 전환 구현 또는 룸에 인원이 20명이 되어야 게임 시작 등 구현

        PhotonNetwork.LoadLevel(1);
    }

    // 랜덤으로 방 접속을 시도하였으나 실패 했을 때 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No Rooms, Create Room");
        // 정원 20명 짜리 방을 생성
        PhotonNetwork.CreateRoom(null ,new RoomOptions { MaxPlayers = 20 });
    }
}
