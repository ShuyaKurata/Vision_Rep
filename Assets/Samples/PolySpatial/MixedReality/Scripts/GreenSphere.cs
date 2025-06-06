using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenSphere : MonoBehaviour
{
     [SerializeField]
    string m_DestructionObjectTag = "Enemy"; // 衝突時に破壊されるオブジェクトのタグ
    public bool Fired = false;
    string m_DropItemTag = "DropItem";

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         
    }

   // --- 衝突処理 ---
void OnCollisionEnter(Collision collision)
{
    if(Fired){
        // 衝突した相手のゲームオブジェクトが指定されたタグを持っているか確認
        if (!string.IsNullOrEmpty(m_DestructionObjectTag) && collision.gameObject.CompareTag(m_DestructionObjectTag))
        {
            // EnemyMovement.cs スクリプトを取得
            EnemyMovement target = collision.gameObject.GetComponent<EnemyMovement>();

            if (target != null)
            {
                target.TakeDamage(1); // BのHPを1減らす
            }

        }
        if (!string.IsNullOrEmpty(m_DropItemTag) && collision.gameObject.CompareTag(m_DropItemTag))
        {
            SquishyController target = collision.gameObject.GetComponent<SquishyController>();

            if (target != null)
            {
                target.DestroyItem(); 

            }
        }
            Destroy(this.gameObject); // 自分（A）を破壊
    }
}

}
