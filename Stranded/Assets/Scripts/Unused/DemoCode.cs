using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float speed;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(-speed, 0, 0);
        }

        Bounds b = transform.GetChild(0).GetComponent<BoxCollider2D>().bounds;
        if (Physics2D.OverlapBox(b.center, b.extents*2, 0, LayerMask.GetMask("Layer1")))
        {
            Debug.Log("There is an object on Layer1 next to us!");
            StartCoroutine(ChangeScene("Multiplayer"));
        }
    }

    IEnumerator ChangeScene(string scene)
    {
        GameObject.Find("Fader").GetComponent<Animator>().Play("FadeToBlack");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(scene);
    }
}
