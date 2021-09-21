using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//@Author:Nagani-syon
//
//モーフィング処理
//
//※ToFix
//・高さが違うもの(posy)をモーフィングしたときにガクッてなる→頂点の位置がローカル基準のためワールド基準に変更すると解決できそう
//・変身前と変身後が同じモデルだと多分バグる→同じモデル又は同じ頂点数の時に例外処理を書いてあげると解決できそう
//・前半ゆっくり変化するイージングを選ぶと違和感がでる→うまいことごまかすしかない


public class VertexChange : MonoBehaviour
{
    #region Setting
    [Header("変身前オブジェクト")]
    [SerializeField] GameObject beforeObj = null;
    [Header("変身後オブジェクト")]
    [SerializeField] GameObject afterObj = null;
    [Header("変身時間")]
    [SerializeField] float changeTime = 0.5f;
    [Header("変身後イージング時間")]
    [SerializeField] float easeTime = 2.0f;
    [Header("ディゾルブ時間")]
    [SerializeField] float dissolveTime = 0.2f;

    [Header("デフォルトスキン")]
    [SerializeField] Material defaultSkin = null;
    [Header("ディゾルブスキン")]
    [SerializeField] Material dissolveSkin = null;
    [Header("変身中スキン")]
    [SerializeField] Material cyberSkin = null;

    private enum DrawType
    {
        Point,Line,Triangle,LineStrip,Quad
    }
    [Header("変身中描画タイプ")]
    [SerializeField] DrawType drawType;

    //テスト用-------------------------------
    [Header("テスト用ランダム表示用リスト")]
    [SerializeField] List<GameObject> Actors;
    [Header("ランダム表示ON/OFF")]
    [SerializeField] bool isRandom=false;
    bool canMorf = true;
    //---------------------------------------
    #endregion

    #region Start
    void Start()
    {    
        //テスト用---
        if (isRandom)
        {
            beforeObj = null;
        }//----------
        else
        {
            beforeObj.SetActive(true);
            beforeObj.GetComponent<MeshRenderer>().material = defaultSkin;
            afterObj.SetActive(false);
        }
      
    }
    #endregion

