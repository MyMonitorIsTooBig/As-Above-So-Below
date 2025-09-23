using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] Transform _player;

    Vector3 _vel = Vector3.zero;

    [SerializeField] float smoothing = 0.3f;

    [SerializeField] bool _followPlayer = false;

    public bool FollowPlayer {  get { return _followPlayer; } set { _followPlayer = value; } }

    Vector2 _followPoint = Vector2.zero;
    public Vector2 FollowPoint { get { return _followPoint; } set {_followPoint = value; } }

    [SerializeField] Vector2 xClamps = Vector2.zero;
    [SerializeField] Vector2 yClamps = Vector2.zero;


    [SerializeField] Vector3 topCorner;
    [SerializeField] Vector3 botCorner;

    Vector3 relativeTopCorner;
    Vector3 relativeBotCorner;

    public Vector3 CamTopCorner { get { return relativeTopCorner; } }
    public Vector3 CamBotCorner { get { return relativeBotCorner; } }



    public static CameraManager Instance;

    private void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        // positional data of the cameras corners
        relativeTopCorner = topCorner + transform.position;
        relativeBotCorner = botCorner + transform.position;

        // if the camera is able to follow player (note: following player takes priority over following the set point)

        if (_followPlayer)
        {
            
            Vector3 smoothPos = Vector3.SmoothDamp(transform.position, _player.position, ref _vel, smoothing);

            transform.position = new Vector3(Mathf.Clamp(smoothPos.x, xClamps.x, xClamps.y), Mathf.Clamp(smoothPos.y, yClamps.x, yClamps.y), -10);

            

            return;
            
        }

        // if the camera should follow a different point

        if (_followPoint != Vector2.zero)
        {
            Vector3 smoothPos = Vector3.SmoothDamp(transform.position, _followPoint, ref _vel, smoothing);

            transform.position = new Vector3(smoothPos.x, smoothPos.y, -10);
        }

    }
}
