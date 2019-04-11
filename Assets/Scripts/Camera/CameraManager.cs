using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera parameter")]
    [SerializeField] private Vector2 sizeBox;
    [SerializeField] private float smoothSpeed = 0.125f;

    private enum stateCamera
    {
        Find,
        MoveTo,
    }

    private bool screenShake = false;

    private stateCamera state = stateCamera.Find;

    //Vector2 newPositionCamera = Vector2.zero;
    private Transform target;
    private List<Transform> attackingFoes;
    private Vector3 bottomLeftPosition;
    //private Vector2 mapSize;

    private MapDrawer mapDrawer;
    private GameManager gameManager;
    private MapParameters parameters;

    // Start is called before the first frame update
    void Start()
    {
        attackingFoes = new List<Transform>();
        mapDrawer = FindObjectOfType<MapDrawer>();
        gameManager = FindObjectOfType<GameManager>();

        parameters = gameManager.parameters;
    }

    public float timeScreenShake= 0.4f;
    // Desired duration of the shake effect
    private float shakeDuration = 0f;

    // A measure of magnitude for the shake. Tweak based on your preference
    private float shakeMagnitude = 0.03f;

    // A measure of how quickly the shake effect should evaporate
    private float dampingSpeed = 1.0f;

    // The initial position of the GameObject
    Vector3 initialPosition;


    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            if (attackingFoes.Count == 0)
            {
                Vector2 position = transform.position;

                Vector2 positionTarget = Vector2.zero;
                //Limit camera X
                if ((position.x - sizeBox.x >= bottomLeftPosition.x || (target.position.x - position.x) > 0) &&
                    (position.x + sizeBox.x <= bottomLeftPosition.x + (parameters.mapSizeX * parameters.cellSize.x) ||
                     (target.position.x - position.x) < 0))
                {
                    positionTarget = new Vector2(target.position.x, 0);
                    position = new Vector2(positionTarget.x, position.y);
                    transform.position = (Vector3) position - Vector3.forward * 10;

                }

                //Limit camera Y
                if ((position.y - sizeBox.y >= bottomLeftPosition.y || (target.position.y - position.y) > 0) &&
                    (position.y + sizeBox.y <= bottomLeftPosition.y + (parameters.mapSizeY * parameters.cellSize.y) ||
                     (target.position.y - position.y) < 0))
                {
                    positionTarget = new Vector2(0, target.position.y);
                    transform.position = new Vector3(position.x, positionTarget.y, -10);
                }

            }
            else
            {
                Vector2 newPositionCamera = Vector2.zero;
                switch (state)
                {
                    case stateCamera.Find:
                        int i = 0;

                        foreach (var monsterIsAttacking in attackingFoes)
                        {
                            newPositionCamera += Vector2.Lerp(target.position, monsterIsAttacking.position, 0.5f);
                            i++;
                        }

                        newPositionCamera /= i;
                        state = stateCamera.MoveTo;
                        break;

                    case stateCamera.MoveTo:
                        Vector3 position = transform.position;
                        transform.position = Vector3.Lerp(position,
                            (Vector3) newPositionCamera - (Vector3.forward * 10), smoothSpeed);

                        if ((newPositionCamera - (Vector2) position).magnitude <= smoothSpeed)
                        {
                            state = stateCamera.Find;
                        }

                        break;
                }
            }

            if (screenShake)
            {
                if (shakeDuration > 0)
                {
                    transform.localPosition += Random.insideUnitSphere * shakeMagnitude;

                    shakeDuration -= Time.deltaTime * dampingSpeed;
                }
                else
                {
                    screenShake = false;
                    shakeDuration = 0f;

                }
            }
        }
    }

    public void ScreenShake()
    {
        shakeDuration = timeScreenShake;
        screenShake = true;
    }

    public void AddMonsterIsAttacking(Transform monster)
    {
        attackingFoes.Add(monster);
    }

    public void RemoveMonsterIsAttacking(Transform monster)
    {
        for (int i= 0; i < attackingFoes.Count; i++)
        {
            if (attackingFoes[i] == monster)
            {
                attackingFoes.RemoveAt(i);
            }
        }
    }

    public void AddPlayer(Transform player)
    {
        target = player;
    }

    private void OnDrawGizmos()
    {
        /*if (newPositionCamera != Vector2.zero)
        {
            Gizmos.DrawSphere(newPositionCamera, 0.2f);
        }*/

        if (false && target != null)
        {
            if (transform != null)
            {
                Vector3 position = transform.position;
                //Show Box area piu
                Gizmos.color = Color.red;

                Gizmos.DrawLine(new Vector3(position.x - sizeBox.x, position.y + sizeBox.y, position.z),
                    new Vector3(position.x - sizeBox.x, position.y - sizeBox.y, position.z));
                Gizmos.DrawLine(new Vector3(position.x + sizeBox.x, position.y - sizeBox.y, position.z),
                    new Vector3(position.x + sizeBox.x, position.y + sizeBox.y, position.z));
                if ((position.x - sizeBox.x >= bottomLeftPosition.x || (target.position.x - position.x) > 0) &&
                    (position.x + sizeBox.x <= bottomLeftPosition.x + parameters.mapSizeX ||
                     (target.position.x - position.x) < 0))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }


                Gizmos.DrawLine(new Vector3(position.x, position.y - sizeBox.y / 2, position.z),
                    new Vector3(position.x, position.y + sizeBox.y / 2, position.z));

                if ((position.y - sizeBox.y >= bottomLeftPosition.y || (target.position.y - position.y) > 0) &&
                    (position.y + sizeBox.y <= bottomLeftPosition.y + parameters.mapSizeY ||
                     (target.position.y - position.y) < 0))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawLine(new Vector3(position.x - sizeBox.x/2, position.y, position.z),
                    new Vector3(position.x + sizeBox.x / 2, position.y, position.z));
            }
        }

        //Size Box Aera Camera
       // Gizmos.color = Color.blue;
       // Gizmos.DrawLine(bottomLeftPosition, new Vector3(bottomLeftPosition.x + parameters.mapSizeX, bottomLeftPosition.y, bottomLeftPosition.z));
       // Gizmos.DrawLine(bottomLeftPosition, new Vector3(bottomLeftPosition.x, bottomLeftPosition.y + parameters.mapSizeY, bottomLeftPosition.z));
       // Gizmos.DrawLine(new Vector3(bottomLeftPosition.x, bottomLeftPosition.y + parameters.mapSizeY, bottomLeftPosition.z), new Vector3(bottomLeftPosition.x + parameters.mapSizeX, bottomLeftPosition.y + parameters.mapSizeY, bottomLeftPosition.z));
       // Gizmos.DrawLine(new Vector3(bottomLeftPosition.x + parameters.mapSizeX, bottomLeftPosition.y + parameters.mapSizeY, bottomLeftPosition.z), new Vector3(bottomLeftPosition.x + parameters.mapSizeX, bottomLeftPosition.y, bottomLeftPosition.z));
    }


}

