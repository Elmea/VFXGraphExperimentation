using UnityEngine;

public class SphereInfluence : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        Simulation.Instance.Subscribe(this);
    }

    // Update is called once per frame
    private void OnDisable()
    {
        Simulation.Instance.Unsubscribe(this);
    }

    private void Update()
    {
        Simulation.Instance.UpdatePosition(this, transform.position);
    }
}
