using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun; // ����Ƽ�� ���� ������Ʈ
using Photon.Realtime; // ���� ���� ���� ���̺귯��

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

    // �����ͼ��� ����
    public void ConnectToMasterServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Try Connect to Master Server");
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // ������ ������ ���� �������� �� �ڵ����� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
    }

    // ���� �� ����
    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Join Random Room");
            PhotonNetwork.JoinRandomRoom(); // �������� ����� ������ ����
        }
        else
        {
            Debug.Log("Offline : Retry Connect to Master Server");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // ������ ������ �ִ� �� ���ο� �����ϸ� �� ��ȯ
    public override void OnJoinedRoom()
    {
        Debug.Log("Enter Room");
        
        // ī��Ʈ �ٿ� ������ ���� ���� �� ��ȯ ���� �Ǵ� �뿡 �ο��� 20���� �Ǿ�� ���� ���� �� ����

        PhotonNetwork.LoadLevel(1);
    }

    // �������� �� ������ �õ��Ͽ����� ���� ���� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No Rooms, Create Room");
        // ���� 20�� ¥�� ���� ����
        PhotonNetwork.CreateRoom(null ,new RoomOptions { MaxPlayers = 20 });
    }
}
