using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターゲットブロック（ボールが当たったら壊れるブロック）を制御する
/// ターゲットブロックの GameObject に追加して使う
/// </summary>
public class TargetBlockController : MonoBehaviour
{

    [SerializeField] int m_life = 2;

    /// <summary>ブロックを壊した時に入る点数</summary>
    public int Score = 100;

    [SerializeField] GameObject m_prefab;

    void Start()
    {

    }

    void Update()
    {

    }

    /// <summary>
    /// Collider に衝突判定があった時に呼ばれる
    /// </summary>
    /// <param name="collision">衝突の情報</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // このログはもう不要なので無効にする
        // Debug.Log("Enter OnCollisionEnter2D."); // 関数が呼び出されたら Console にログを出力する

        // 衝突相手がボールだったら自分自身を破棄する
        if (collision.gameObject.tag == "BallTag")
        {
            // 衝突相手がボールだったら
            if (collision.gameObject.tag == "BallTag")
            {
                // ライフを減らす
                m_life -= 1;
                // ライフが 0 になったらブロックを消す
                if (m_life < 1)
                {
                    Destroy(this.gameObject);

                    if (m_prefab)
                    {
                        Instantiate(m_prefab, transform.position, Quaternion.identity); // prefab を産む
                    }

                }


            }

        }
    }

    /// <summary>
    /// 「トリガーモードの」Collider に衝突判定があった時に呼ばれる
    /// </summary>
    /// <param name="collision">衝突の情報</param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Enter OnTriggerEnter2D."); // 関数が呼び出されたら Console にログを出力する

        // 衝突相手がボールだったら自分自身を破棄する
        if (collision.gameObject.tag == "BallTag")
        {
            Destroy(this.gameObject);
        }
    }
}