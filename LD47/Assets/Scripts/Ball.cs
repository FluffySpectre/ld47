using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float kickForce = 3;
    public float dribbleForce = 1;
 
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Bot")
        {
            bool isKicking = other.transform.GetComponent<BotController>().IsDashing;

            Vector3 direction = (other.transform.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().AddForce(-direction * (isKicking ? kickForce : dribbleForce), ForceMode2D.Impulse);
        }
    }

    public void ResetBall()
    {
        transform.position = Vector3.zero;
        // enable physics again
        GetComponent<Collider2D>().enabled = true;
        GetComponent<Rigidbody2D>().simulated = true;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }
}
