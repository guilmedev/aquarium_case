using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitsHandler : MonoBehaviour
{
    private float hitPosOffset = .03f; // to help lerp stops in the middle of the role
    [SerializeField]
    private float moveVelocity = .2f; // to help lerp stops in the middle of the role

    [SerializeField]
    private List<GameObject> hits = new List<GameObject>();

    private GameObject moveble = null;

    IEnumerator runningCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init(GameObject moveble, float hitOffset)
    {
        this.moveble = moveble;
        this.hitPosOffset = hitOffset;
        StartCoroutine(CoroutineCoordinator());
    }

    public void AddNewPoint(GameObject newHit)
    {
        hits.Add(newHit);
    }

    public void LerpToPoint(GameObject newHit)
    {
        IniLerpRoutine(newHit);
    }

    public void RemoveWater(GameObject newHit)
    {
        IniLerpRoutine(newHit);
    }

    private void IniLerpRoutine(GameObject newHit)
    {
        //Call a new coroutine reference
        if (runningCoroutine == null)
        {
            runningCoroutine = LerpFunction(newHit);
            StartCoroutine(runningCoroutine);
        }
        else
            coroutineQueue.Enqueue(LerpFunction(newHit));
    }

    public void DisableAllHits()
    {
        ClearList();
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
    [HideInInspector]
    public float currentTimeToBeEmpty;
    private bool commandSent;

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
        float lerpValue = 0;
        float startValue = moveble.transform.position.y;
        float endValue = (hitPoint.transform.position.y) + hitPosOffset;

        commandSent = false;

        while (timeElapsed < currentTimeToBeEmpty)
        {
            lerpValue = Mathf.Lerp(startValue, endValue, timeElapsed / currentTimeToBeEmpty);

            moveble.transform.position = new Vector3(moveble.transform.position.x, lerpValue, moveble.transform.position.z);

            timeElapsed += Time.deltaTime;


            if (currentTimeToBeEmpty - timeElapsed < .8)
            {
                ReduceWatter(hitPoint);
            }

            yield return null;
        }

        moveble.transform.position = new Vector3(moveble.transform.position.x, endValue, moveble.transform.position.z);
    }


    private void ReduceWatter(GameObject hit)
    {
        if (commandSent) return;

        HitPoint hitPoint = hit.GetComponentInChildren<HitPoint>();
        if (hitPoint != null)
        {
            hitPoint.ReduceWatter();
        }

        commandSent = true;
    }

    #endregion
}
