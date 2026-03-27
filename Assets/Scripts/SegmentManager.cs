using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Build;
using UnityEngine;

public class SegmentManager : MonoBehaviour
{
    public bool enableDebugLogs = true;
    [Header("Note: Element 0 will not be spawned")]
    public GameObject[] segmentPrefabs;
    public Transform player;
    public int maxSegments = 10;
    private int noOfSegmentsSet;

    private List<GameObject> activeSegments = new List<GameObject>();
    private Vector3 nextSpawnPoint;
    private int previousSegmentindex;

    /// <summary>
    /// Attempts to find the player Transform by name if not assigned in the inspector.
    /// </summary>
    private void Awake()
    {
        if (player == null)
        {
            player = transform.Find("HeroKnight").GetComponent<Transform>();
        }
    }

    /// <summary>
    /// Initialises default values and spawns the starting segment followed by a randomised initial set.
    /// </summary>
    void Start()
    {
        //To avoid zero or null
        if (maxSegments == 0)
        {
            maxSegments = segmentPrefabs.Length - 1;
        }

        //To avoid zero or null
        if (noOfSegmentsSet == 0)
        {
            noOfSegmentsSet = segmentPrefabs.Length - 1;
        }

        //Starting SpawnPoint
        nextSpawnPoint = new Vector3(player.position.x - 6, 0, 0);

        // Spawn START Segment
        SpawnStartingSegment();

        // Spawn initial segments
        SpawnSegmentsSetRandomly();
    }

    /// <summary>
    /// Logs a message to the console when debug logging is enabled.
    /// </summary>
    /// <param name="message">The message to log.</param>
    void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log(message);
    }

    /// <summary>
    /// Checks each frame whether the player has reached the next segment trigger,
    /// spawning new segments and removing old ones as needed.
    /// </summary>
    void Update()
    {
        if (PlayerReachedNextSegment())
        {
            SpawnSegmentsSetRandomly();
            RemoveOldSegment();
        }
    }

    /// <summary>
    /// Spawns a single random segment at the current spawn point, avoiding repeating the previous segment.
    /// </summary>
    void SpawnRandomSegment()
    {
        Log("Spawning Random Segment");
        int randomIndex = Random.Range(1, segmentPrefabs.Length);

        // To avoid same index
        if (randomIndex == previousSegmentindex)
        {
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

    /// <summary>
    /// Spawns the fixed starting segment (index 0) at the initial spawn point.
    /// </summary>
    void SpawnStartingSegment()
    {
        Log("Spawning Starting Segment");
        //Spawn Start Segment - Element 0
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

    /// <summary>
    /// Spawns the segment at the given index from <see cref="segmentPrefabs"/> at the current spawn point.
    /// </summary>
    /// <param name="segmentIndex">The index of the prefab to spawn.</param>
    void SpawnSpecificSegment(int segmentIndex)
    {
        Log("Spawning Specific Segment");
        if (segmentIndex > segmentPrefabs.Length)
        {
            Log(segmentIndex + "is out of Bounds");
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

    /// <summary>
    /// Shuffles all non-starting segment indices and spawns each one in a random order.
    /// </summary>
    void SpawnSegmentsSetRandomly()
    {
        Log("Spawning Set of " + noOfSegmentsSet + " Segments Randomly");
        // Create an array of indices from 1 to segmentPrefabs.Length
        int[] segmentIndices = Enumerable.Range(1, noOfSegmentsSet).ToArray();

        // Shuffle the array to randomize the segment order
        segmentIndices = segmentIndices.OrderBy(x => Random.value).ToArray();

        // Loop through the shuffled indices and spawn the corresponding segments
        for (int i = 0; i < noOfSegmentsSet; i++)
        {
            int segmentIndex = segmentIndices[i]; // Get the shuffled segment index

            // Spawn the specific segment based on the random order
            SpawnSpecificSegment(segmentIndex);
        }
    }

    /// <summary>
    /// Destroys the oldest active segment when the number of active segments exceeds <see cref="maxSegments"/>.
    /// </summary>
    void RemoveOldSegment()
    {
        Log("Removing Old Segments");
        if (activeSegments.Count > maxSegments)
        {
            Destroy(activeSegments[0]);
            activeSegments.RemoveAt(0);
        }
    }

    /// <summary>
    /// Returns true when the player's X position is within 10 units of the next spawn point,
    /// indicating it is time to spawn the next batch of segments.
    /// </summary>
    /// <returns>True if the player is close enough to trigger the next segment spawn.</returns>
    bool PlayerReachedNextSegment()
    {
        Log("Player Reached Next Segment");
        return player.position.x > nextSpawnPoint.x - 10f;
    }
}