    #region Update
    // Update is called once per frame
    void Update()
    {
        if (isRandom)//テスト用----
        {
            if (canMorf)
            {
                canMorf = false;

                if (beforeObj == null)
                {
                    beforeObj = Actors[(int)Random.Range(0, Actors.Count)];
                }
                else
                {
                    beforeObj = afterObj;
                }

                afterObj = Actors[(int)Random.Range(0, Actors.Count)];
                while (beforeObj == afterObj)
                {
                    afterObj = Actors[(int)Random.Range(0, Actors.Count)];
                }

                beforeObj.SetActive(true);
                beforeObj.GetComponent<MeshRenderer>().material = defaultSkin;
                afterObj.SetActive(false);
                StartCoroutine(Morfing());
            }
        }//-------------------------
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Morfing());
            }
        }
       
    }
    #endregion

    IEnumerator Morfing()
    {
        ////ディゾルブする
        var m_before = beforeObj.GetComponent<MeshRenderer>().material = dissolveSkin;
        //ディゾルブ
        ProcessTimer timer = new ProcessTimer(); timer.Restart();
        while (timer.TotalSeconds < dissolveTime)
        {
            var amount = Easing.Linear(timer.TotalSeconds, dissolveTime, 0, 1);
            m_before.SetFloat("_DisAmount", amount);
            yield return null;
        }
        m_before.SetFloat("_DisAmount", 1.0f);

        //描画タイプによって表現を変える
        switch (drawType)
        {
            case DrawType.Point:     ToPoint(beforeObj);     break;
            case DrawType.Line:      ToLine(beforeObj);      break;
            case DrawType.Triangle:  ToMesh(beforeObj);      break;
            case DrawType.LineStrip: ToLineStrip(beforeObj); break;
            case DrawType.Quad:      ToQuad(beforeObj);      break;
        }
        //変身中スキンに変更
        beforeObj.GetComponent<MeshRenderer>().material = cyberSkin;

        //拡散する
        Mesh beforeMesh=beforeObj.GetComponent<MeshFilter>().mesh;
        Vector3[] beforeVertices=beforeMesh.vertices;
        Vector3[] defaultBeforeVertices = beforeMesh.vertices;

        yield return new WaitForSeconds(0.4f);

        //拡散エフェクト処理
        timer.Restart();
        while (timer.TotalSeconds < changeTime)
        {
            for (var i = 0; i < beforeVertices.Length; i++)
            {
                beforeVertices[i] += new Vector3(Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f));//全頂点のxとyをランダムでちょっと動かす
            }
            beforeMesh.SetVertices(beforeVertices);
            yield return null;
        }
        beforeMesh.SetVertices(defaultBeforeVertices);

        //モデル入れ替え------------------------↑変身前オブジェクトへの処理
        beforeObj.SetActive(false);      
        afterObj.SetActive(true);
        //--------------------------------------↓変身後オブジェクトへの処理

        //描画タイプによって表現を変える
        switch (drawType)
        {
            case DrawType.Point:     ToPoint(afterObj);     break;
            case DrawType.Line:      ToLine(afterObj);      break;
            case DrawType.Triangle:  ToMesh(afterObj);      break;
            case DrawType.LineStrip: ToLineStrip(afterObj); break;
            case DrawType.Quad:      ToQuad(beforeObj);     break;
        }
        //変身中スキンに変更
        afterObj.GetComponent<MeshRenderer>().material = cyberSkin;

        //収縮する
        Mesh afterMesh = afterObj.GetComponent<MeshFilter>().mesh;
        Vector3[] afterVertices = afterMesh.vertices;
        Vector3[] defaultAfterVertices = afterMesh.vertices;
        

        //変化後の頂点の位置が自然に見えるように位置調整

        //変身前のほうが頂点数がすくなかったら
        if (beforeVertices.Length < afterVertices.Length)
        {
            for (var i = 0; i < beforeVertices.Length; i++)
            {
                afterVertices[i] = beforeVertices[i];
            }
            for(var i = beforeVertices.Length; i < afterVertices.Length; i++)
            {
                afterVertices[i] = beforeVertices[Random.Range(0,beforeVertices.Length)];
            }
        }
        else //変身後のほうが頂点数がすくなかったら(頂点数一緒の場合はほぼない)
        {
            for (var i = 0; i < afterVertices.Length; i++)
            {
                afterVertices[i] = beforeVertices[i];
            }
            //↓ワンチャンなくても問題なさそう
            for(var i = afterVertices.Length; i < beforeVertices.Length; i++)
            {
                beforeVertices[i] = afterVertices[Random.Range(0, afterVertices.Length)];
            }
            //
        }

        //拡散エフェクト処理
        timer.Restart();
        while (timer.TotalSeconds < changeTime)
        {

            for (var i = 0; i < afterVertices.Length; i++)
            {
                afterVertices[i] += new Vector3(Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f));//全頂点のxとyをランダムでちょっと動かす
            }
            afterMesh.SetVertices(afterVertices);

            yield return null;
        }

        //拡散している頂点→変身後の頂点位置に戻す
        timer.Restart();
        Vector3[] firstPos = afterMesh.vertices;
        while (timer.TotalSeconds < easeTime)
        {
            for (var i = 0; i < afterVertices.Length; i++)
            {
                float x = Easing.QuintOut(timer.TotalSeconds, easeTime, firstPos[i].x, defaultAfterVertices[i].x);
                float y = Easing.QuintOut(timer.TotalSeconds, easeTime, firstPos[i].y, defaultAfterVertices[i].y);
                float z = Easing.QuintOut(timer.TotalSeconds, easeTime, firstPos[i].z, defaultAfterVertices[i].z);
                afterVertices[i] = new Vector3(x, y, z);            
            }
            afterMesh.SetVertices(afterVertices);
            yield return null;
        }
        afterMesh.SetVertices(defaultAfterVertices);
        //調整(完成形を少し長く見せるため)
        yield return new WaitForSeconds(0.01f);

        //メッシュ化
        ToMesh(afterObj);

        //ディゾルブする
        var m_after = afterObj.GetComponent<MeshRenderer>().material = dissolveSkin;
        //ディゾルブ
        timer.Restart();
        while (timer.TotalSeconds < dissolveTime)
        {
            var amount = Easing.Linear(timer.TotalSeconds, dissolveTime, 1, 0);
            m_after.SetFloat("_DisAmount", amount);
            yield return null;
        }
        m_after.SetFloat("_DisAmount", 0.0f);

        //元のテクスチャに差し替える
        m_after = afterObj.GetComponent<MeshRenderer>().material = defaultSkin;

        //RigidBodyと当たり判定つける
        //afterObj.AddComponent<Rigidbody>();
        //afterObj.AddComponent<MeshCollider>().convex=true;

        //テスト用---
        if (isRandom)
        {
            yield return new WaitForSeconds(0.5f);
            canMorf = true;
        } 
        //------------
    }

    #region DrawType
    //点描画に切り替える
    void ToPoint(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Points, 0);    
    }

    //ライン描画に切り替える
    void ToLine(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Lines, 0);
    }

    //メッシュ描画に切り替える
    void ToMesh(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Triangles, 0);
    }

    //ラインストリップ描画に切り替える
    void ToLineStrip(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.LineStrip, 0);
    }

    //クアッド描画に切り替える
    void ToQuad(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Quads, 0);
    }
    #endregion
}
