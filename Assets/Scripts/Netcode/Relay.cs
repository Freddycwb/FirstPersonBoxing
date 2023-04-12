using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.VisualScripting;
using System;
using Unity.Networking.Transport;
using System.Runtime.CompilerServices;

public class Relay : MonoBehaviour
{
    public Action created;
    public Action joined;
    public Action failed;


    public string code;

    public GameEvent leave;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        try
        {
            Debug.Log("allocation");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            Debug.Log("joincode");
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            code = joinCode;
            Debug.Log("sethost");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
            created.Invoke();
        }
        catch (RelayServiceException e)
        {
            Debug.Log("Fail");
            failed.Invoke();
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            joined.Invoke();
        } catch (RelayServiceException e)
        {
            Debug.Log("Fail");
            failed.Invoke();
        }
    }
}
