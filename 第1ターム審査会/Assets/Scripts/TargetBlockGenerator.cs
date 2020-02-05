using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターゲットブロックを並べる
/// 適当な GameObject に追加して使う
/// 追加した GameObject の Position から左右に Columns の数だけ、下方向に Rows の数だけブロックを生成する
/// </summary>
public class TargetBlockGenerator : MonoBehaviour
{
    /// <summary>並べるプレハブを指定する。複数指定できる</summary>
    [SerializeField] GameObject[] m_targetPrefabArray;
    /// <summary>横にいくつ並べるか</summary>
    [SerializeField] int m_columns = 7;
    /// <summary>縦にいくつ並べるか</summary>
    [SerializeField] int m_rows = 6;

    void Start()
    {
        GenerateBlocks();
    }

    void Update()
    {

    }

    public void GenerateBlocks()
    {
        Vector2 pos = transform.position;   // pos は「ブロックを配置する位置」
        float blockHeight = 0f, blockWidth = 0f;

        // ブロックを並べる
        for (int j = 0; j < m_rows; j++)
        {
            // 横に並べる
            for (int i = 0; i < m_columns; i++)
            {
                int prefabIndex = j % m_targetPrefabArray.Length;
                GameObject go = Instantiate(m_targetPrefabArray[prefabIndex]);  // プレハブからオブジェクトを生成する
                go.transform.position = pos;    // オブジェクトの位置を設定する
                go.transform.SetParent(this.transform); // 生成したブロックを自分自身の子オブジェクトに設定する
                blockWidth = go.GetComponent<SpriteRenderer>().bounds.size.x;   // いま生成したオブジェクトの高さと幅を取得する
                blockHeight = go.GetComponent<SpriteRenderer>().bounds.size.y;
                int a = (i % 2 == 0) ? 1 : -1;
                pos = pos + Vector2.right * (i + 1) * a * blockWidth; // 列を左右にずらす
            }
            // 行を下にずらす
            pos = new Vector2(transform.position.x, pos.y - blockHeight);
        }
    }
}