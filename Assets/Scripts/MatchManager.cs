using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MatchManager : NetworkBehaviour
{
    [SerializeField] private GameEvent startMatch;
    [SerializeField] private GameEvent endMatch;
    [SerializeField] private GameObject lobbyCamera;
    [SerializeField] private Vector3[] highlightPosition;
    [SerializeField] private Vector3[] highlightRotation;

    private int numberOfPlayers;
    private int numberOfPlayersReady;

    [SerializeField] private Relay relay;

    private List<GameObject> players = new List<GameObject>();


    public void GetPlayers()
    {
        numberOfPlayers++;
    }
    
    public void StartMatch()
    {
        numberOfPlayersReady++;
        if (numberOfPlayers == numberOfPlayersReady)
        {
            lobbyCamera.SetActive(false);
            startMatch.Raise();
        }
    }

    public void EndMatch()
    {
        numberOfPlayersReady = 0;
        StartCoroutine("SlowTime");
    }

    private IEnumerator SlowTime()
    {
        lobbyCamera.SetActive(true);
        lobbyCamera.transform.position = highlightPosition[0];
        lobbyCamera.transform.rotation = Quaternion.Euler(highlightRotation[0]);
        Time.timeScale = 0.1f;
        for (float i = 0; i < 0.12; i += Time.deltaTime)
        {
            yield return new WaitForEndOfFrame(); 
        }
        lobbyCamera.transform.position = highlightPosition[1];
        lobbyCamera.transform.rotation = Quaternion.Euler(highlightRotation[1]);
        for (float i = 0; i < 0.12; i += Time.deltaTime)
        {
            yield return new WaitForEndOfFrame();
        }
        lobbyCamera.transform.position = highlightPosition[2];
        lobbyCamera.transform.rotation = Quaternion.Euler(highlightRotation[2]);
        for (float i = 0; i < 0.12; i += Time.deltaTime)
        {
            yield return new WaitForEndOfFrame();
        }
        lobbyCamera.transform.position = highlightPosition[3];
        lobbyCamera.transform.rotation = Quaternion.Euler(highlightRotation[3]);
        Time.timeScale = 1;
        numberOfPlayersReady = 0;
        endMatch.Raise();
    }
}