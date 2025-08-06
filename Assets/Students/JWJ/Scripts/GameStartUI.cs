using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Sprite five;
    [SerializeField] private Sprite four;
    [SerializeField] private Sprite three;
    [SerializeField] private Sprite two;
    [SerializeField] private Sprite one;

    [SerializeField] private GameObject waiting;
    [SerializeField] private GameObject gameStart;

    [SerializeField] private Image CountDownimage;

    private void Start()
    {
        CountDownimage.enabled = false;
        gameStart.SetActive(false);
        waiting.SetActive(true);
    }
    public void openGameStartUI()
    {
        waiting.SetActive(false);
        StartCoroutine(StartGameWithDelay());
    }

    private IEnumerator StartGameWithDelay()
    {
        Debug.Log("모든 캐릭터 생성 완료. 게임시작 5초전");
        CountDownimage.enabled = true;
        CountDownimage.sprite = five;
        yield return new WaitForSeconds(1);

        CountDownimage.sprite = four;
        yield return new WaitForSeconds(1);

        CountDownimage.sprite = three;
        yield return new WaitForSeconds(1);

        CountDownimage.sprite = two;
        yield return new WaitForSeconds(1);

        CountDownimage.sprite = one;
        yield return new WaitForSeconds(1);

        CountDownimage.enabled = false;
        gameStart.SetActive(true);
        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            Manager.Game.GameStart();
        }
        yield return new WaitForSeconds(2);

        gameStart.SetActive(false);


        
    }
}
