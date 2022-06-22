using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Range(0.5f, 2)]
    public float SpawnRate = 1;
    public GameObject[] Enemies;

    float _spawnRate;
    float limitH;
    float limitV;

    // Start is called before the first frame update
    void Start()
    {
        _spawnRate = SpawnRate;
    }

    // Update is called once per frame
    void Update()
    {
        _spawnRate -= Time.deltaTime;

        //Spawn enemies only in areas visible in the camera
        limitV = Camera.main.orthographicSize * 2;
        limitH = limitV * (float)Screen.width / Screen.height;
        Transform camTrans = Camera.main.transform;
        Vector3 camPos = camTrans.position;
        Bounds limit = new Bounds(camPos, new Vector3(limitH, limitV, 0));
        float x = Random.Range(-limitH, limitH);
        float y = Random.Range(-limitV, limitV);

        if (_spawnRate < 0 && GameManager.Instance.IsDead == false)
        {
            SpawnAI(x, y);
        }   
    }

    //Spawn AI
    void SpawnAI(float xPos, float yPos)
    {
        int randomAI = Random.Range(0, Enemies.Length);

        Vector3 pos = new Vector3(xPos, yPos, 0);
        Instantiate(Enemies[randomAI], pos, Quaternion.identity);

        _spawnRate = SpawnRate;
    }
}
