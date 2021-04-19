using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityInverter : MonoBehaviour
{
    [Range(0.01f, 1.0f)][SerializeField] float flipGravityModifier = 0.5f;
    GameObject player;
    bool inverted = false;
    Vector2 min, max;
    float gravityMagnitude;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
        
        min = new Vector2(transform.position.x - transform.localScale.x, transform.position.y - transform.localScale.y);
        max = new Vector2(transform.position.x + transform.localScale.x, transform.position.y + transform.localScale.y);

        gravityMagnitude = Mathf.Abs(Physics.gravity.y);
    }

    void LateUpdate() {
        if(player.GetComponent<Rocket>().IsDead()) return;

        if(IsPlayerInZone() && !inverted) {
            Debug.Log("Inverting");
            SetGravity(Vector3.up * gravityMagnitude * flipGravityModifier);
            inverted = true;
        } else if(!IsPlayerInZone() && inverted) {
            SetGravity(Vector3.down * gravityMagnitude);
            inverted = false;
        }
    }

    void SetGravity(Vector3 _direction) {
        Physics.gravity = _direction;
    }

    bool IsPlayerInZone() {
        RaycastHit[] _hits = Physics.RaycastAll(player.transform.position, Vector3.forward, 10f);
        foreach(RaycastHit _hit in _hits) {
            if(_hit.transform == transform) return true;
        }
        return false;
    }
}