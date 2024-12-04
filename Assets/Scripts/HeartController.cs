using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartController : MonoBehaviour
{
    PlayerController player;

    GameObject[] heartContainers;
    Image[] heartFills;
    public Transform heartsParent;
    public GameObject heartsContainerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        player = PlayerController.Instance;
        heartContainers = new GameObject[PlayerController.Instance.MaxHealth];
        heartFills = new Image[PlayerController.Instance.MaxHealth];


        PlayerController.Instance.onHealthChangedCallback += UpdateHeartHUD;
        InstantiateHeartContainer();
        UpdateHeartHUD();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetHeartContainers()
    {
        for(int i = 0; i < heartContainers.Length; i++)
        {
            if(i < PlayerController.Instance.MaxHealth)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(false);
            }
        }
    }

    void SetHeartFills()
    {
        for(int i = 0; i < heartFills.Length; i++)
        {
            if(i < PlayerController.Instance.Health)
            {
                heartFills[i].fillAmount = 1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }
        }
    }

    void InstantiateHeartContainer()
    {
        for(int i = 0 ; i < PlayerController.Instance.MaxHealth; i++)
        {
            GameObject temp = Instantiate(heartsContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
        

    }

    void UpdateHeartHUD()
    {
        SetHeartContainers();
        SetHeartFills();
    }
}
