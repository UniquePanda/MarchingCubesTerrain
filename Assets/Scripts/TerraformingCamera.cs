using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraformingCamera : MonoBehaviour
{
    public float brushSize = 2.0f;

    Vector3 hitPoint;
    Camera cam;

    void Awake() {
        cam = GetComponent<Camera>();
    }

    void LateUpdate() {
        if (Input.GetMouseButton(0)) {
            Terraform(true);
        } else if (Input.GetMouseButton(1)) {
            Terraform(false);
        }
    }

    void Terraform(bool add) {
        RaycastHit hit;

        if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 1000)) {
            Chunk hitChunk = hit.collider.gameObject.GetComponent<Chunk>();
            hitPoint = hit.point;

            hitChunk.EditWeights(hitPoint, brushSize, add);
        }
    }
}
