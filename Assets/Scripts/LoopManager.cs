using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoopManager : MonoBehaviour
{
    public string[] chunkSceneNames = { "sample_chunk1", "sample_chunk2" };
    public Transform player;
    public float segmentLength = 15f;
    public float cornerRotation = 90f;

    private int currentIndex = 0;
    private Scene currentScene;
    private Scene nextScene;
    private Transform currentAnchor;
    private Transform nextAnchor;
    private bool isTransitioning = false;

    IEnumerator Start()
    {
        yield return LoadChunk(0, Vector3.zero, Quaternion.identity);
        currentScene = SceneManager.GetSceneByName(chunkSceneNames[0]);
        currentAnchor = GetAnchor(currentScene);
    }

    private Transform GetAnchor(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Anchor")
                return root.transform;
        }
        return null;
    }

    public void OnExitTrigger()
    {
        if (!isTransitioning)
            StartCoroutine(TransitionToNextChunk());
    }

    IEnumerator TransitionToNextChunk()
    {
        isTransitioning = true;

        int nextIndex = (currentIndex + 1) % chunkSceneNames.Length;

        Vector3 nextPos = currentAnchor.position + currentAnchor.forward * segmentLength;
        Quaternion nextRot = currentAnchor.rotation * Quaternion.Euler(0, cornerRotation, 0);

        yield return LoadChunk(nextIndex, nextPos, nextRot);

        yield return SceneManager.UnloadSceneAsync(currentScene);

        currentIndex = nextIndex;
        currentScene = nextScene;
        currentAnchor = nextAnchor;
        isTransitioning = false;
    }

    IEnumerator LoadChunk(int index, Vector3 pos, Quaternion rot)
    {
        var op = SceneManager.LoadSceneAsync(chunkSceneNames[index], LoadSceneMode.Additive);
        yield return op;

        nextScene = SceneManager.GetSceneByName(chunkSceneNames[index]);
        nextAnchor = GetAnchor(nextScene);
        nextAnchor.SetPositionAndRotation(pos, rot);
    }
}