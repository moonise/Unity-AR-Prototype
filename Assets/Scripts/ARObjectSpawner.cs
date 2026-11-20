using System.Collections.Generic; //die verwendung von listen 
using UnityEngine; 
using UnityEngine.XR.ARFoundation; //stellt die ar funktionen bereit
using UnityEngine.XR.ARSubsystems; //enthält die typen für die ar erkennung

public class ARObjectSpawner : MonoBehaviour
{
    public GameObject objectPrefab;

    private ARRaycastManager raycastManager;
    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        //prüft ob der benutzer berührt (berühren: dokunmak)
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);//

        //reagiert nur auf den beginn einer berührung
        if (touch.phase != TouchPhase.Began)
            return;

        //prüft ob die berührung auf einern erkannten ar fläche liegt 
        if (raycastManager.Raycast(
            touch.position,
            hits,
            TrackableType.PlaneWithinPolygon))
        {
            //Erzeugt das Prefab an der erkannten Position
            // mit der Rotation der erkannten Fläche

            Instantiate(objectPrefab, hits[0].pose.position,
                hits[0].pose.rotation);
        }
    }
}
