using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject PlayerPrefab;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    void Start()
    {
        CreatePlayer();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("LeaveRoom()");
        }
    }

    void CreatePlayer()
    {
        Vector3 spawnPosition = Random.insideUnitSphere * 5f;
        spawnPosition.y = 1f;

        PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPosition, Quaternion.identity);
    }

    // �뿡�� ������ ȣ��Ǵ� �ݹ��Լ�
    public override void OnLeftRoom()
    {
        Debug.Log("LoadScene(0)");
        SceneManager.LoadScene(0);
    }

}
