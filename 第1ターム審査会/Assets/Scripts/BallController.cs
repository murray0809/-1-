using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   // UI を使うために必要になる

/// <summary>
/// ボールを制御するクラス
/// ボールのオブジェクトに追加して使う
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class BallController : MonoBehaviour
{
    /// <summary>ボールが最初に動く方向</summary>
    [SerializeField] Vector2 m_powerDirection = Vector2.up + Vector2.right;
    /// <summary>ボールを最初に動かす力の大きさ</summary>
    [SerializeField] float m_powerScale = 5f;
    Rigidbody2D m_rb2d;
    /// <summary>得点を表示する Text</summary>
    [SerializeField] Text m_scoreText;
    /// <summary>得点</summary>
    int m_score = 0;
    /// <summary>メッセージを表示する Text</summary>
    [SerializeField] Text m_messageText;
    /// <summary>ゲームスタートボタン</summary>
    [SerializeField] Button m_startButton;
    /// <summary>ボールが衝突した時の SFX</summary>
    [SerializeField] AudioClip m_sfx;
    /// <summary>ゲームオーバー時の SFX</summary>
    [SerializeField] AudioClip m_gameoverSfx;
    /// <summary>ゲームクリアー時の SFX</summary>
    [SerializeField] AudioClip m_gameClearSfx;
    AudioSource m_audioSource;
    /// <summary>HitFactor 係数</summary>
    [SerializeField] float m_hitFactorCoefficient = 2f;
    /// <summary>ボールの最低速度</summary>
    [SerializeField] float m_minSpeed = 4f;
    /// <summary>ボールの最高速度</summary>
    [SerializeField] float m_maxSpeed = 5f;
    /// <summary>ゲームリスタートボタン</summary>
    [SerializeField] Button m_restartButton;
    /// <summary>ボールの初期位置</summary>
    Vector2 m_initialPosition;
    /// <summary>これ以上ボールの進行方向が水平になったら補正を入れるというリミット</summary>
    [SerializeField] float m_horizontalLimit = 0.05f;
    /// <summary>ボールに補正を補正を入れ時のベクトル</summary>
    [SerializeField] Vector2 m_adjustVector = Vector3.down * 3f;

    void Start()
    {
        // のちに操作するコンポーネントを保持しておく
        m_rb2d = GetComponent<Rigidbody2D>();
        m_audioSource = GetComponent<AudioSource>();

        m_initialPosition = transform.position; // ボールの初期位置を記憶しておく
        m_score = 0;    // 得点を初期化する

        // m_startButton が設定されていない時はボールを押す
        if (m_startButton == null)
        {
            Push();
        }
    }

    void Update()
    {

    }

    /// <summary>
    /// ボールに力を加える
    /// </summary>
    public void Push()
    {
        m_rb2d.AddForce(m_powerDirection.normalized * m_powerScale, ForceMode2D.Impulse);

        // m_startButton がある場合は、非表示にする
        if (m_startButton)
        {
            m_startButton.gameObject.SetActive(false);
        }

        AdjustSpeed();  // 速度を調整する
    }

    /// <summary>
    /// 衝突判定をする（コライダーモード）
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突相手から TargetBlockController コンポーネントを取得する
        TargetBlockController target = collision.gameObject.GetComponent<TargetBlockController>();
        if (target) // もしコンポーネントが取れたら（衝突相手が TargetBlock だったら
        {
            AddScore(target.Score);   // スコアを加算する
        }
        else if (collision.gameObject.tag == "Player")  // 衝突相手がパドルだったら
        {
            // パドルのどの位置に当たったかに応じて、ボールに力を加える
            float hitFactor = HitFactor(transform.position, collision.transform.position, collision.collider.bounds.size.x);
            Vector2 forceDirection = Vector2.right * hitFactor * m_hitFactorCoefficient;
            m_rb2d.AddForce(forceDirection, ForceMode2D.Impulse);
        }

        // 音を鳴らす
        if (m_audioSource)
        {
            m_audioSource.PlayOneShot(m_sfx);
        }

        // Killzone にぶつかったらゲームオーバー
        if (collision.gameObject.tag == "KillzoneTag")
        {
            GameOver();
        }

    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 水平にぶつかっていたら、下向きに力を加える（続行不可能状態の防止）
        Vector2 v = m_rb2d.velocity.normalized;
        if (Mathf.Abs(v.y) < m_horizontalLimit)
        {
            m_rb2d.AddForce(m_adjustVector, ForceMode2D.Impulse);
        }

        AdjustSpeed();  // ボールの速度を調整する
    }

    /// <summary>
    /// 衝突判定をする（トリガーモード）
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Killzone にぶつかったらゲームオーバー
        if (collision.gameObject.tag == "KillzoneTag")
        {
            GameOver();
        }

    }

    /// <summary>
    /// 点数を加える
    /// </summary>
    /// <param name="score">加算する点数</param>
    public void AddScore(int score)
    {
        m_score += score;   // 点数を足す
        Debug.Log("Score: " + m_score.ToString());  // Console に出力する

        if (m_scoreText)    // m_scoreText が設定されていたら
        {
            m_scoreText.text = "Crystals: " + m_score.ToString() +"/3";  // 表示を更新する
        }

        // 残りのブロック数を集計する（ブロックが壊れる前に集計するので、一つ多いことに注意すること）
        TargetBlockController[] blocks = GameObject.FindObjectsOfType<TargetBlockController>();
        Debug.Log("残り " + blocks.Length + " 個");
        if (blocks.Length < 2)
        {
            Debug.Log("Clear!");
            GameClear();
        }
    }

    /// <summary>
    /// ゲームオーバーにする
    /// </summary>
    void GameOver()
    {
        // ボールを止める
        m_rb2d.velocity = Vector2.zero;

        // メッセージを表示する
        if (m_messageText)
        {
            m_messageText.text = "Game Over";
        }

        // 音を鳴らす
        if (m_audioSource)
        {
            m_audioSource.PlayOneShot(m_gameoverSfx);
        }

        // Restart ボタンを表示する
        if (m_restartButton)
        {
            m_restartButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// ゲームをクリアした時に呼ばれる
    /// </summary>
    void GameClear()
    {
        // ボールを止める
        m_rb2d.velocity = Vector2.zero;

        // メッセージを表示する
        if (m_messageText)
        {
            m_messageText.text = "Congratulations!";
        }

        // 音を鳴らす
        if (m_audioSource)
        {
            m_audioSource.PlayOneShot(m_gameClearSfx);
        }

        // Restart ボタンを表示する
        if (m_restartButton)
        {
            m_restartButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// パドルのどの位置に当たったのかを計算する。
    /// 右端に当たったら 1、中央に当たったら 0、左端に当たったら 1 を返す。
    /// </summary>
    /// <param name="ballPosition"></param>
    /// <param name="paddlePosition"></param>
    /// <param name="paddleWidth"></param>
    /// <returns></returns>
    float HitFactor(Vector2 ballPosition, Vector2 paddlePosition, float paddleWidth)
    {
        float hitFactor = (ballPosition.x - paddlePosition.x) / paddleWidth;
        return hitFactor;
    }

    /// <summary>
    /// ボールの速度を調整する
    /// </summary>
    void AdjustSpeed()
    {
        Vector2 v = m_rb2d.velocity;

        if (v.magnitude < m_minSpeed)
        {
            m_rb2d.velocity = m_rb2d.velocity.normalized * m_minSpeed;
        }
        else if (v.magnitude > m_maxSpeed)
        {
            m_rb2d.velocity = m_rb2d.velocity.normalized * m_maxSpeed;
        }
    }

    /// <summary>
    /// ゲームオーバー後にゲームをリセットする
    /// </summary>
    public void ResetGame()
    {
        m_score = 0;    // 得点をリセットする
        AddScore(0);    // 得点表示をリセットする
        transform.position = m_initialPosition; // ボールの位置をリセットする

        // TargetBlock を全て消す
        TargetBlockController[] targetBlocks = GameObject.FindObjectsOfType<TargetBlockController>();
        foreach (TargetBlockController block in targetBlocks)
        {
            Destroy(block.gameObject);
        }

        // TargetBlock を再生成する
        GameObject generatorObject = GameObject.Find("TargetBlockGenerator/TargetBlockGenerator1/TargetBlockGenerator2/TargetBlockGenerator3");
        TargetBlockGenerator generator = generatorObject.GetComponent<TargetBlockGenerator>();
        generator.GenerateBlocks();

        m_messageText.gameObject.SetActive(false);  // メッセージを消す
        m_restartButton.gameObject.SetActive(false);    // Restart ボタンを消す
        m_startButton.gameObject.SetActive(true);   // Start ボタンを表示する
    }
}