using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiMemory
{
    public float Age
    {
        get 
        { 
            return Time.time - lastSeen; 
        }
    }
    public GameObject gameObject;
    public Vector3 position;
    public Vector3 direction;
    public float distance;
    public float angle;
    public float lastSeen;
    public float score;
}

public class AiSensorMemory
{
    public List<AiMemory> memories = new List<AiMemory>();
    GameObject[] characters;

    public AiSensorMemory(int maxPlayer)
    {
        characters = new GameObject[maxPlayer];
    }

    public void UpdateSenses(AiSensor sensor)
    {
        int targets = sensor.Filter(characters); // Uses scanLayers directly
        for (int i = 0; i < targets; ++i)
        {
            GameObject target = characters[i];
            RefreshMemory(sensor.gameObject, target);
        }
    }

    public void RefreshMemory(GameObject agent, GameObject target)
    {
        AiMemory memory = FetchMemory(target);
        memory.gameObject = target;
        memory.position = target.transform.position;
        memory.direction = target.transform.position - agent.transform.position;
        memory.distance = memory.direction.magnitude;
        memory.angle = Vector3.Angle(agent.transform.forward, memory.direction);
        memory.lastSeen = Time.time;
    }

    public AiMemory FetchMemory(GameObject gameObject)
    {
        AiMemory memory = memories.Find(x => x.gameObject == gameObject);
        if(memory == null)
        {
            memory = new AiMemory();
            memories.Add(memory);
        }
        return memory;
    }

    public void ForgetMemories(float olderThan)
    {
        memories.RemoveAll(m => m.Age > olderThan);
        memories.RemoveAll(m => !m.gameObject);
        memories.RemoveAll(m => m.gameObject.GetComponent<CharacterDeadState>().IsDead());
        memories.RemoveAll(m => m.gameObject.GetComponent<CharacterDeadState>().IsOccupied());
    }
}
