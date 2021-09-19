using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//@Author:Nagani-syon
//
//���[�t�B���O����
//
//��ToFix
//�E�������Ⴄ����(posy)�����[�t�B���O�����Ƃ��ɃK�N�b�ĂȂ遨���_�̈ʒu�����[�J����̂��߃��[���h��ɕύX����Ɖ����ł�����
//�E�ϐg�O�ƕϐg�オ�������f�����Ƒ����o�O�遨�������f�����͓������_���̎��ɗ�O�����������Ă�����Ɖ����ł�����
//�E�O���������ω�����C�[�W���O��I�Ԃƈ�a�����ł遨���܂����Ƃ��܂��������Ȃ�


public class VertexChange : MonoBehaviour
{
    #region Setting
    [Header("�ϐg�O�I�u�W�F�N�g")]
    [SerializeField] GameObject beforeObj = null;
    [Header("�ϐg��I�u�W�F�N�g")]
    [SerializeField] GameObject afterObj = null;
    [Header("�ϐg����")]
    [SerializeField] float changeTime = 0.5f;
    [Header("�ϐg��C�[�W���O����")]
    [SerializeField] float easeTime = 2.0f;
    [Header("�f�B�]���u����")]
    [SerializeField] float dissolveTime = 0.2f;

    [Header("�f�t�H���g�X�L��")]
    [SerializeField] Material defaultSkin = null;
    [Header("�f�B�]���u�X�L��")]
    [SerializeField] Material dissolveSkin = null;
    [Header("�ϐg���X�L��")]
    [SerializeField] Material cyberSkin = null;

    private enum DrawType
    {
        Point,Line,Triangle,LineStrip,Quad
    }
    [Header("�ϐg���`��^�C�v")]
    [SerializeField] DrawType drawType;

    //�e�X�g�p-------------------------------
    [Header("�e�X�g�p�����_���\���p���X�g")]
    [SerializeField] List<GameObject> Actors;
    [Header("�����_���\��ON/OFF")]
    [SerializeField] bool isRandom=false;
    bool canMorf = true;
    //---------------------------------------
    #endregion

    #region Start
    void Start()
    {    
        //�e�X�g�p---
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
        if (isRandom)//�e�X�g�p----
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
        ////�f�B�]���u����
        var m_before = beforeObj.GetComponent<MeshRenderer>().material = dissolveSkin;
        //�f�B�]���u
        ProcessTimer timer = new ProcessTimer(); timer.Restart();
        while (timer.TotalSeconds < dissolveTime)
        {
            var amount = Easing.Linear(timer.TotalSeconds, dissolveTime, 0, 1);
            m_before.SetFloat("_DisAmount", amount);
            yield return null;
        }
        m_before.SetFloat("_DisAmount", 1.0f);

        //�`��^�C�v�ɂ���ĕ\����ς���
        switch (drawType)
        {
            case DrawType.Point:     ToPoint(beforeObj);     break;
            case DrawType.Line:      ToLine(beforeObj);      break;
            case DrawType.Triangle:  ToMesh(beforeObj);      break;
            case DrawType.LineStrip: ToLineStrip(beforeObj); break;
            case DrawType.Quad:      ToQuad(beforeObj);      break;
        }
        //�ϐg���X�L���ɕύX
        beforeObj.GetComponent<MeshRenderer>().material = cyberSkin;

        //�g�U����
        Mesh beforeMesh=beforeObj.GetComponent<MeshFilter>().mesh;
        Vector3[] beforeVertices=beforeMesh.vertices;
        Vector3[] defaultBeforeVertices = beforeMesh.vertices;

        yield return new WaitForSeconds(0.4f);

        //�g�U�G�t�F�N�g����
        timer.Restart();
        while (timer.TotalSeconds < changeTime)
        {
            for (var i = 0; i < beforeVertices.Length; i++)
            {
                beforeVertices[i] += new Vector3(Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f));//�S���_��x��y�������_���ł�����Ɠ�����
            }
            beforeMesh.SetVertices(beforeVertices);
            yield return null;
        }
        beforeMesh.SetVertices(defaultBeforeVertices);

