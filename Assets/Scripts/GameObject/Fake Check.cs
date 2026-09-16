using UnityEngine;

public class FakeCheck : MonoBehaviour
{
    public GameObject targetCheck;
    public GameObject faker;

    void Update()
    {
        if (targetCheck == null && faker != null)   {
            faker.SetActive(true);
        }
    }
}
