using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Robber : MonoBehaviour
{
    NavMeshAgent agent;
    public GameObject target;
    public GameObject car;
    public GameObject[] hidingSpot;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Seek(target.transform.position);
        //Flee(target.transform.position);
       EscapeToCar();
    }

    void EscapeToCar()
    {
        float dis = Vector3.Distance(target.transform.position, transform.position);
        Vector3 dir = (transform.position - target.transform.position).normalized;
        bool runToCar = false; ;

        if (Vector3.Angle(target.transform.forward, dir) < 90.0f && dis < 30)//check if the target is in fov or not
        {
            if (Physics.Raycast(transform.position, dir, dis, LayerMask.GetMask("Tree")))//if blocked by tree
            {
                runToCar = true;//in fov but blocked by tree
            }
        }
        else//if not in fov 
            runToCar = true;

        if (dis < 10)//if too close to cop hide
            runToCar = false;

        if (Vector3.Distance(car.transform.position, transform.position) < 17)// very close to car, just run
            runToCar = true;

        if (runToCar)
            Seek(car.transform.position);
        else
            Hide();
    }

    void Hide()
    {
        float nearest = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;

        for(int i = 0; i < hidingSpot.Length; i++)
        {
            Vector3 hidingDir = hidingSpot[i].transform.position - target.transform.position;
            Vector3 hidingPos = hidingSpot[i].transform.position + hidingDir.normalized * 2;
            float dist = Vector3.Distance(transform.position, hidingPos);
            if (dist < nearest)
            {
                chosenSpot = hidingPos;
                nearest = dist;
            }
        }
        Seek(chosenSpot);
    }

    void Seek(Vector3 location)
    {
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 targetDir = location - agent.transform.position;
        agent.SetDestination(agent.transform.position - targetDir);
    }

    void Pursue()
    {
        Vector3 targetDir = target.transform.position - agent.transform.position;
        float predictionTime = targetDir.magnitude / (agent.speed + target.GetComponent<Drive>().speed);
        Vector3 intercept = target.transform.position + predictionTime * target.transform.forward * 5;
        Seek(intercept);
    }

    void Arrival(Vector3 location)
    {
        float slowdownDistance = 5.0f;
        float slowdownRate = 0.9f;

        Vector3 targetDir = location - agent.transform.position;
        if(targetDir.magnitude < slowdownDistance)
        {
            location = agent.transform.position + slowdownRate * targetDir;
        }
        Seek(location);

    }

    Vector3 wanderTarget = Vector3.zero;

    void Wander()
    {
        float wanderRadius = 10;
        float wanderDistance = 20;
        float wanderJitter = 1;
        
        wanderTarget += new Vector3(Random.Range(-1.0f,1.0f) * wanderJitter,
            0,Random.Range(-1.0f,1.0f) * wanderJitter);

        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        Vector3 targetLocal = wanderTarget + new Vector3(0, 0, wanderDistance);
        Vector3 targetWorld = gameObject.transform.InverseTransformVector(targetLocal);
        Seek(targetWorld);
    }
}