        //���f������ւ�------------------------���ϐg�O�I�u�W�F�N�g�ւ̏���
        beforeObj.SetActive(false);      
        afterObj.SetActive(true);
        //--------------------------------------���ϐg��I�u�W�F�N�g�ւ̏���

        //�`��^�C�v�ɂ���ĕ\����ς���
        switch (drawType)
        {
            case DrawType.Point:     ToPoint(afterObj);     break;
            case DrawType.Line:      ToLine(afterObj);      break;
            case DrawType.Triangle:  ToMesh(afterObj);      break;
            case DrawType.LineStrip: ToLineStrip(afterObj); break;
            case DrawType.Quad:      ToQuad(beforeObj);     break;
        }
        //�ϐg���X�L���ɕύX
        afterObj.GetComponent<MeshRenderer>().material = cyberSkin;

        //���k����
        Mesh afterMesh = afterObj.GetComponent<MeshFilter>().mesh;
        Vector3[] afterVertices = afterMesh.vertices;
        Vector3[] defaultAfterVertices = afterMesh.vertices;
        

        //�ω���̒��_�̈ʒu�����R�Ɍ�����悤�Ɉʒu����

        //�ϐg�O�̂ق������_���������Ȃ�������
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
        else //�ϐg��̂ق������_���������Ȃ�������(���_���ꏏ�̏ꍇ�͂قڂȂ�)
        {
            for (var i = 0; i < afterVertices.Length; i++)
            {
                afterVertices[i] = beforeVertices[i];
            }
            //�������`�����Ȃ��Ă����Ȃ�����
            for(var i = afterVertices.Length; i < beforeVertices.Length; i++)
            {
                beforeVertices[i] = afterVertices[Random.Range(0, afterVertices.Length)];
            }
            //
        }

        //�g�U�G�t�F�N�g����
        timer.Restart();
        while (timer.TotalSeconds < changeTime)
        {

            for (var i = 0; i < afterVertices.Length; i++)
            {
                afterVertices[i] += new Vector3(Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f), Random.Range(-0.005f, 0.005f));//�S���_��x��y�������_���ł�����Ɠ�����
            }
            afterMesh.SetVertices(afterVertices);

            yield return null;
        }

        //�g�U���Ă��钸�_���ϐg��̒��_�ʒu�ɖ߂�
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
        //����(�����`���������������邽��)
        yield return new WaitForSeconds(0.01f);

        //���b�V����
        ToMesh(afterObj);

        //�f�B�]���u����
        var m_after = afterObj.GetComponent<MeshRenderer>().material = dissolveSkin;
        //�f�B�]���u
        timer.Restart();
        while (timer.TotalSeconds < dissolveTime)
        {
            var amount = Easing.Linear(timer.TotalSeconds, dissolveTime, 1, 0);
            m_after.SetFloat("_DisAmount", amount);
            yield return null;
        }
        m_after.SetFloat("_DisAmount", 0.0f);

        //���̃e�N�X�`���ɍ����ւ���
        m_after = afterObj.GetComponent<MeshRenderer>().material = defaultSkin;

        //RigidBody�Ɠ����蔻�����
        //afterObj.AddComponent<Rigidbody>();
        //afterObj.AddComponent<MeshCollider>().convex=true;

        //�e�X�g�p---
        if (isRandom)
        {
            yield return new WaitForSeconds(0.5f);
            canMorf = true;
        } 
        //------------
    }

    #region DrawType
    //�_�`��ɐ؂�ւ���
    void ToPoint(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Points, 0);    
    }

    //���C���`��ɐ؂�ւ���
    void ToLine(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Lines, 0);
    }

    //���b�V���`��ɐ؂�ւ���
    void ToMesh(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Triangles, 0);
    }

    //���C���X�g���b�v�`��ɐ؂�ւ���
    void ToLineStrip(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.LineStrip, 0);
    }

    //�N�A�b�h�`��ɐ؂�ւ���
    void ToQuad(GameObject obj)
    {
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Quads, 0);
    }
    #endregion
}
