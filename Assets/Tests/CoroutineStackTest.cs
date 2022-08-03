using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineStackTest : MonoBehaviour
{

    public Vector3 posCageMax;
    public Vector3 posCageMin;

    [SerializeField]
    private float HIT_POS_OFFSET = .03f; // to help lerp stops in the middle of the role
    [SerializeField]
    private float moveVelocity = .2f; // to help lerp stops in the middle of the role

    public bool calculateRoutines;

    [SerializeField]
    private List<GameObject> hits = new List<GameObject>();

    [SerializeField]
    private GameObject targetGO;
    [Space]
    [SerializeField]
    private GameObject moveble;
    [SerializeField]
    private int currentHit;

    IEnumerator runningCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CoroutineCoordinator());

    }

    public void AddNewPoint()
    {
        //random point
        var randomPoint = new Vector3(0, Random.Range(posCageMax.y, posCageMin.y), Random.Range(posCageMax.z, posCageMin.z));
        var newHit = Instantiate(targetGO, randomPoint, Quaternion.identity);

        hits.Add(newHit);


        //Call a new coroutine reference
        if (runningCoroutine == null)
        {
            runningCoroutine = LerpFunction(newHit);
            StartCoroutine(runningCoroutine);
        }
        else
            coroutineQueue.Enqueue(LerpFunction(newHit));
    }

    public void ClearList()
    {
        foreach (GameObject item in hits)
        {
            Destroy(item);
        }

        hits.Clear();

        // Stops the Routine Queue
        coroutineQueue.Clear();
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
        }
    }



    #region Coroutine
    [SerializeField]
    private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();

    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0)
                yield return StartCoroutine(coroutineQueue.Dequeue());
            yield return null;
        }
    }

    IEnumerator LerpFunction(GameObject hitPoint)
    {
        float timeElapsed = 0;
        Vector3 lerpValue;
        Vector3 startValue = moveble.transform.position;
        Vector3 endValue = (hitPoint.transform.position);

        while (timeElapsed < 3f)
        {
            lerpValue = Vector3.Lerp(startValue, endValue, timeElapsed / 3f);

            moveble.transform.position = lerpValue;

            timeElapsed += Time.deltaTime;

            //if (hitPoint != null)
            //Debug.Log("Moving ... to : " + hitPoint?.gameObject?.name);

            yield return null;
        }

        moveble.transform.position = endValue;
    }
    #endregion
}
