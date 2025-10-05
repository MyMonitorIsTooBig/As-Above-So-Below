using UnityEngine;
using UnityEngine.UIElements;

public class ParallaxController : MonoBehaviour
{
    private float length;
    private float lengthy;
    Vector3 startpos;
    public GameObject cam;
    public float parallaxEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startpos = transform.position;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        lengthy = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    // Update is called once per frame
    void Update()
    {
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        float tempy = (cam.transform.position.y * (1 - parallaxEffect));
        float disty = (cam.transform.position.y * parallaxEffect);

        transform.position = new Vector3(startpos.x + dist, startpos.y + disty, transform.position.z);

        if (temp > startpos.x + length) startpos.x += length*2;
        else if (temp < startpos.x - length) startpos.x -= length*2;


        if (tempy > startpos.y + lengthy) startpos.y += lengthy * 2;
        else if (tempy < startpos.y - lengthy) startpos.y -= lengthy * 2;
    }
}
