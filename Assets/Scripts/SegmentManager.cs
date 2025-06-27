using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Build;
using UnityEngine;

public class SegmentManager : MonoBehaviour
{
    public GameObject[] segmentPrefabs;
    public Transform player;
    private int maxSegments = 0;

    private List<GameObject> activeSegments = new List<GameObject>();
    private Vector3 nextSpawnPoint;
    private int previousSegmentindex;

    void Start()
    {
        //Find the player position
        if (player == null)
        {
            player = transform.Find("HeroKnight").GetComponent<Transform>();
        }

        //To avoid zero or null
        if (maxSegments == 0) {
            maxSegments = segmentPrefabs.Length-1;
        }

        //Starting SpawnPoint
        nextSpawnPoint = new Vector3(player.position.x-6,0,0);

        // Spaw START Segment
        SpawnStartingSegment();

        // Spawn initial segments
        SpawnSegmentsSetRandomly();


    }

    void Update()
    {
        if (PlayerReachedNextSegment())
        {
            //SpawnRandomSegment();
            SpawnSegmentsSetRandomly();
            RemoveOldSegment();
        }
    }

    void SpawnRandomSegment()
    {
        //Spaw Random Segment
        int randomIndex = Random.Range(1, segmentPrefabs.Length);
        
        // To avoid same index
        if (randomIndex == previousSegmentindex) {

            if (previousSegmentindex == segmentPrefabs.Length)
            {
                randomIndex = previousSegmentindex - 1;
            }
            else
            {
                randomIndex++;
            }
        }

        GameObject newSegment = Instantiate(segmentPrefabs[randomIndex], nextSpawnPoint, Quaternion.identity);

        // Get the start point
        Transform newSegmentStartPoint = newSegment.GetComponent<Segment>().startPoint;

        // Offset the position
        Vector3 offset = nextSpawnPoint - newSegmentStartPoint.position;
        newSegment.transform.position += offset;

        // Add the new segment to the list of active segments
        activeSegments.Add(newSegment);

        // Update next spawn point based on the end point of the new segment
        nextSpawnPoint = newSegment.GetComponent<Segment>().endPoint.position;

        // Update the previousSegmentIndex
        previousSegmentindex = randomIndex;
    }

    void SpawnStartingSegment()
    {
        //Spaw Start Segment - Element 0
        int randomIndex = Random.Range(0, 0);
        GameObject newSegment = Instantiate(segmentPrefabs[randomIndex], nextSpawnPoint, Quaternion.identity);

        // Get the start point
        Transform newSegmentStartPoint = newSegment.GetComponent<Segment>().startPoint;

        // Offset the position
        Vector3 offset = nextSpawnPoint - newSegmentStartPoint.position;
        newSegment.transform.position += offset;

        // Add the new segment to the list of active segments
        activeSegments.Add(newSegment);

        // Update next spawn point based on the end point of the new segment
        nextSpawnPoint = newSegment.GetComponent<Segment>().endPoint.position;
    }

    void SpawnSpecificSegment(int segmentIndex)
    {
        if (segmentIndex > segmentPrefabs.Length) {
            Debug.Log(segmentIndex + "is out of Bounds");
        }

        GameObject newSegment = Instantiate(segmentPrefabs[segmentIndex], nextSpawnPoint, Quaternion.identity);

        // Get the start point
        Transform newSegmentStartPoint = newSegment.GetComponent<Segment>().startPoint;

        // Offset the position
        Vector3 offset = nextSpawnPoint - newSegmentStartPoint.position;
        newSegment.transform.position += offset;

        // Add the new segment to the list of active segments
        activeSegments.Add(newSegment);

        // Update next spawn point based on the end point of the new segment
        nextSpawnPoint = newSegment.GetComponent<Segment>().endPoint.position;

        // Update the previousSegmentIndex
        previousSegmentindex = segmentIndex;
    }

    void SpawnSegmentsSetRandomly()
    {
        // Create an array of indices from 1 to segmentPrefabs.Length
        int[] segmentIndices = Enumerable.Range(1, maxSegments).ToArray();

        // Shuffle the array to randomize the segment order
        segmentIndices = segmentIndices.OrderBy(x => Random.value).ToArray();

        // Loop through the shuffled indices and spawn the corresponding segments
        for (int i = 0; i < maxSegments; i++)
        {
            int segmentIndex = segmentIndices[i]; // Get the shuffled segment index

            // Spawn the specific segment based on the random order
            SpawnSpecificSegment(segmentIndex);
        }
    }

    void RemoveOldSegment()
    {
        if (activeSegments.Count > maxSegments)
        {
            Destroy(activeSegments[0]);
            activeSegments.RemoveAt(0);
        }
    }

    bool PlayerReachedNextSegment()
    {
        // Define how close the player must be to the end point to spawn the next segment
        return player.position.x > nextSpawnPoint.x - 10f;
    }
}
    

